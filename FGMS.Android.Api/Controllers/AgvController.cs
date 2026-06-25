using FGMS.Services.Interfaces;
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
        private readonly IAgvTaskSyncService agvTaskSyncService;
        private readonly ConfigHelper configHelper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agvTaskSyncService"></param>
        /// <param name="configHelper"></param>
        public AgvController(IAgvTaskSyncService agvTaskSyncService, ConfigHelper configHelper)
        {
            this.agvTaskSyncService = agvTaskSyncService;
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

            string taskUrl = configHelper.GetAppSettings<string>("AgvServer"),
                taskType = param.taskType,
                taskCode = param.taskCode,
                start = param.start ?? string.Empty,
                end = param.end ?? string.Empty;

            return await agvTaskSyncService.ExecuteAgvTaskAsync(taskType, taskUrl, taskCode, start, end);
        }

        ///// <summary>
        ///// 模拟AGV任务
        ///// </summary>
        ///// <param name="param">{ 'taskType': 'execute|continue', 'taskCode': 'string', 'start': 'string', 'end': 'string' }</param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<dynamic> TestTask([FromBody] dynamic param)
        //{
        //    string taskType = param.taskType;
        //    string taskCode = param.taskCode;
        //    var orderEntity = await workOrderService.ModelAsync(expression: src => src.AgvTaskCode.Equals(taskCode));
        //    switch (orderEntity.Type)
        //    {
        //        case WorkOrderType.砂轮返修:
        //            orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.呼叫AGV : WorkOrderStatus.返修配送;
        //            break;
        //        case WorkOrderType.砂轮退仓:
        //            orderEntity.Status = taskType.Equals("execute") ? WorkOrderStatus.呼叫AGV : WorkOrderStatus.退仓配送;
        //            break;
        //    }
        //    orderEntity.AgvStatus = taskType;
        //    bool success = await workOrderService.UpdateAsync(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status, src => src.AgvStatus });
        //    string msg = orderEntity.AgvStatus.Equals("execute") ? "工单已更新，AGV收料" : "工单已更新，AGV开始配送";
        //    return new { success, message = success ? msg : "工单更新失败" };
        //}
    }
}
