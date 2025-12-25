using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.PC.Api.Filters;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 制令单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/productionOrder")]
    public class ProductionOrderController : ControllerBase
    {
        private readonly IProductionOrderService productionOrderService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionOrderService"></param>
        /// <param name="mapper"></param>
        public ProductionOrderController(IProductionOrderService productionOrderService, IMapper mapper)
        {
            this.productionOrderService = productionOrderService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 获取制令单列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="keyword">关键字：单号、成品料号、机台编码</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet("list")]
        [PermissionAsync("production_order_management", "view", "电脑")]
        public async Task<IActionResult> ListAsync(int? pageIndex, int? pageSize, string? keyword, string? status)
        {
            var expression = ExpressionBuilder.GetTrue<ProductionOrder>()
                .AndIf(!string.IsNullOrEmpty(keyword), x => x.OrderNo!.Contains(keyword!) || x.FinishCode!.Contains(keyword!) || x.Equipment!.Code!.Contains(keyword!) || x.MaterialCode.Contains(keyword!))
                .AndIf(!string.IsNullOrEmpty(status), x => x.Status == Enum.Parse<ProductionOrderStatus>(status!));
            
            var query = productionOrderService.GetQueryable(
                expression, 
                include: x => x.Include(x => x.UserInfo!)
                    .Include(x => x.WorkOrder!)
                    .Include(x => x.Equipment!).ThenInclude(x => x.Organize!)
                    .Include(x => x.MaterialIssueOrders!.Where(d => d.Type == MioType.补料)))
                .OrderByDescending(x => x.Id)
                .AsNoTracking();

            var total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var list = await query.ToListAsync();
            var dtos = mapper.Map<List<ProductionOrderDto>>(list);
            return Ok(new { total, rows = dtos });
        }

        /// <summary>
        /// 按设备获取未叫料的制令单列表
        /// </summary>
        /// <param name="equipmentId">机台ID</param>
        /// <returns></returns>
        [HttpGet("listByEquipment")]
        [PermissionAsync("production_order_management", "view", "电脑")]
        public async Task<IActionResult> ListByEquipmentAsync(int equipmentId)
        {
            var query = productionOrderService.GetQueryable(expression: src => src.EquipmentId == equipmentId && src.WorkOrder == null && !src.MaterialIssueOrders.Any() && src.Status == ProductionOrderStatus.已排配);
            var entities = await query.ToListAsync();
            var dtos = mapper.Map<List<ProductionOrderDto>>(entities);
            return Ok(dtos);
        }

        /// <summary>
        /// 添加制令单
        /// </summary>
        /// <param name="dto">JSON</param>
        /// <returns></returns>
        [HttpPost("add")]
        [PermissionAsync("production_order_management", "management", "电脑")]
        public async Task<dynamic> AddAsync([FromBody] ProductionOrderDto dto)
        {
            var entity = mapper.Map<ProductionOrder>(dto);
            bool success = await productionOrderService.AddAsync(entity);
            return success ? new { success, message = "添加成功" } : new { success, message = "添加失败" };
        }
    }
}
