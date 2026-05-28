using Microsoft.AspNetCore.Mvc;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// API健康检查接口
    /// </summary>
    [ApiController]
    [Route("fgms/android/[controller]/[action]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public HealthController(ILogger<HealthController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 简单的心跳检测接口，返回服务器当前时间和状态信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Ping()
        {
            logger.LogDebug("{time} 心跳检测", DateTime.Now);
            return Ok(new
            {
                success = true,
                message = "ok"
            });
        }
    }
}
