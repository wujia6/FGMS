using System.Linq.Expressions;
using FGMS.Agv.Api.Hubs;
using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace FGMS.Agv.Api.Controllers
{
    /// <summary>
    /// 海康AGV接口
    /// </summary>
    [ApiController]
    [Route("fgms/hk/agv")]
    public class AgvController : ControllerBase
    {
        private readonly IWorkOrderService workOrderService;
        private readonly IAgvTaskSyncService agvTaskSyncService;
        private readonly IMapper mapper;
        private readonly IHubContext<AgvHubService> hubContext;

        private readonly HttpClientHelper httpClient;
        private readonly ConfigHelper configHelper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workOrderService"></param>
        /// <param name="agvTaskSyncService"></param>
        /// <param name="hubContext"></param>
        /// <param name="mapper"></param>
        /// <param name="httpClient"></param>
        /// <param name="configHelper"></param>
        public AgvController(
            IWorkOrderService workOrderService,
            IAgvTaskSyncService agvTaskSyncService,
            IMapper mapper,
            IHubContext<AgvHubService> hubContext,
            HttpClientHelper httpClient,
            ConfigHelper configHelper)
        {
            this.workOrderService = workOrderService;
            this.agvTaskSyncService = agvTaskSyncService;
            this.mapper = mapper;
            this.hubContext = hubContext;
            this.httpClient = httpClient;
            this.configHelper = configHelper;
        }

        /// <summary>
        /// 获取任务列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("taskList")]
        public async Task<IActionResult> TaskListAsync()
        {
            var entities = await agvTaskSyncService.ListAsync();
            return Ok(mapper.Map<List<AgvTaskSyncDto>>(entities));
        }

        /// <summary>
        /// 获取AGV状态
        /// </summary>
        /// <param name="agvCode">编号</param>
        /// <returns></returns>
        [HttpPost("getStatus")]
        public async Task<IActionResult> GetStatusAsync(string agvCode)
        {
            string server = configHelper.GetAppSettings<string>("AgvServer");
            string commandUrl = $"{server}queryTaskStatus";
            var paramData = new Dictionary<string, object>
            {
                { "reqCode", DateTime.Now.ToString("yyyyMMddHHmmssff") },
                { "agvCode", agvCode }
            };
            var result = await httpClient.PostAsync<dynamic>(commandUrl, paramData);
            bool success = int.Parse(result.code.ToString()) == 0;

            if (!success)
                return BadRequest(new { success = false, message = $"获取AGV状态：{result.message}" });

            List<dynamic> resultData = JsonConvert.DeserializeObject<List<dynamic>>(result.data.ToString());

            if (!resultData.Any(src => int.Parse(src.taskStatus.ToString()) == 2))
                return Ok(new { status = 0, message = "AGV空闲中" });

            string taskCode = resultData.FirstOrDefault(src => int.Parse(src.taskStatus.ToString()) == 2)!.taskCode;
            var orderEntity = await workOrderService.ModelAsync(expression: src => src.AgvTaskCode.Equals(taskCode));

            if (orderEntity == null)
                return Ok(new { status = 9, message = "未找到AGV工单任务" });

            return Ok(new { status = 1, message = $"AGV正在执行任务，工单：{orderEntity.OrderNo}" });
        }

        /// <summary>
        /// 执行agv任务
        /// </summary>
        /// <param name="param">JSON</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost("executeTask")]
        public async Task<IActionResult> ExecuteTaskAsync([FromBody] dynamic param)
        {
            if (param is null || param.taskCode is null)
                throw new ArgumentNullException(nameof(param));

            string taskType = param.taskType,
                taskCode = param.taskCode,
                taskUrl = configHelper.GetAppSettings<string>("AgvServer"),
                start = param.start,
                end = param.end;

            var result = await agvTaskSyncService.ExecuteAgvTaskAsync(taskType, taskUrl, taskCode, start, end);
            if (result.success)
            {
                // 发送通知
                await hubContext.Clients.All.SendAsync("agvTaskExecuted");
            }
            return Ok(result);
        }

        /// <summary>
        /// AGV模拟执行
        /// </summary>
        /// <param name="param">JSON</param>
        /// <returns></returns>
        [HttpPost("executeSimulate")]
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
