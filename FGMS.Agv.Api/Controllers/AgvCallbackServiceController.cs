using FGMS.Agv.Api.Hubs;
using FGMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FGMS.Agv.Api.Controllers
{
    /// <summary>
    /// AGV回调接口
    /// </summary>
    [ApiController]
    [Route("fgms/hk/agvCallbackService")]
    public class AgvCallbackServiceController : ControllerBase
    {
        private readonly IAgvTaskSyncService agvTaskSyncService;
        private readonly IHubContext<AgvHubService> hubContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agvTaskSyncService"></param>
        /// <param name="serviceHubContext"></param>
        public AgvCallbackServiceController(IAgvTaskSyncService agvTaskSyncService, IHubContext<AgvHubService> hubContext)
        {
            this.agvTaskSyncService = agvTaskSyncService;
            this.hubContext = hubContext;
        }

        /// <summary>
        /// AGV回调
        /// </summary>
        /// <param name="param">回调参数</param>
        /// <returns></returns>
        [HttpPost("agvCallback")]
        public async Task<IActionResult> AgvCallbackAsync([FromBody] dynamic param)
        {
            string reqCode = DateTime.Now.ToString("yyyyMMddHHmmssff");

            if (param is null || param.taskCode is null || param.robotCode is null || param.method is null)
                return BadRequest(new { code = 1, message = "失败", reqCode });

            string taskCode = param.taskCode, robotCode = param.robotCode, method = param.method;
            await agvTaskSyncService.CallbackAsync(taskCode, robotCode, method);
            await hubContext.Clients.All.SendAsync("agvCallbackReceived", true);
            return Ok(new { code = 0, message = "成功", reqCode });
        }
    }
}
