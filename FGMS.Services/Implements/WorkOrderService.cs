using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FGMS.Services.Implements
{
    internal class WorkOrderService : BaseService<WorkOrder>, IWorkOrderService
    {
        private readonly IWorkOrderRepository? workOrderRepository;
        private readonly IFgmsDbContext? fgmsDbContext;
        private readonly IComponentRepository? componentRepository;
        private readonly IElementEntityRepository? elementEntityRepository;
        private readonly ICargoSpaceRepository? cargoSpaceRepositroy;
        private readonly IWorkOrderStandardRepository? workOrderStandardRepository;
        private readonly IEquipmentRepository? equipmentRepository;
        private readonly IComponentLogRepository? componentLogRepository;
        private readonly IProductionOrderRepository? productionOrderRepository;
        private readonly ITrackLogRepository? logRepository;

        public WorkOrderService(IBaseRepository<WorkOrder> repo, IFgmsDbContext context) : base(repo, context)
        {
        }

        public WorkOrderService(
            IBaseRepository<WorkOrder> repo,
            IFgmsDbContext context,
            IComponentRepository? componentRepository,
            IElementEntityRepository? elementEntityRepository,
            ICargoSpaceRepository? cargoSpaceRepositroy,
            IWorkOrderStandardRepository? workOrderStandardRepository,
            IEquipmentRepository? equipmentRepository,
            IComponentLogRepository? componentLogRepository,
            IProductionOrderRepository? productionOrderRepository,
            ITrackLogRepository? logRepository) : base(repo, context)
        {
            workOrderRepository = repo as IWorkOrderRepository;
            fgmsDbContext = context;
            this.componentRepository = componentRepository;
            this.elementEntityRepository = elementEntityRepository;
            this.cargoSpaceRepositroy = cargoSpaceRepositroy;
            this.workOrderStandardRepository = workOrderStandardRepository;
            this.equipmentRepository = equipmentRepository;
            this.componentLogRepository = componentLogRepository;
            this.productionOrderRepository = productionOrderRepository;
            this.logRepository = logRepository;
        }

        public async Task<dynamic> ReceiveAsync(dynamic paramJson)
        {
            await fgmsDbContext!.BeginTrans();
            try
            {
                int woId = paramJson!.woId;
                var orderEntity = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId);

                if (orderEntity == null)
                    return new { success = false, message = "未知工单" };

                var expression = new List<Expression<Func<WorkOrder, object>>> { src => src.Status };
                switch (orderEntity.Type)
                {
                    case WorkOrderType.砂轮返修:
                        orderEntity.Status = WorkOrderStatus.参数修整;
                        orderEntity.AgvTaskCode = Guid.NewGuid().ToString("N")[..16];
                        expression.Add(src => src.AgvTaskCode);
                        break;
                    case WorkOrderType.砂轮退仓:
                        //入库
                        var cmps = await componentRepository!.GetListAsync(expression: src => src.WorkOrderId == orderEntity.Id, include: src => src.Include(src => src.ElementEntities!));
                        foreach (var cmp in cmps)
                        {
                            if (!cmp.CargoSpaceHistory.HasValue)
                            {
                                return cmp.IsStandard ? new { success = false, message = $"标组：{cmp.Code} 未找到相关货位，入库失败" } : new { success = false, message = $"非标组未找到相关货位，入库失败" };
                            }
                            cmp.CargoSpaceId = cmp.CargoSpaceHistory.Value;
                            cmp.WorkOrderId = null;
                            cmp.Status = ElementEntityStatus.在库;
                            componentRepository.UpdateEntity(cmp, new Expression<Func<Component, object>>[] { src => src.Status, src => src.CargoSpaceId, src => src.WorkOrderId });
                            //更新工件状态
                            var ees = cmp.ElementEntities!.ToList();
                            ees.ForEach(ee =>
                            {
                                ee.CargoSpaceId = cmp.CargoSpaceId;
                                ee.Status = ElementEntityStatus.在库;
                            });
                            elementEntityRepository!.UpdateEntity(ees, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.CargoSpaceId! });
                        }
                        orderEntity.Status = WorkOrderStatus.工单结束;
                        break;
                    default: break;
                }
                workOrderRepository.UpdateEntity(orderEntity, expression.ToArray());
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }

        public async Task<dynamic> CancelAsync(int orderId)
        {
            var order = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == orderId, include: src => src.Include(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            if (order == null)
                return new { success = false, message = "工单不存在" };

            if (order.Status == WorkOrderStatus.待审 || order.Status == WorkOrderStatus.审核通过 || order.Status == WorkOrderStatus.砂轮整备 || order.Status == WorkOrderStatus.参数修整 || order.Status == WorkOrderStatus.整备完成)
            {
                await fgmsDbContext!.BeginTrans();
                try
                {
                    if (order.Components != null && order.Components.Any())
                    {
                        foreach (var cmp in order.Components)
                        {
                            if (cmp.IsStandard)
                            {
                                if (!cmp.CargoSpaceHistory.HasValue)
                                {
                                    return new { success = false, message = $"标组：{cmp.Code}未找到相关货位，入库失败" };
                                }
                                cmp.CargoSpaceId = cmp.CargoSpaceHistory.Value;
                                cmp.WorkOrderId = new int?();
                                cmp.Status = ElementEntityStatus.在库;
                                componentRepository!.UpdateEntity(cmp, new Expression<Func<Component, object>>[] { src => src.Status, src => src.CargoSpaceId!, src => src.WorkOrderId! });

                                if (cmp.ElementEntities == null) continue;
                                var ees = cmp.ElementEntities.ToList();
                                ees.ForEach(ee =>
                                {
                                    ee.CargoSpaceId = cmp.CargoSpaceId;
                                    ee.Status = ElementEntityStatus.在库;
                                });
                                elementEntityRepository!.UpdateEntity(ees, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.CargoSpaceId! });
                            }
                            else
                            {
                                if (cmp.ElementEntities == null) continue;
                                foreach (var ee in cmp.ElementEntities)
                                {
                                    if (!ee.CargoSpaceHistory.HasValue)
                                    {
                                        return new { success = false, message = $"工件：{ee.MaterialNo}未找到相关货位，入库失败" };
                                    }
                                    ee.ComponentId = new int?();
                                    ee.CargoSpaceId = ee.CargoSpaceHistory.Value;
                                    ee.IsGroup = false;
                                    ee.Status = ElementEntityStatus.在库;
                                    elementEntityRepository!.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.CargoSpaceId!, src => src.ComponentId!, src => src.IsGroup });
                                }
                                componentRepository!.DeleteEntity(cmp);
                            }
                        }
                    }
                    order.Status = WorkOrderStatus.取消;
                    workOrderRepository.UpdateEntity(order, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                    bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                    if (success)
                        await fgmsDbContext.CommitTrans();
                    else
                        await fgmsDbContext.RollBackTrans();
                    return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
                }
                catch (Exception ex)
                {
                    await fgmsDbContext.RollBackTrans();
                    throw new Exception("Error Updating Order Status" + ex.Message);
                }
            }
            else
                return new { success = false, message = "无法取消，工单已进入生产流程。请按退仓流程操作" };
        }

        public async Task<dynamic> ReadyAsync(dynamic paramJson)
        {
            int woId = paramJson.woId;
            var order = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId);

            if (order is null)
                return new { success = false, message = "未知工单" };

            await fgmsDbContext!.BeginTrans();
            try
            {
                //添加标准组
                int[] stdCmpIds = JsonConvert.DeserializeObject<int[]>(paramJson.stdCmpIds.ToString());
                if (stdCmpIds.Length > 0)
                {
                    var stdCmps = await componentRepository!.GetListAsync(expression: src => stdCmpIds.Contains(src.Id), include: src => src.Include(src => src.ElementEntities!));
                    foreach (var cmp in stdCmps)
                    {
                        if (cmp.Status != ElementEntityStatus.在库)
                            return new { success = false, message = $"标准组：{cmp.Code} 状态异常！" };

                        //修改标准组工件状态
                        foreach (var ee in cmp.ElementEntities!)
                        {
                            if (ee.Status != ElementEntityStatus.在库)
                                return new { success = false, message = $"标准组工件：{ee.Code} 状态异常！" };

                            ee.CargoSpaceId = new int?();
                            ee.Status = ElementEntityStatus.出库;
                            elementEntityRepository!.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[] { src => src.CargoSpaceId!, src => src.Status });
                        }
                        //修改标准组状态
                        cmp.WorkOrderId = woId;
                        cmp.CargoSpaceId = new int?();
                        cmp.Status = ElementEntityStatus.出库;
                        componentRepository.UpdateEntity(cmp, new Expression<Func<Component, object>>[] { src => src.WorkOrderId!, src => src.CargoSpaceId!, src => src.Status });
                    }
                }
                //添加非标组
                List<dynamic> nonStdCmps = JsonConvert.DeserializeObject<List<dynamic>>(paramJson.nonStdCmps.ToString());
                if (nonStdCmps != null && nonStdCmps.Count > 0)
                {
                    foreach (var cmp in nonStdCmps)
                    {
                        List<dynamic> eeIds = JsonConvert.DeserializeObject<List<dynamic>>(cmp.eeIds.ToString());
                        var intIds = eeIds.Select(src => (int)src.id).ToArray();
                        var ees = await elementEntityRepository!.GetListAsync(expression: src => intIds.Contains(src.Id));
                        foreach (var ee in ees)
                        {
                            if (ee.Status != ElementEntityStatus.在库)
                                return new { success = false, message = $"非标组工件：{ee.Code} 状态异常！" };
                            ee.ComponentId = cmp.cmpId;
                            ee.IsGroup = true;
                            ee.CargoSpaceId = new int?();
                            ee.Status = ElementEntityStatus.出库;
                            ee.Position = eeIds.FirstOrDefault(src => (int)src.id == ee.Id)!.position.ToString();
                            elementEntityRepository.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[]
                            {
                                src => src.ComponentId!,
                                src => src.IsGroup,
                                src => src.CargoSpaceId!,
                                src => src.Status,
                                src => src.Position
                            });
                        }
                    }
                }
                order.Status = WorkOrderStatus.参数修整;
                workOrderRepository.UpdateEntity(order, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }

        public async Task<dynamic> RenovatedAsync(ElementEntity entity, string workOrderNo, int renovateorId)
        {
            if (entity.Status != ElementEntityStatus.出库)
                entity.Status = ElementEntityStatus.出库;

            var order = await workOrderRepository!.GetEntityAsync(expression: src => src.OrderNo == workOrderNo);

            if (order is null)
                return new { success = false, message = "未知工单" };

            await fgmsDbContext!.BeginTrans();
            try
            {
                if (order.RenovateorId == null)
                {
                    order.RenovateorId = renovateorId;
                    workOrderRepository.UpdateEntity(order, new Expression<Func<WorkOrder, object>>[] { src => src.RenovateorId! });
                }

                elementEntityRepository!.UpdateEntity(entity, new Expression<Func<ElementEntity, object>>[] 
                {
                    src => src.Status,
                    src => src.BigDiameter!,
                    src => src.SmallDiameter!,
                    src => src.InnerDiameter!,
                    src => src.OuterDiameter!,
                    src => src.Width!,
                    src => src.BigRangle!,
                    src => src.SmallRangle!,
                    src => src.PlaneWidth!,
                    src => src.AxialRunout!,
                    src => src.RadialRunout!,
                    src => src.CurrentAngle!
                });
                logRepository!.AddEntity(new TrackLog { Type = LogType.整修, Content = $"工件：{entity.MaterialNo} 整修" });
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "修整成功" : "修整失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }

        public async Task<dynamic> ReadyActionAsync(dynamic paramJson)
        {
            int woId = paramJson.woId;
            var order = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId);

            if (order is null)
                return new { success = false, message = "未知工单" };

            await fgmsDbContext!.BeginTrans();
            try
            {
                //添加标准组
                List<dynamic> stdCmps = JsonConvert.DeserializeObject<List<dynamic>>(paramJson.stdCmps.ToString());
                if (stdCmps != null && stdCmps.Count > 0)
                {
                    int[] jsonCmpIds = stdCmps.Select(src => (int)src.cmpId).ToArray();
                    var cmps = await componentRepository!.GetListAsync(expression: src => jsonCmpIds.Contains(src.Id), include: src => src.Include(src => src.ElementEntities!));
                    foreach (var cmp in cmps)
                    {
                        if (cmp.Status != ElementEntityStatus.在库)
                            return new { success = false, message = $"标准组：{cmp.Code} 状态异常！" };

                        var jsonCmp = stdCmps.FirstOrDefault(src => (int)src.cmpId == cmp.Id);
                        List<dynamic> jsonEes = JsonConvert.DeserializeObject<List<dynamic>>(jsonCmp!.ees.ToString());

                        //修改标准组工件状态
                        foreach (var ee in cmp.ElementEntities!)
                        {
                            if (ee.Status != ElementEntityStatus.在库)
                                return new { success = false, message = $"标准组工件：{ee.Code} 状态异常！" };

                            var jsonEe = jsonEes.FirstOrDefault(src => (int)src.id == ee.Id) ?? null;

                            if (jsonEe == null)
                                return new { success = false, message = $"标准组 {cmp.Code} 工件 {ee.Code} 未提供序列位" };

                            ee.CargoSpaceId = new int?();
                            ee.Status = ElementEntityStatus.出库;
                            ee.Position = jsonEe.position;
                            elementEntityRepository!.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[] { src => src.CargoSpaceId!, src => src.Status, src => src.Position });
                        }

                        //修改标准组状态
                        cmp.WorkOrderId = woId;
                        cmp.CargoSpaceId = new int?();
                        cmp.Status = ElementEntityStatus.出库;
                        componentRepository.UpdateEntity(cmp, new Expression<Func<Component, object>>[] { src => src.WorkOrderId!, src => src.CargoSpaceId!, src => src.Status });
                    }
                }
                //添加非标组
                List<dynamic> nonStdCmps = JsonConvert.DeserializeObject<List<dynamic>>(paramJson.nonStdCmps.ToString());
                if (nonStdCmps != null && nonStdCmps.Count > 0)
                {
                    foreach (var cmp in nonStdCmps)
                    {
                        List<dynamic> eeIds = JsonConvert.DeserializeObject<List<dynamic>>(cmp.eeIds.ToString());
                        var intIds = eeIds.Select(src => (int)src.id).ToArray();
                        var ees = await elementEntityRepository!.GetListAsync(expression: src => intIds.Contains(src.Id));
                        foreach (var ee in ees)
                        {
                            if (ee.Status != ElementEntityStatus.在库)
                                return new { success = false, message = $"非标组工件：{ee.Code} 状态异常！" };

                            var jsonEe = eeIds.FirstOrDefault(src => (int)src.id == ee.Id);

                            if (jsonEe is null || jsonEe.position is null)
                                return new { success = false, message = $"工件：{ee.Code} 未提供序列位" };

                            ee.ComponentId = cmp.cmpId;
                            ee.IsGroup = true;
                            ee.CargoSpaceId = new int?();
                            ee.Status = ElementEntityStatus.出库;
                            ee.Position = jsonEe.position.ToString();
                            elementEntityRepository.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[]
                            {
                                src => src.ComponentId!,
                                src => src.IsGroup,
                                src => src.CargoSpaceId!,
                                src => src.Status,
                                src => src.Position
                            });
                        }
                    }
                }
                order.Status = WorkOrderStatus.参数修整;
                workOrderRepository.UpdateEntity(order, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }

        public async Task<dynamic> AuditAsync(dynamic paramJson)
        {
            int woId = paramJson!.woId;
            var orderEntity = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId);

            if (orderEntity == null)
                return new { success = false, message = "未知工单" };

            int quantity = paramJson.nonStdCount is null ? 0 : paramJson.nonStdCount;
            int[] stdIds = JsonConvert.DeserializeObject<int[]>(paramJson.stdIds.ToString());

            if ((quantity + stdIds.Length) > 3)
                return new { success = false, message = "砂轮组数量超出限制" };

            await fgmsDbContext!.BeginTrans();
            try
            {
                //标准组
                if (stdIds.Length > 0)
                {
                    foreach (var id in stdIds)
                    {
                        workOrderStandardRepository!.AddEntity(new WorkOrderStandard { WorkOrderId = woId, StandardId = id });
                    }
                }
                //非标组
                if (quantity > 0)
                {
                    var cs = await cargoSpaceRepositroy!.GetEntityAsync(expression: src => src.Code!.Equals("NSG"));

                    if (cs == null)
                        return new { success = false, message = "未提供非标组货位，请联系管理员" };

                    for (int i = 0; i < quantity; i++)
                    {
                        componentRepository!.AddEntity(new Component
                        {
                            IsStandard = false,
                            WorkOrderId = woId,
                            CargoSpaceHistory = cs.Id,
                            Status = ElementEntityStatus.出库
                        });
                    }
                }

                orderEntity.Status = WorkOrderStatus.审核通过;
                workOrderRepository.UpdateEntity(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }

        public async Task<dynamic> MachineUpperAsync(int orderId)
        {
            var workOrder = await workOrderRepository!.GetEntityAsync(
                expression: src => src.Id == orderId,
                include: src => src
                    .Include(src => src.Childrens)
                    .Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!)
                    .Include(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            if (workOrder == null)
                return new { success = false, message = "未知工单" };

            if (workOrder.Childrens is not null && workOrder.Childrens.Any(src => src.Type == WorkOrderType.砂轮返修))
                return new { success = false, message = $"{workOrder.OrderNo}包含返修单，请待返修流程结束" };

            var productionOrder = workOrder.ProductionOrder;
            var equipment = productionOrder!.Equipment;

            if (equipment is null)
                return new { success = false, message = "未知设备" };

            if (equipment.Mount)
                return new { success = false, message = "该设备已挂载砂轮组，请勿重复上机" };

            var logs = new List<TrackLog>();
            var ees = workOrder.Components!.SelectMany(src => src.ElementEntities!).ToList();
            ees.ForEach(ee =>
            {
                ee.Status = ElementEntityStatus.上机;
                ee.BeginTime = DateTime.Now;
                ee.Remark = null;
                ee.Component = null;
                logs.Add(new TrackLog
                {
                    Content = $"工件：{ee.MaterialNo}已上机，机台：{workOrder.ProductionOrder!.Equipment!.Code}，时间：{ee.BeginTime.Value}",
                    JsonContent = CreateElementEntityJson(ee)
                });
            });

            productionOrder.Status = ProductionOrderStatus.生产中;
            equipment.Mount = true;
            await fgmsDbContext!.BeginTrans();
            try
            {
                bool updateSuccess =
                    elementEntityRepository!.UpdateEntity(ees, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.BeginTime, src => src.Remark }) &&
                    equipmentRepository!.UpdateEntity(equipment, new Expression<Func<Equipment, object>>[] { src => src.Mount }) &&
                    productionOrderRepository!.UpdateEntity(productionOrder, new Expression<Func<ProductionOrder, object>>[] { src => src.Status });

                if (!updateSuccess)
                    return new { success = false, message = "实体更新失败" };

                var addList = RecordComponentLogs(workOrder);

                if (addList.Any())
                    componentLogRepository!.AddEntity(addList);

                logRepository!.AddEntity(logs);
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (!success)
                {
                    await fgmsDbContext.RollBackTrans();
                    return new { success = false, message = "上机操作失败" };
                }
                await fgmsDbContext.CommitTrans();
                return new { success = true, message = "上机操作成功" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext!.RollBackTrans();
                throw new Exception($"Error:{ex.Message}");
            }
        }

        public async Task<dynamic> MachineDownAsync(List<Component> components)
        {
            var firstComponnet = components.First();
            if (firstComponnet.WorkOrderId == null)
                return new { success = false, message = "工单ID不能为空" };

            int woId = firstComponnet.WorkOrderId.Value;
            var orderEntity = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId, include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!));

            if (orderEntity == null)
                return new { success = false, message = "未知工单" };

            if (orderEntity.ProductionOrder == null)
                return new { success = false, message = "未知机台" };

            var equipment = orderEntity.ProductionOrder!.Equipment!;
            var updateEeList = new List<ElementEntity>();
            var logs = new List<TrackLog>();
            var finishTime = DateTime.UtcNow;

            // 处理元件实体
            foreach (var cmp in components)
            {
                if (cmp.ElementEntities == null) continue;
                foreach (var ee in cmp.ElementEntities)
                {
                    if (ee.BeginTime == null) continue;

                    ee.Status = ElementEntityStatus.下机;
                    ee.FinishTime = finishTime;
                    ee.UseDuration += (float)(finishTime - ee.BeginTime.Value).TotalHours;
                    updateEeList.Add(ee);
                    logs.Add(new TrackLog
                    {
                        Content = $"工件：{ee.MaterialNo}已下机，时间：{finishTime}",
                        JsonContent = CreateElementEntityJson(ee)
                    });
                }
            }

            // 更新设备状态
            equipment.Mount = false;
            await fgmsDbContext!.BeginTrans();
            try
            {
                bool updateSuccess = 
                    elementEntityRepository!.UpdateEntity(updateEeList, GetElementEntityUpdateFields()) && 
                    equipmentRepository!.UpdateEntity(equipment, new Expression<Func<Equipment, object>>[] { src => src.Mount });

                if (!updateSuccess)
                {
                    return new { success = false, message = "实体更新失败" };
                }

                //添加日志
                var updateCmpLogs = await GetComponentLogsAsync(components, orderEntity.OrderNo, finishTime);
                componentLogRepository!.AddEntity(updateCmpLogs);
                logRepository!.AddEntity(logs);
                
                bool saved = await fgmsDbContext.SaveChangesAsync() > 0;
                if (!saved)
                {
                    await fgmsDbContext.RollBackTrans();
                    return new { success = false, message = "实体保存失败" };
                }
                await fgmsDbContext.CommitTrans();
                return new { success = true, message = "下机操作成功" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception($"下机过程中发生错误：{ex.Message}");
            }
        }

        // 创建元素实体的JSON内容
        private static string CreateElementEntityJson(ElementEntity ee)
        {
            var logData = new
            {
                ee.Code,
                ee.BigDiameter,
                ee.SmallDiameter,
                ee.InnerDiameter,
                ee.OuterDiameter,
                ee.Width,
                ee.BigRangle,
                ee.SmallRangle,
                ee.PlaneWidth,
                ee.AxialRunout,
                ee.RadialRunout,
                ee.CurrentAngle,
                ee.BeginTime,
                ee.FinishTime,
                ee.UseDuration
            };
            return JsonConvert.SerializeObject(logData);
        }

        // 上机组件日志
        private static List<ComponentLog> RecordComponentLogs(WorkOrder order)
        {
            var addList = new List<ComponentLog>();
            var components = order.Components!.Where(src => src.IsStandard == true) ?? null;
            if (components != null && components!.Any())
            {
                foreach (var cmp in components)
                {
                    string equipmentCode = cmp.WorkOrder!.ProductionOrder!.Equipment!.Code;
                    string orderNo = cmp.WorkOrder.OrderNo;
                    var upperEes = cmp.ElementEntities!.Select(ee => new
                    {
                        ee.Code,
                        ee.BigDiameter,
                        ee.SmallDiameter,
                        ee.InnerDiameter,
                        ee.OuterDiameter,
                        ee.Width,
                        ee.BigRangle,
                        ee.SmallRangle,
                        ee.PlaneWidth,
                        ee.AxialRunout,
                        ee.RadialRunout,
                        ee.CurrentAngle,
                        ee.BeginTime,
                        ee.FinishTime,
                        ee.UseDuration,
                        Status = "上机"
                    }) ?? null;
                    addList.Add(new ComponentLog
                    {
                        Code = cmp.Code!,
                        OrderNo = orderNo,
                        MaterialNo = order.MaterialNo,
                        MaterialSpec = order.MaterialSpec,
                        EquipmentCode = equipmentCode,
                        RequiredDate = order.RequiredDate!.Value,
                        UpperJson = JsonConvert.SerializeObject(upperEes)
                    });
                }
            }
            return addList;
        }

        // 提取的更新字段配置方法
        private static Expression<Func<ElementEntity, object>>[] GetElementEntityUpdateFields()
        {
            return new Expression<Func<ElementEntity, object>>[]
            {
                src => src.Status,
                src => src.BigDiameter,
                src => src.SmallDiameter,
                src => src.InnerDiameter,
                src => src.OuterDiameter,
                src => src.Width,
                src => src.BigRangle,
                src => src.SmallRangle,
                src => src.PlaneWidth,
                src => src.AxialRunout,
                src => src.RadialRunout,
                src => src.CurrentAngle,
                src => src.FinishTime,
                src => src.UseDuration
            };
        }

        // 下机组件日志
        private async Task<List<ComponentLog>> GetComponentLogsAsync(List<Component> cmps, string orderNo, DateTime finishTime)
        {
            var updateCmpLogs = new List<ComponentLog>();
            foreach (var cmp in cmps.Where(c => c.IsStandard && c.ElementEntities != null))
            {
                var cmpLog = await componentLogRepository!.GetEntityAsync(expression: src => src.OrderNo.Equals(orderNo) && src.DownJson == null);
                if (cmpLog != null)
                {
                    var downEes = cmp.ElementEntities!.Select(ee => new
                    {
                        ee.Code,
                        ee.BigDiameter,
                        ee.SmallDiameter,
                        ee.InnerDiameter,
                        ee.OuterDiameter,
                        ee.Width,
                        ee.BigRangle,
                        ee.SmallRangle,
                        ee.PlaneWidth,
                        ee.AxialRunout,
                        ee.RadialRunout,
                        ee.CurrentAngle,
                        ee.BeginTime,
                        FinishTime = finishTime,
                        ee.UseDuration,
                        Status = "下机"
                    });
                    cmpLog.DownJson = JsonConvert.SerializeObject(downEes);
                    updateCmpLogs.Add(cmpLog);
                }
            }
            return updateCmpLogs;
        }

        // 砂轮组强制退仓
        public async Task<dynamic> WheelBackStockAsync(int woId)
        {
            var record = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId, include: src => src.Include(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            if (record is null)
                return new { success = false, message = "未知砂轮工单" };

            if (record.Status == WorkOrderStatus.工单结束)
                return new { success = false, message = "砂轮工单已结束" };

            if (record.ProductionOrderId.HasValue)
                return new { success = false, message = "正常砂轮工单，请走正常流程" };

            var components = record.Components!;
            if (components is null || !components.Any())
                return new { success = false, message = "未知砂轮组" };

            await fgmsDbContext!.BeginTrans();
            try
            {
                foreach (var com in components)
                {
                    if (com.CargoSpaceHistory is null)
                        return new { success = false, message = $"砂轮组 {com.Code} 未记录仓位信息，无法入库" };

                    foreach (var ee in com.ElementEntities!)
                    {
                        if (ee.CargoSpaceHistory is null)
                            return new { success = false, message = $"砂轮 {ee.Code} 未记录仓位信息，无法入库" };

                        if (ee.Status != ElementEntityStatus.在库)
                        {
                            ee.Status = ElementEntityStatus.在库;
                            ee.CargoSpaceId = ee.CargoSpaceHistory.Value;
                            ee.Position = null;
                            elementEntityRepository!.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.CargoSpaceId, src => src.Position } );
                        }
                    }

                    if (com.Status != ElementEntityStatus.在库)
                    {
                        com.WorkOrderId = null;
                        com.Status = ElementEntityStatus.在库;
                        com.CargoSpaceId = com.CargoSpaceHistory.Value;
                        componentRepository!.UpdateEntity(com, new Expression<Func<Component, object>>[] { src => src.Status, src => src.WorkOrderId, src => src.CargoSpaceId });
                    }
                }
                record.Status = WorkOrderStatus.工单结束;
                workOrderRepository.UpdateEntity(record, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "砂轮组强制退仓成功" : "砂轮组强制退仓失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                return new { success = false, message = "强制退仓过程中发生错误：" + ex.Message };
            }
        }
    }
}
