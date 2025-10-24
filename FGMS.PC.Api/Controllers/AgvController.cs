using System.Linq.Expressions;
using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// AGV接口
    /// </summary>
    //[Authorize]
    [ApiController]
    [Route("fgms/pc/agv")]
    public class AgvController : ControllerBase
    {
        private readonly IWorkOrderService workOrderService;
        private readonly IAgvTaskSyncService agvTaskSyncService;
        private readonly HttpClientHelper httpClient;
        private readonly ConfigHelper configHelper;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workOrderService"></param>
        /// <param name="agvTaskSyncService"></param>
        /// <param name="httpClient"></param>
        /// <param name="configHelper"></param>
        /// <param name="mapper"></param>
        public AgvController(IWorkOrderService workOrderService, IAgvTaskSyncService agvTaskSyncService, HttpClientHelper httpClient, ConfigHelper configHelper, IMapper mapper)
        {
            this.workOrderService = workOrderService;
            this.agvTaskSyncService = agvTaskSyncService;
            this.httpClient = httpClient;
            this.configHelper = configHelper;
            this.mapper = mapper;
        }

        /// <summary>
        /// agv任务上报
        /// </summary>
        /// <returns></returns>
        [HttpGet("tasksync")]
        public async Task<dynamic> TaskSyncAsync()
        {
            var entities = await agvTaskSyncService.ListAsync();
            return mapper.Map<List<AgvTaskSyncDto>>(entities);
        }

        /// <summary>
        /// 执行AGV任务
        /// </summary>
        /// <param name="param">{ 'taskType': 'execute|continue', 'taskCode': 'string', 'start':'string', 'end': 'string' }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [Authorize]
        [HttpPost("executetask")]
        public async Task<dynamic> ExecuteTask([FromBody] dynamic param)
        {
            if (param is null || param.taskCode is null)
                throw new ArgumentNullException(nameof(param));

            string server = configHelper.GetAppSettings<string>("AgvServer");

            string taskType = param.taskType;
            string taskCode = param.taskCode;
            string taskUrl = string.Empty;
            Dictionary<string, object> taskParam;
            if (taskType.Equals("execute"))
            {
                if (param.start is null || param.end is null)
                    throw new ArgumentNullException(nameof(param));

                taskUrl = $"{server}genAgvSchedulingTask";
                taskParam = new Dictionary<string, object>
                {
                    { "reqCode", DateTime.Now.ToString("yyyyMMddHHmmssff") },
                    { "taskTyp", "SL1" },
                    { "taskCode", taskCode },
                    {
                        "positionCodePath", new List<Dictionary<string, object>>
                        {
                            new() { { "positionCode", param.start.ToString() },{ "type", "00" } },
                            new() { { "positionCode", param.end.ToString() }, { "type", "00" } }
                        }
                    }
                };
            }
            else
            {
                taskUrl = $"{server}continueTask";
                taskParam = new Dictionary<string, object>
                {
                    { "reqCode", DateTime.Now.ToString("yyyyMMddHHmmssff") },
                    { "taskCode", param.taskCode.ToString() }
                };
            }
            var result = await httpClient.PostAsync<dynamic>(taskUrl, taskParam);
            bool success = int.Parse(result.code.ToString()) == 0;
            if (!success)
                return new { success = false, message = $"AGV呼叫失败：{result.message}" };
            var orderEntity = await workOrderService.ModelAsync(expression: src => src.AgvTaskCode.Equals(taskCode));
            switch (orderEntity.Type)
            {
                case WorkOrderType.砂轮申领:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.AGV收料 : WorkOrderStatus.工单配送;
                    break;
                case WorkOrderType.砂轮返修:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.AGV收料 : WorkOrderStatus.工单配送;
                    break;
            }
            orderEntity.AgvStatus = taskType;
            success = await workOrderService.UpdateAsync(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status, src => src.AgvStatus });
            return new { success, message = success ? taskType.Equals("execute") ? "工单已更新，AGV收料" : "工单已更新，AGV开始配送" : "工单更新失败" };
        }

        /// <summary>
        /// AGV状态
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("agvstatus")]
        public async Task<dynamic> AgvStatusAsync()
        {
            string server = configHelper.GetAppSettings<string>("AgvServer");
            string commandUrl = $"{server}queryTaskStatus";
            var paramData = new Dictionary<string, object>
            {
                { "reqCode", DateTime.Now.ToString("yyyyMMddHHmmssff") },
                { "agvCode", "276" }
            };
            var result = await httpClient.PostAsync<dynamic>(commandUrl, paramData);
            bool success = int.Parse(result.code.ToString()) == 0;

            if (!success)
                return new { success = false, message = $"获取AGV状态：{result.message}" };

            List<dynamic> resultData = JsonConvert.DeserializeObject<List<dynamic>>(result.data.ToString());

            if (!resultData.Any(src => int.Parse(src.taskStatus.ToString()) == 2))
                return new { status = 0, message = "AGV空闲中" };

            string taskCode = resultData.FirstOrDefault(src => int.Parse(src.taskStatus.ToString()) == 2)!.taskCode;
            var orderEntity = await workOrderService.ModelAsync(expression: src => src.AgvTaskCode.Equals(taskCode));

            if (orderEntity == null)
                return new { status = 9, message = "未找到AGV工单任务" };

            return new { status = 1, message = $"AGV正在执行任务，工单：{orderEntity.OrderNo}" };
        }

        /// <summary>
        /// 模拟AGV
        /// </summary>
        /// <param name="param">{ 'taskType': 'execute|continue', 'taskCode': 'string', 'start':'string', 'end': 'string' }</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("executesimulate")]
        public async Task<dynamic> ExecuteSimulate([FromBody] dynamic param)
        {
            string taskType = param.taskType;
            string taskCode = param.taskCode;
            var orderEntity = await workOrderService.ModelAsync(expression: src => src.AgvTaskCode.Equals(taskCode));
            switch (orderEntity.Type)
            {
                case WorkOrderType.砂轮申领:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.AGV收料 : WorkOrderStatus.工单配送;
                    break;
                case WorkOrderType.砂轮返修:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.AGV收料 : WorkOrderStatus.工单配送;
                    break;
            }
            orderEntity.AgvStatus = taskType;
            bool success = await workOrderService.UpdateAsync(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status, src => src.AgvStatus });
            string msg = orderEntity.AgvStatus.Equals("execute") ? "工单已更新，AGV收料" : "工单已更新，AGV开始配送";
            return new { success, message = success ? msg : "工单更新失败" };
        }
    }
}
