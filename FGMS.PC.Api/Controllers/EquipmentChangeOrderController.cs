using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 机台变更单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/equipmentChangeOrder")]
    public class EquipmentChangeOrderController : ControllerBase
    {
        private readonly IEquipmentChangeOrderService equipmentChangeOrderService;
        private readonly IProductionOrderService productionOrderService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipmentChangeOrderService"></param>
        /// <param name="productionOrderService"></param>
        /// <param name="mapper"></param>
        public EquipmentChangeOrderController(IEquipmentChangeOrderService equipmentChangeOrderService, IProductionOrderService productionOrderService, IMapper mapper)
        {
            this.equipmentChangeOrderService = equipmentChangeOrderService;
            this.productionOrderService = productionOrderService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize, string? status)
        {
            var expression = ExpressionBuilder.GetTrue<EquipmentChangeOrder>()
                .AndIf(!string.IsNullOrEmpty(status), src => src.Status == Enum.Parse<WorkOrderStatus>(status!));
            var query = equipmentChangeOrderService.GetQueryable(
                expression, 
                include: src => src.Include(src => src.ProductionOrder!).Include(src => src.Equipment!).ThenInclude(src => src.Organize!).Include(src => src.UserInfo!))
                .OrderByDescending(src => src.Id)
                .AsNoTracking();
            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            var list = await query.ToListAsync();
            var dtoList = mapper.Map<List<EquipmentChangeOrderDto>>(list);
            return new { total, list = dtoList };
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="paramJson">{ 'ecid': int, 'status': 'string' }</param>
        /// <returns></returns>
        [HttpPut("audit")]
        public async Task<dynamic> AuditAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.ecid is null || paramJson.status is null)
                return Task.FromResult<dynamic>(new { success = false, message = "参数错误" });

            int ecid = (int)paramJson.ecid;
            string status = (string)paramJson.status;
            return await equipmentChangeOrderService.AuditAsync(ecid, status);
        }
    }
}
