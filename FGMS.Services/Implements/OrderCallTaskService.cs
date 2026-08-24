using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Mx.Services;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FGMS.Services.Implements
{
    public class OrderCallTaskService : BackgroundService, IOrderCallTaskService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<OrderCallTaskService> logger;
        private readonly GenerateRandomNumber randomNumber;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly SemaphoreSlim timerSemaphore = new(1, 1);
        private readonly SemaphoreSlim workSemaphore = new(1, 1);
        private readonly CancellationTokenSource stopTokenSource = new();
        private readonly string baseUrl;

        private static readonly List<ProductionOrder> retryPoList = new();

        private PeriodicTimer? timer;
        private TimerSettings currentSettings;
        private bool isDisposed;
        private CancellationTokenSource? heartbeatCts; // 心跳任务的独立取消令牌

        public OrderCallTaskService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<OrderCallTaskService> logger,
            GenerateRandomNumber randomNumber,
            IHttpClientFactory httpClientFactory,
            ConfigHelper configHelper,
            IOptionsMonitor<TimerSettings> timerSettingsMonitor)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.randomNumber = randomNumber;
            this.httpClientFactory = httpClientFactory;
            currentSettings = timerSettingsMonitor.CurrentValue;
            timerSettingsMonitor.OnChange(OnSettingsChanged);
            baseUrl = configHelper.GetAppSettings<string>("SelfUrl") ?? "http://localhost:5000";
        }

        private void OnSettingsChanged(TimerSettings newSettings)
        {
            logger.LogInformation("TimerSettings changed: IntervalMinutes={interval}, Enabled={enabled}", newSettings.IntervalMinutes, newSettings.Enabled);
            Task.Run(async () => await ApplySettingsChangeAsync(newSettings));
        }

        private async Task ApplySettingsChangeAsync(TimerSettings newSettings)
        {
            await timerSemaphore.WaitAsync();
            try
            {
                currentSettings = newSettings;
                var oldTimer = timer;
                timer = null;
                if (newSettings.Enabled)
                {
                    timer = new PeriodicTimer(TimeSpan.FromMinutes(newSettings.IntervalMinutes));
                    logger.LogInformation("Timer restarted with new settings.");
                }
                oldTimer?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "应用设置变更时出错");
            }
            finally
            {
                timerSemaphore.Release();
            }
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            logger.LogInformation("============================开始执行定时任务============================");

            using var scope = serviceScopeFactory.CreateScope();
            var productionOrderRepository = scope.ServiceProvider.GetRequiredService<IProductionOrderRepository>();
            var materialIssueOrderRepository = scope.ServiceProvider.GetRequiredService<IMaterialIssueOrderRepository>();
            var wheelWorkOrderRepository = scope.ServiceProvider.GetRequiredService<IWorkOrderRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<IFgmsDbContext>();

            try
            {
                #region 1.处理可用制令单
                await dbContext.BeginTrans();
                // 查询所有已排配未报工的制令单
                var productOrderSources = await productionOrderRepository.GetListAsync(
                    expression: src => src.Status != ProductionOrderStatus.已暂停,
                    include: src => src.Include(src => src.Equipment!).Include(src => src.WorkOrder!),
                    asNoTracking: false);

                if (!productOrderSources.Any())
                {
                    logger.LogInformation("没有符合条件的制令单");
                    return;
                }

                var productOrders = productOrderSources.ToList();

                // 按设备分组并获取可用的制令单
                var equipmentGroups = productOrders.GroupBy(src => src.Equipment!.Code);
                var availableOrders = GetAvailableOrders(equipmentGroups, cancellationToken);

                if (availableOrders == null || !availableOrders.Any())
                {
                    logger.LogInformation("没有可处理的制令单");
                    return;
                }

                string availableOrderNos = string.Join(", ", availableOrders.Select(src => src.OrderNo));
                logger.LogInformation("发现 {count} 条可叫料制令单：{orderNos}", availableOrders.Count, availableOrderNos);
                #endregion

                #region 2.创建物料发料单
                // 如重试列表中有数据，合并到本次处理的订单中，避免漏单
                availableOrders = MergeRetryOrders(availableOrders);
                var materialIssueOrders = CreateMaterialIssueOrdersAsync(availableOrders, scope);
                if (!materialIssueOrders.Any())
                    return;

                bool is_successed = materialIssueOrderRepository.AddEntity(materialIssueOrders);
                if (!is_successed)
                {
                    logger.LogWarning("创建物料发料单失败");
                    return;
                }

                availableOrders.ForEach(src => src.Status = ProductionOrderStatus.待发料);
                bool success = productionOrderRepository.UpdateEntity(availableOrders, new Expression<Func<ProductionOrder, object>>[] { src => src.Status });
                int productionSaveCount = await dbContext.SaveChangesAsync();
                if (!success || productionSaveCount == 0)
                {
                    logger.LogWarning("更新制令单状态失败");
                    return;
                }
                logger.LogInformation("创建 {count} 条发料单，更新 {orderCount} 条制令单状态为'待发料'", materialIssueOrders.Count, availableOrders.Count);
                #endregion

                #region 3.创建砂轮工单
                // 更新可叫料的制令单状态为待发料
                productOrders.Where(src => availableOrders.Any(dest => dest.Id == src.Id)).ToList().ForEach(src => src.Status = ProductionOrderStatus.待发料);

                // 获取标记需要申请砂轮的制令单
                var finalOrders = GetMarkCallableWheelOrders(productOrders);

                if (finalOrders.Any())
                {
                    // 添加砂轮工单
                    var result = GetRequireAddWheelOrders(finalOrders);
                    var addWorkOrders = result.workOrders;
                    var dict = result.dict;

                    var successed = wheelWorkOrderRepository.AddEntity(addWorkOrders);
                    var whellSaveCount = await dbContext.SaveChangesAsync();
                    if (!successed || whellSaveCount <= 0)
                    {
                        logger.LogWarning("创建砂轮工单失败，或无砂轮工单被保存");
                        return;
                    }

                    // 更新制令单关联的砂轮工单Id
                    finalOrders.ForEach(src => src.WorkOrderId = addWorkOrders.FirstOrDefault(dest => dest.OrderNo.Equals(dict.GetValueOrDefault(src.OrderNo)))!.Id);
                    bool updateWheelResult = productionOrderRepository.UpdateEntity(finalOrders, new Expression<Func<ProductionOrder, object>>[] { src => src.WorkOrderId });
                    int updateWheelCount = await dbContext.SaveChangesAsync();
                    if (!updateWheelResult || updateWheelCount == 0)
                    {
                        logger.LogWarning("更新制令单的砂轮工单关联失败");
                        return;
                    }
                }
                #endregion

                #region 4.提交更改
                await dbContext.CommitTrans();

                string loggerInfomation = $"创建砂轮工单 {finalOrders.Count} 条";
                if (finalOrders.Any(src => src.IsDc.GetValueOrDefault()))
                    loggerInfomation += $"，其中段差制令单的砂轮工单 {finalOrders.Count(src => src.IsDc.GetValueOrDefault())} 条";

                logger.LogInformation(loggerInfomation);
                var duration = DateTime.Now - startTime;
                logger.LogInformation("定时任务执行完成，耗时: {duration:0}ms", duration.TotalMilliseconds);
                #endregion
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("操作被取消，执行回滚");
                await dbContext.RollBackTrans();
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "执行具体任务时出错");
                await dbContext.RollBackTrans();
                throw;
            }
        }

        // 心跳方法
        private async Task RunHeartbeatAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("心跳服务启动，间隔: {interval}分钟", currentSettings.HeartbeatInterva);

            // 延迟10秒开始，确保应用完全启动
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("心跳服务在启动延迟期间被取消，立即停止");
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await SendHeartbeatAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("心跳发送被取消，立即停止");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "心跳发送失败");
                }

                // 每 {interval} 分钟发送一次心跳
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(currentSettings.HeartbeatInterva), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation("心跳等待被取消，立即停止");
                    break;
                }
            }

            logger.LogInformation("心跳服务停止");
        }

        // 健康检查接口调用
        private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
        {
            using var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            try
            {
                var response = await client.GetAsync($"{baseUrl}/fgms/android/health/ping", cancellationToken);
                if (response.IsSuccessStatusCode)
                    logger.LogDebug("心跳成功");
                else
                    logger.LogWarning("心跳失败: {statusCode}", response.StatusCode);
            }
            catch (OperationCanceledException)
            {
                // 正常取消，重新抛出以便上层处理
                throw;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "心跳异常");
            }
        }

        // 获取可用的制令单，每台设备的工时小于24小时都可叫料
        private static List<ProductionOrder> GetAvailableOrders(IEnumerable<IGrouping<string, ProductionOrder>> groups, CancellationToken cancellationToken)
        {
            var availableOrders = new List<ProductionOrder>();
            foreach (var group in groups)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 处理前制程的制令单，设备代码为 DCJGZ 的订单
                var qzcOrders = group.Where(src => src.Equipment!.Code.Equals("DCJGZ") && src.Status == ProductionOrderStatus.已排配).OrderBy(src => src.Id).ToList();
                if (qzcOrders.Any())
                {
                    availableOrders.AddRange(qzcOrders);
                }

                // 计算已在生产或待处理的工时
                double currentWorkHours = group
                    .Where(src => (src.Status == ProductionOrderStatus.生产中 || src.Status == ProductionOrderStatus.已收料 || src.Status == ProductionOrderStatus.待发料) && src.Equipment!.Code != "DCJGZ")
                    .Sum(src => src.WorkHours ?? 0);

                if (currentWorkHours < 24)
                {
                    // 获取已排配的订单（按优先级排序）
                    var scheduledOrders = group.Where(src => src.Status == ProductionOrderStatus.已排配 && src.IsDc!.Value == false).OrderBy(src => src.Id).ToList();

                    if (scheduledOrders.Any())
                    {
                        // 处理第一个订单（强制加入）
                        var firstOrder = scheduledOrders.First();
                        availableOrders.Add(firstOrder);
                        currentWorkHours += firstOrder.WorkHours ?? 0;

                        // 处理剩余订单（按规则判断）
                        foreach (var order in scheduledOrders.Skip(1))
                        {
                            double orderHours = order.WorkHours ?? 0;
                            if (currentWorkHours + orderHours < 24)
                            {
                                availableOrders.Add(order);
                                currentWorkHours += orderHours;
                            }
                            else
                                break;
                        }
                    }
                }
            }
            return availableOrders;
        }

        // 标记可呼叫砂轮工单的制令单
        private static List<ProductionOrder> GetMarkCallableWheelOrders(List<ProductionOrder> srcOrder)
        {
            // 处理非段差制令单
            var groupData1 = srcOrder.Where(src => src.Equipment!.Code != "DCJGZ").GroupBy(src => src.Equipment!.Code);
            var ordersRequireWheel = ProcessProdcutOrdersCallableByWheel(groupData1);

            // 处理段差制令单
            var krCods = srcOrder.Where(src => src.Equipment!.Code == "DCJGZ" && src.KrCode != null).Select(src => src.KrCode).Distinct();
            var groupData2 = srcOrder.Where(src => krCods.Contains(src.KrCode)).GroupBy(src => src.KrCode)!;
            var dcOrdersRequireWheel = ProcessProdcutOrdersCallableByWheel(groupData2!);

            // 合并需要申请砂轮的制令单
            var callbleOrders = ordersRequireWheel.Concat(dcOrdersRequireWheel).ToList();
            return callbleOrders;
        }

        // 确定哪些制令单需要申请砂轮
        private static List<ProductionOrder> ProcessProdcutOrdersCallableByWheel(IEnumerable<IGrouping<string, ProductionOrder>> groupData)
        {
            var result = new List<ProductionOrder>();
            // 按设备分组处理
            foreach (var data in groupData)
            {
                // 获取当前设备所有制令单
                var allOrders = data.ToList();

                // 获取制令当关联（未驳回、未结束、未退仓配送）的砂轮工单的成品料号
                //var existingFinishCodes = allOrders.Where(src =>
                //        src.WorkOrder != null &&
                //        src.WorkOrder.Pid == null &&
                //        src.WorkOrder.Status != WorkOrderStatus.待审 &&
                //        src.WorkOrder.Status != WorkOrderStatus.驳回 &&
                //        src.WorkOrder.Status != WorkOrderStatus.退仓配送 &&
                //        src.WorkOrder.Status != WorkOrderStatus.工单结束)
                //    .SelectMany(src => src.WorkOrder!.ProductionOrders!)
                //    .Select(src => src!.FinishCode)
                //    .Where(code => code != null)
                //    .Distinct()
                //    .ToHashSet();

                var existingFinishCodes = allOrders.Where(src =>
                        src.WorkOrder != null &&
                        src.WorkOrder.Pid == null &&
                        src.WorkOrder.Status != WorkOrderStatus.待审 &&
                        src.WorkOrder.Status != WorkOrderStatus.驳回 &&
                        src.WorkOrder.Status != WorkOrderStatus.退仓配送 &&
                        src.WorkOrder.Status != WorkOrderStatus.工单结束)
                    .Select(src => src.WorkOrder)
                    .Select(src => src!.MaterialNo)
                    .Where(code => code != null)
                    .Distinct()
                    .ToHashSet();

                // 处理待发料且未处理砂轮申请的制令单
                var waitingOrders = allOrders.FindAll(order => !order.RequireWheel && order.Status == ProductionOrderStatus.待发料);
                foreach (var order in waitingOrders)
                {
                    // 检查生产列表的成品料号，如当前制令单的成品料号不在生产的列表中，则需要申请砂轮
                    if (!existingFinishCodes.Contains(order.FinishCode))
                    {
                        order.RequireWheel = true;  // 标记需要砂轮
                        existingFinishCodes.Add(order.FinishCode);
                        result.Add(order);
                    }
                }
            }
            return result;
        }

        // 创建物料发料单
        private List<MaterialIssueOrder> CreateMaterialIssueOrdersAsync(List<ProductionOrder> orders, IServiceScope scope)
        {
            var businessService = scope.ServiceProvider.GetRequiredService<IBusinessService>();
            var randomNumber = scope.ServiceProvider.GetRequiredService<GenerateRandomNumber>();

            var materialIssueOrders = new List<MaterialIssueOrder>();

            foreach (var order in orders.Where(src => src.Status == ProductionOrderStatus.已排配))
            {
                var storagePosition = businessService.GetStoragePositionsAsync(order.OrderNo).Result;

                if (storagePosition is null)
                {
                    logger.LogInformation("制令单 {orderNo} 的物料 {materialCode} 没有找到对应的库位或出库信息，已加入重试列表", order.OrderNo, order.MaterialCode);
                    retryPoList.Add(order);
                    continue;
                }

                var materialIssueOrder = new MaterialIssueOrder
                {
                    ProductionOrderId = order.Id,
                    CreateorId = 1,
                    Type = MioType.发料,
                    OrderNo = $"MIO{randomNumber.CreateOrderNum()}",
                    MaterialNo = order.MaterialCode,
                    MaterialName = order.MaterialName,
                    MaterialSpce = order.MaterialSpec,
                    Quantity = order.Quantity,
                    MxWareHouse = storagePosition?.Warehouse,
                    MxCargoSpace = storagePosition?.CargoSpace,
                    MxOutStoreOrderNo = storagePosition?.OutStoreOrderCode
                };
                materialIssueOrders.Add(materialIssueOrder);
            }
            return materialIssueOrders;
        }

        // 创建砂轮工单
        private (List<WorkOrder> workOrders, Dictionary<string, string> dict) GetRequireAddWheelOrders(List<ProductionOrder> productionOrders)
        {
            var wheelWorkOrders = new List<WorkOrder>();
            var dict = new Dictionary<string, string>();
            foreach (var order in productionOrders)
            {
                var wheelWorkOrder = new WorkOrder
                {
                    UserInfoId = 1,
                    OrderNo = $"WO{randomNumber.CreateOrderNum()}",
                    Type = WorkOrderType.砂轮申领,
                    Priority = WorkOrderPriority.低,
                    MaterialNo = order.FinishCode,
                    MaterialSpec = order.FinishSpec,
                    Status = WorkOrderStatus.待审,
                    CreateDate = DateTime.Now,
                    RequiredDate = order.PlannedBeginTime!.Value,
                    AgvTaskCode = Guid.NewGuid().ToString("N")[..16],
                    PreAllocationEquipmentCode = order.KrCode   // 预分配设备代码
                };
                wheelWorkOrders.Add(wheelWorkOrder);
                dict[order.OrderNo] = wheelWorkOrder.OrderNo;
            }
            return (wheelWorkOrders, dict);
        }

        // 将重试列表中的订单合并到本次处理的订单中，避免漏单
        private List<ProductionOrder> MergeRetryOrders(List<ProductionOrder> availableOrders)
        {
            if (!retryPoList.Any())
                return availableOrders;

            string retryOrderNos = string.Join(", ", retryPoList.Select(src => src.OrderNo));
            logger.LogInformation("重试列表中有 {count} 个制令单：{orderNos}，合并到本次处理", retryPoList.Count, retryOrderNos);
            var retryIds = retryPoList.Select(src => src.Id).ToHashSet();

            // 移除已存在的
            availableOrders.RemoveAll(src => retryIds.Contains(src.Id));
            // 添加重试的
            availableOrders.AddRange(retryPoList);
            retryPoList.Clear();

            return availableOrders;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("启动定时任务");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("停止定时任务");

            // 立即取消所有任务（包括心跳任务）
            stopTokenSource.Cancel();

            // 如果心跳任务有独立的取消令牌，也立即取消
            heartbeatCts?.Cancel();

            // 停止定时器，防止新任务启动
            await timerSemaphore.WaitAsync();
            try
            {
                var oldTimer = timer;
                timer = null;
                oldTimer?.Dispose();
            }
            finally
            {
                timerSemaphore.Release();
            }

            // 不等待任何任务完成，立即返回
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 创建心跳任务的独立取消令牌
            heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, stopTokenSource.Token);
            var heartbeatToken = heartbeatCts.Token;

            // 启动心跳任务（不等待）
            _ = Task.Run(async () => await RunHeartbeatAsync(heartbeatToken), heartbeatToken);
            PeriodicTimer? localTimer = null;
            await timerSemaphore.WaitAsync();
            try
            {
                if (!currentSettings.Enabled)
                {
                    logger.LogInformation("定时任务未配置启动");
                    return;
                }
                localTimer = new PeriodicTimer(TimeSpan.FromMinutes(currentSettings.IntervalMinutes));
            }
            finally
            {
                timerSemaphore.Release();
            }
            logger.LogInformation("定时任务运行中，间隔: {interval} 分钟", currentSettings.IntervalMinutes);
            try
            {
                while (!stoppingToken.IsCancellationRequested && !stopTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        if (localTimer != null && await localTimer.WaitForNextTickAsync(stoppingToken))
                        {
                            if (await workSemaphore.WaitAsync(0, stoppingToken))
                            {
                                try
                                {
                                    if (currentSettings.Enabled)
                                    {
                                        await DoWorkAsync(stoppingToken);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "工作任务执行出错");
                                }
                                finally
                                {
                                    workSemaphore.Release();
                                }
                            }
                            else
                            {
                                logger.LogWarning("上一次任务尚未完成，跳过本次执行");
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("定时任务服务被取消");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "定时任务执行出错");
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                localTimer?.Dispose();
                // 取消心跳任务（立即停止，不等待）
                if (heartbeatCts != null)
                {
                    heartbeatCts.Cancel();
                    heartbeatCts.Dispose();
                    heartbeatCts = null;
                }
            }
        }

        public override void Dispose()
        {
            if (!isDisposed)
            {
                logger.LogInformation("开始释放 OrderCallTaskService 资源");

                // 取消所有任务
                stopTokenSource.Cancel();

                // 取消心跳任务
                if (heartbeatCts != null)
                {
                    heartbeatCts.Cancel();
                    heartbeatCts.Dispose();
                    heartbeatCts = null;
                }

                stopTokenSource.Dispose();
                timerSemaphore?.Dispose();
                workSemaphore?.Dispose();

                var oldTimer = timer;
                timer = null;
                oldTimer?.Dispose();

                isDisposed = true;
                logger.LogInformation("OrderCallTaskService 资源释放完成");
            }
            base.Dispose();
        }
    }
}
