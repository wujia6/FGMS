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
        private readonly HttpClientHelper httpClientHelper;

        public AgvTaskSyncService(IBaseRepository<AgvTaskSync> repo, IFgmsDbContext context, IWorkOrderRepository workOrderRepository, HttpClientHelper httpClientHelper) : base(repo, context)
        {
            agvRepository = repo as IAgvTaskSyncRepository ?? throw new ArgumentNullException(nameof(repo));
            orderRepository = workOrderRepository ?? throw new ArgumentNullException(nameof(workOrderRepository));
            this.httpClientHelper = httpClientHelper ?? throw new ArgumentNullException(nameof(httpClientHelper));
        }

        public async Task CallbackAsync(string taskCode, string robotCode, string method)
        {
            switch (method)
            {
                case "start":
                    var workOrder = await orderRepository.GetEntityAsync(
                        expression: src => src.AgvTaskCode.Equals(taskCode),
                        include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!).ThenInclude(src => src.Organize!));
                    if (workOrder != null)
                    {
                        var sync = new AgvTaskSync
                        {
                            AgvCode = robotCode,
                            TaskCode = taskCode,
                            WorkOrderNo = workOrder.OrderNo,
                            Start = workOrder.Type == WorkOrderType.砂轮申领 ? "GW1" : workOrder.ProductionOrder!.Equipment!.Organize!.Code,
                            End = workOrder.Type == WorkOrderType.砂轮返修 || workOrder.Type == WorkOrderType.砂轮退仓 ? "GW2" : workOrder.ProductionOrder!.Equipment!.Organize!.Code
                        };
                        agvRepository.AddEntity(sync);
                    }
                    break;
                case "end":
                    var taskSync = await agvRepository.GetListAsync(expression: src => src.TaskCode.Equals(taskCode));
                    if (taskSync is not null && taskSync.Any())
                        agvRepository.DeleteEntity(taskSync);
                    break;
                default:
                    break;
            }
        }

        public async Task<dynamic> ExecuteAgvTaskAsync(string taskType, string taskUrl, string taskCode, string? start = null, string? end = null)
        {
            Dictionary<string, object> taskParam;
            if (taskType.Equals("execute"))
            {
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
            bool success = int.Parse(result.code.ToString()) == 0;

            if (!success)
                return new { success = false, message = $"AGV呼叫失败：{result.message}" };

            var orderEntity = await orderRepository.GetEntityAsync(expression: src => src.AgvTaskCode.Equals(taskCode));
            switch (orderEntity.Type)
            {
                case WorkOrderType.砂轮返修:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.呼叫AGV : WorkOrderStatus.返修配送;
                    break;
                case WorkOrderType.砂轮退仓:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.呼叫AGV : WorkOrderStatus.退仓配送;
                    break;
            }
            orderEntity.AgvStatus = taskType;
            success = orderRepository.UpdateEntity(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status, src => src.AgvStatus });
            return new { success, message = success ? taskType.Equals("execute") ? "工单已更新，AGV收料" : "工单已更新，AGV开始配送" : "工单更新失败" };
        }
    }
}
