using FGMS.Android.Api.Filters;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// 机台更换单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/equipmentChangeOrder")]
    [PermissionAsync("m_whell_management", "management", "移动")]
    public class EquipmentChangeOrderController : ControllerBase
    {
        private readonly IEquipmentChangeOrderService equipmentChangeOrderService;
        private readonly UserOnline userOnline;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipmentChangeOrderService"></param>
        /// <param name="userOnline"></param>
        public EquipmentChangeOrderController(IEquipmentChangeOrderService equipmentChangeOrderService, UserOnline userOnline)
        {
            this.equipmentChangeOrderService = equipmentChangeOrderService;
            this.userOnline = userOnline;
        }

        /// <summary>
        /// 添加机台更换单
        /// </summary>
        /// <param name="paramJson">{ 'woId': int, 'equipmentId': int, 'oldEquipmentCode': 'string', 'reason': 'string' }</param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<dynamic> AddAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.woId is null || paramJson.equipmentId is null || paramJson.oldEquipmentCode is null || paramJson.reason is null)
                return Task.FromResult<dynamic>(new { success = false, message = "参数错误" });

            int woId = paramJson.woId,
                equipmentId = paramJson.equipmentId,
                userId = userOnline.Id!.Value;
            string oldEquipmentCode = paramJson.oldEquipmentCode,
                reason = paramJson.reason;
            return await equipmentChangeOrderService.CreateAsync(woId, equipmentId, oldEquipmentCode, reason, userId);
        }
    }
}
