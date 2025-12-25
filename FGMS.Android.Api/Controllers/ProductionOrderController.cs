using System.Linq.Expressions;
using FGMS.Android.Api.Filters;
using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Mx.Services;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// 制令单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/productionOrder")]
    [PermissionAsync("m_production_order_management", "management", "移动")]
    public class ProductionOrderController : ControllerBase
    {
        private readonly IProductionOrderService productionOrderService;
        private readonly IBusinessService businessService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionOrderService"></param>
        /// <param name="businessService"></param>
        /// <param name="mapper"></param>
        public ProductionOrderController(IProductionOrderService productionOrderService, IBusinessService businessService, IMapper mapper)
        {
            this.productionOrderService = productionOrderService;
            this.businessService = businessService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 获取制令单列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="equCode">设备编码</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> ListAsync(int? pageIndex, int? pageSize, string? equCode)
        {
            var expression = ExpressionBuilder.GetTrue<ProductionOrder>()
                .AndIf(!string.IsNullOrEmpty(equCode), x => x.Equipment!.Code.Equals(equCode))
                .And(x => x.Status != ProductionOrderStatus.已完成);

            var query = productionOrderService.GetQueryable(
                expression,
                include: x => x
                    .Include(x => x.UserInfo!)
                    .Include(x => x.Equipment!)
                    .Include(x => x.WorkOrder!)
                    .Include(x => x.MaterialIssueOrders!).ThenInclude(mio => mio.Sendor!))
                    .OrderBy(x => x.Id)
                    .AsNoTracking();

            var total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var list = await query.ToListAsync();
            var dtos = mapper.Map<List<ProductionOrderDto>>(list);
            return Ok(new { total, rows = dtos });
        }

        /// <summary>
        /// 开工
        /// </summary>
        /// <param name="paramJson">{ 'poid': int }</param>
        /// <returns></returns>
        [HttpPost("madeStart")]
        public async Task<IActionResult> MadeStartAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.poid is null)
                return BadRequest(new { success = false, message = "参数错误" });

            int poid = paramJson.poid;
            var target = await productionOrderService.ModelAsync(
                expression: src => src.Id == poid,
                include: src => src.Include(src => src.WorkOrder!).ThenInclude(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            if (target is null)
                return BadRequest(new { success = false, message = "未知制令单" });

            if (target.Status != ProductionOrderStatus.已收料)
                return BadRequest(new { success = false, message = "请按流程叫料、收料后，再开工" });

            if (target.WorkOrder != null)
            {
                if (target.WorkOrder.Status != WorkOrderStatus.机台接收)
                    return BadRequest(new { success = false, message = $"砂轮工单：{target.WorkOrder.OrderNo}状态错误" });

                if (target.WorkOrder.Components != null && target.WorkOrder.Components.Any(src => src.ElementEntities != null && src.ElementEntities.Any(e => e.Status != ElementEntityStatus.上机)))
                    return BadRequest(new { success = false, message = "砂轮未上机" });
            }

            target.Status = ProductionOrderStatus.生产中;
            bool success = await productionOrderService.UpdateAsync(target, new Expression<Func<ProductionOrder, object>>[] { src => src.Status });
            await businessService.UpdateProductionOrderStatus(target.OrderNo, "生产中");   // 同步状态到MES
            return success ? Ok(new { success, message = "操作成功，状态已更新" }) : BadRequest(new { success, message = "操作失败" });
        }
    }
}
