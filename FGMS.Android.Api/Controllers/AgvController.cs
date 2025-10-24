using FGMS.Models.Entities;
using FGMS.Models;
using FGMS.Services.Interfaces;
using System.Linq.Expressions;
using FGMS.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// AGV接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/[controller]/[action]")]
    public class AgvController : ControllerBase
    {
        private readonly IWorkOrderService workOrderService;
        private readonly HttpClientHelper httpClient;
        private readonly ConfigHelper configHelper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workOrderService"></param>
        /// <param name="httpClient"></param>
        /// <param name="configHelper"></param>
        public AgvController(IWorkOrderService workOrderService, HttpClientHelper httpClient, ConfigHelper configHelper)
        {
            this.workOrderService = workOrderService;
            this.httpClient = httpClient;
            this.configHelper = configHelper;
        }

        /// <summary>
        /// 执行AGV任务
        /// </summary>
        /// <param name="param">{ 'taskType': 'execute|continue', 'taskCode': 'string', 'start': 'string', 'end': 'string' }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost]
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
                case WorkOrderType.砂轮返修:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.呼叫AGV : WorkOrderStatus.返修配送;
                    break;
                case WorkOrderType.砂轮退仓:
                    orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.呼叫AGV : WorkOrderStatus.退仓配送;
                    break;
            }
            orderEntity.AgvStatus = taskType;
            success = await workOrderService.UpdateAsync(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status, src => src.AgvStatus });
            return new { success, message = success ? taskType.Equals("execute") ? "工单已更新，AGV收料" : "工单已更新，AGV开始配送" : "工单更新失败" };
        }

        /// <summary>
        /// 模拟AGV任务
        /// </summary>
        /// <param name="param">{ 'taskType': 'execute|continue', 'taskCode': 'string', 'start': 'string', 'end': 'string' }</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> TestTask([FromBody] dynamic param)
        {
            string taskType = param.taskType;
            string taskCode = param.taskCode;
            var orderEntity = await workOrderService.ModelAsync(expression: src => src.AgvTaskCode.Equals(taskCode));
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
            bool success = await workOrderService.UpdateAsync(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status, src => src.AgvStatus });
            string msg = orderEntity.AgvStatus.Equals("execute") ? "工单已更新，AGV收料" : "工单已更新，AGV开始配送";
            return new { success, message = success ? msg : "工单更新失败" };
        }
    }
}
