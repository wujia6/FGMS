using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Services.Implements
{
    internal class AgvTaskSyncService : BaseService<AgvTaskSync>, IAgvTaskSyncService
    {
        private readonly IAgvTaskSyncRepository agvRepository;
        private readonly IWorkOrderRepository orderRepository;
        private readonly IEquipmentRepository equipmentRepository;
        private readonly HttpClientHelper httpClientHelper;
        private readonly IFgmsDbContext fgmsDbContext;

        public AgvTaskSyncService(
            IBaseRepository<AgvTaskSync> repo,
            IFgmsDbContext context,
            IWorkOrderRepository workOrderRepository,
            IEquipmentRepository equipmentRepository,
            HttpClientHelper httpClientHelper) : base(repo, context)
        {
            agvRepository = repo as IAgvTaskSyncRepository ?? throw new ArgumentNullException(nameof(repo));
            fgmsDbContext = context ?? throw new ArgumentNullException(nameof(context));
            this.orderRepository = workOrderRepository ?? throw new ArgumentNullException(nameof(workOrderRepository));
            this.equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
            this.httpClientHelper = httpClientHelper ?? throw new ArgumentNullException(nameof(httpClientHelper));
        }

        public async Task CallbackAsync(string taskCode, string robotCode, string method)
        {
            bool actionResult = false;
            switch (method)
            {
                case "start":
                    var workOrder = await orderRepository.GetEntityAsync(
                        expression: src => src.AgvTaskCode.Equals(taskCode),
                        include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!).ThenInclude(src => src.Organize!));

                    if (workOrder is null)
                        return;

                    var equipment = workOrder.ProductionOrder is null ?
                        await equipmentRepository.GetEntityAsync(expression: src => src.Code.Equals(workOrder.RepairEquipmentCode), include: src => src.Include(src => src.Organize!)) :
                        workOrder.ProductionOrder.Equipment;

                    string orgCode = equipment!.Organize!.Code;
                    var sync = new AgvTaskSync
                    {
                        AgvCode = robotCode,
                        TaskCode = taskCode,
                        WorkOrderNo = workOrder.OrderNo,
                        Start = workOrder.Type == WorkOrderType.砂轮申领 ? "GW1" : orgCode,
                        End = workOrder.Type == WorkOrderType.砂轮返修 || workOrder.Type == WorkOrderType.砂轮退仓 ? "GW2" : orgCode
                    };
                    actionResult = agvRepository.AddEntity(sync);
                    break;
                case "end":
                    var taskSync = await agvRepository.GetListAsync(expression: src => src.TaskCode.Equals(taskCode));
                    if (taskSync is not null && taskSync.Any())
                        actionResult = agvRepository.DeleteEntity(taskSync);
                    break;
                default:
                    break;
            }

            if (actionResult)
                await fgmsDbContext.SaveChangesAsync();
        }

        public async Task<dynamic> ExecuteAgvTaskAsync(string taskType, string taskUrl, string taskCode, string? start = null, string? end = null)
        {
            var orderEntity = await orderRepository.GetEntityAsync(expression: src => src.AgvTaskCode.Equals(taskCode));

            if (orderEntity is null)
                return new { success = false, message = "未知工单" };

            switch (orderEntity.Type)
            {
                case WorkOrderType.砂轮返修:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.呼叫AGV : WorkOrderStatus.返修配送;
                    break;
                case WorkOrderType.砂轮退仓:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.呼叫AGV : WorkOrderStatus.退仓配送;
                    break;
            }

            await fgmsDbContext.BeginTrans();
            try
            {
                orderEntity.AgvStatus = taskType;
                orderRepository.UpdateEntity(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status, src => src.AgvStatus });

                //呼叫AGV
                Dictionary<string, object> taskParam;
                if (taskType.Equals("execute"))
                {
                    //如或是返修工单，则从工单关联设备获取配送区域
                    if (string.IsNullOrEmpty(end) && orderEntity.Type == WorkOrderType.砂轮返修)
                    {
                        var equipment = await equipmentRepository.GetEntityAsync(expression: src => src.Code.Equals(orderEntity.RepairEquipmentCode),include: src => src.Include(src => src.Organize!));
                        end = equipment?.Organize?.Code;
                    }

                    if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                        throw new ArgumentNullException("start,end参数不能为空");

                    taskUrl = $"{taskUrl}genAgvSchedulingTask";
                    taskParam = new Dictionary<string, object>
                    {
                        { "reqCode", DateTime.Now.ToString("yyyyMMddHHmmssff") },
                        { "taskTyp", "SL1" },
                        { "taskCode", taskCode },
                        {
                            "positionCodePath", new List<Dictionary<string, object>>
                            {
                                new() { { "positionCode", start },{ "type", "00" } },
                                new() { { "positionCode", end }, { "type", "00" } }
                            }
                        }
                    };
                }
                else
                {
                    taskUrl = $"{taskUrl}continueTask";
                    taskParam = new Dictionary<string, object>
                    {
                        { "reqCode", DateTime.Now.ToString("yyyyMMddHHmmssff") },
                        { "taskCode", taskCode }
                    };
                }

                var result = await httpClientHelper.PostAsync<dynamic>(taskUrl, taskParam);
                bool success = success = int.Parse(result.code.ToString()) == 0 && await fgmsDbContext.SaveChangesAsync() > 0;

                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();

                return new
                {
                    success,
                    message = success ? taskType.Equals("execute") ? "工单已更新，AGV开始收料" : "工单已更新，AGV开始配送" : $"AGV呼叫失败：{result.message}"
                };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                return new { success = false, message = $"工单更新失败：{ex.Message}" };
            }
        }
    }
}
