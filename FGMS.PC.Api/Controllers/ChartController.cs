using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 图表接口
    /// </summary>
    [ApiController]
    [Route("fgms/pc/chart")]
    public class ChartController : ControllerBase
    {
        private readonly IWorkOrderService workOrderService;
        private readonly IBrandService brandService;
        private readonly IElementEntityService elementEntityService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workOrderService"></param>
        /// <param name="brandService"></param>
        /// <param name="elementEntityService"></param>
        /// <param name="mapper"></param>
        public ChartController(IWorkOrderService workOrderService, IBrandService brandService, IElementEntityService elementEntityService, IMapper mapper)
        {
            this.workOrderService = workOrderService;
            this.brandService = brandService;
            this.elementEntityService = elementEntityService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 获取品牌元件分布
        /// </summary>
        /// <returns></returns>
        [HttpGet("brand")]
        public async Task<dynamic> BrandAsync()
        {
            var result = new List<dynamic>();
            var entities = await brandService.ListAsync(include: src => src.Include(src => src.Elements!));
            entities.ForEach(e =>
            {
                result.Add(new { name = e.Name, quantity = e.Elements!.Count() });
            });
            return new { total = entities.Count, data = result };
        }

        /// <summary>
        /// 按状态获取工件分布
        /// </summary>
        /// <returns></returns>
        [HttpGet("entities")]
        public async Task<dynamic> EntitiesAsync()
        {
            var result = new List<dynamic>();
            var entities = await elementEntityService.ListAsync();
            foreach(var status in Enum.GetValues(typeof(ElementEntityStatus)))
            {
                result.Add(new 
                { 
                    name = Enum.GetName(typeof(ElementEntityStatus), status), 
                    quantity = entities.Where(src => src.Status == (ElementEntityStatus)Enum.Parse(typeof(ElementEntityStatus), status.ToString()!)).Count()
                });
            }
            return new { total = entities.Count, data = result };
        }

        /// <summary>
        /// 查询指定类型、状态工单
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="status">状态</param>
        /// <param name="count">条数</param>
        /// <returns></returns>
        [HttpGet("order")]
        public async Task<dynamic> OrderAsync(string? type, string? status, int count = 20)
        {
            var expression = ExpressionBuilder.GetTrue<WorkOrder>();
            if (!string.IsNullOrEmpty(type))
                expression = expression.And(src => src.Type == (WorkOrderType)Enum.Parse(typeof(WorkOrderType), type));
            if (!string.IsNullOrEmpty(status))
                expression = expression.And(src => src.Status == (WorkOrderStatus)Enum.Parse(typeof(WorkOrderStatus), status));
            var entities = await workOrderService.ListAsync(expression, include: src => src.Include(src => src.UserInfo!).Include(src => src.Equipment!));
            entities = entities.OrderByDescending(src => src.Priority).ThenByDescending(src => src.RequiredDate).Take(count).ToList();
            return new { rows = mapper.Map<List<WorkOrderDto>>(entities) };
        }

        /// <summary>
        /// 待审、审核通过工单
        /// </summary>
        /// <returns></returns>
        [HttpGet("orderInAudit")]
        public async Task<dynamic> OrderInAuditAsync()
        {
            var orderEntities = (await workOrderService.ListAsync(
                expression: src => 
                                src.Type != WorkOrderType.机台更换 && src.Status != WorkOrderStatus.工单结束 && 
                                (src.Status == WorkOrderStatus.待审 || src.Status == WorkOrderStatus.审核通过 || 
                                src.Status == WorkOrderStatus.砂轮整备 || src.Status == WorkOrderStatus.参数修整 || 
                                src.Status == WorkOrderStatus.整备完成 || src.Type == WorkOrderType.砂轮退仓 || src.Type == WorkOrderType.砂轮返修), 
                include: src => src.Include(src => src.UserInfo!).Include(src => src.Equipment!)))
                .OrderByDescending(src => src.Priority)
                .ThenByDescending(src => src.RequiredDate)
                .ToList();
            return new { rows = mapper.Map<List<WorkOrderDto>>(orderEntities) };
        }

        /// <summary>
        /// 除待审、审核通过、工单结束的工单
        /// </summary>
        /// <returns></returns>
        [HttpGet("orderNotInAudit")]
        public async Task<dynamic> OrderNotInAuditAsync()
        {
            var orderEntities = await workOrderService.ListAsync(
                expression: src => src.Status != WorkOrderStatus.审核通过 && src.Status != WorkOrderStatus.砂轮整备 && 
                                src.Status != WorkOrderStatus.参数修整 && src.Status != WorkOrderStatus.整备完成 && 
                                src.Status != WorkOrderStatus.待审 && src.Status != WorkOrderStatus.工单结束 &&
                                src.Status != WorkOrderStatus.驳回 && src.Type == WorkOrderType.砂轮申领, 
                include: src => src.Include(src => src.UserInfo!).Include(src => src.Equipment!));
            orderEntities = orderEntities.OrderByDescending(src => src.CreateDate).ThenByDescending(src => src.Priority).ToList();
            return new { rows = mapper.Map<List<WorkOrderDto>>(orderEntities) };
        }
    }
}
