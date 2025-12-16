using FGMS.Models.Entities;
using FGMS.Models;
using FGMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// RCS回调接口
    /// </summary>
    [ApiController]
    [Route("fgms/pc/agvCallbackService")]
    public class AgvCallbackServiceController : ControllerBase
    {
        private readonly IAgvTaskSyncService agvTaskSyncService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workOrderService"></param>
        /// <param name="agvTaskSyncService"></param>
        public AgvCallbackServiceController(IAgvTaskSyncService agvTaskSyncService)
        {
            this.agvTaskSyncService = agvTaskSyncService;
        }

        /// <summary>
        /// RCS回调
        /// </summary>
        /// <param name="param">回调参数</param>
        /// <returns></returns>
        [HttpPost("agvCallback")]
        public async Task<dynamic> AgvCallback([FromBody] dynamic param)
        {
            string reqCode = DateTime.Now.ToString("yyyyMMddHHmmssff");

            if(param is null || param.taskCode is null || param.robotCode is null || param.method is null)
                return new { code = 1, message = "失败", reqCode };

            string taskCode = param.taskCode, robotCode = param.robotCode, method = param.method;
            //switch (method)
            //{
            //    case "start":
            //        var workOrder = await workOrderService.ModelAsync(
            //            expression: src => src.AgvTaskCode.Equals(taskCode), 
            //            include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!).ThenInclude(src => src.Organize!));
            //        if (workOrder != null)
            //        {
            //            var sync = new AgvTaskSync
            //            {
            //                AgvCode = robotCode,
            //                TaskCode = taskCode,
            //                WorkOrderNo = workOrder.OrderNo,
            //                Start = workOrder.Type == WorkOrderType.砂轮申领 ? "GW1" : workOrder.ProductionOrder!.Equipment!.Organize!.Code,
            //                End = workOrder.Type == WorkOrderType.砂轮返修 || workOrder.Type == WorkOrderType.砂轮退仓 ? "GW2" : workOrder.ProductionOrder!.Equipment!.Organize!.Code
            //            };
            //            await agvTaskSyncService.AddAsync(sync);
            //        }
            //        break;
            //    case "end":
            //        var taskSync = await agvTaskSyncService.ListAsync(expression: src => src.TaskCode.Equals(taskCode));
            //        if (taskSync is not null && taskSync.Any())
            //            await agvTaskSyncService.RemoveAsync(taskSync);
            //        break;
            //    default:
            //        break;
            //}
            await agvTaskSyncService.CallbackAsync(taskCode, robotCode, method);
            return new { code = "0", message = "成功", reqCode };
        }
    }
}
