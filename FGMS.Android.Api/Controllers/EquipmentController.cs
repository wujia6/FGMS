using FGMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// 机台接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/equipment")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentService equipmentService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipmentService"></param>
        public EquipmentController(IEquipmentService equipmentService)
        {
            this.equipmentService = equipmentService;
        }

        /// <summary>
        /// 获取机台所属组织码
        /// </summary>
        /// <param name="equCode">机台编码</param>
        /// <returns></returns>
        [HttpGet("orgcode")]
        public async Task<IActionResult> GetOrgCodeAsync([FromQuery] string equCode)
        {
            var equItem = await equipmentService.ModelAsync(expression: src => src.Code.Equals(equCode), include: src => src.Include(src => src.Organize!));
            if (equItem == null || equItem.Organize == null)
            {
                return BadRequest(new { success = false, message = "设备或组织不存在" });
            }
            return Ok(new { success = true, orgCode = equItem.Organize.Code });
        }
    }
}
