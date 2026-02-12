using System.Globalization;
using System.Linq.Dynamic.Core;
using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Mx.Services;
using FGMS.PC.Api.Filters;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

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
        private readonly IEquipmentService equipmentService;
        private readonly IEquipmentChangeOrderService equipmentOrderChangeService;
        private readonly IMaterialIssueOrderService materialIssueOrderService;
        private readonly IProductionOrderService productionOrderService;
        private readonly IBusinessService businessService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workOrderService"></param>
        /// <param name="brandService"></param>
        /// <param name="elementEntityService"></param>
        /// <param name="equipmentService"></param>
        /// <param name="equipmentOrderChangeService"></param>
        /// <param name="materialIssueOrderService"></param>
        /// <param name="productionOrderService"></param>
        /// <param name="businessService"></param>
        /// <param name="mapper"></param>
        public ChartController(
            IWorkOrderService workOrderService,
            IBrandService brandService,
            IElementEntityService elementEntityService,
            IEquipmentService equipmentService,
            IEquipmentChangeOrderService equipmentOrderChangeService,
            IMaterialIssueOrderService materialIssueOrderService,
            IProductionOrderService productionOrderService,
            IBusinessService businessService,
            IMapper mapper)
        {
            this.workOrderService = workOrderService;
            this.brandService = brandService;
            this.elementEntityService = elementEntityService;
            this.equipmentService = equipmentService;
            this.equipmentOrderChangeService = equipmentOrderChangeService;
            this.materialIssueOrderService = materialIssueOrderService;
            this.productionOrderService = productionOrderService;
            this.businessService = businessService;
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
            foreach (var status in Enum.GetValues(typeof(ElementEntityStatus)))
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
            var entities = await workOrderService.ListAsync(expression, include: src => src.Include(src => src.UserInfo!).Include(src => src.ProductionOrder!.Equipment!));
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
                        src.Status != WorkOrderStatus.工单结束 &&
                        (src.Status == WorkOrderStatus.待审 || src.Status == WorkOrderStatus.审核通过 ||
                        src.Status == WorkOrderStatus.砂轮整备 || src.Status == WorkOrderStatus.参数修整 ||
                        src.Status == WorkOrderStatus.整备完成 || src.Type == WorkOrderType.砂轮退仓 || src.Type == WorkOrderType.砂轮返修),
                    include: src => src.Include(src => src.UserInfo!).Include(src => src.ProductionOrder!.Equipment!)))
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
                include: src => src.Include(src => src.UserInfo!).Include(src => src.ProductionOrder!.Equipment!));
            orderEntities = orderEntities.OrderByDescending(src => src.CreateDate).ThenByDescending(src => src.Priority).ToList();
            return new { rows = mapper.Map<List<WorkOrderDto>>(orderEntities) };
        }

        /// <summary>
        /// 获取未审核机台变更单
        /// </summary>
        /// <returns></returns>
        [HttpGet("equipmentChangeByNotAudit")]
        public async Task<IActionResult> EquipmentChangeByNotAuditAsync()
        {
            var query = equipmentOrderChangeService.GetQueryable(src => src.Status == WorkOrderStatus.待审)
                .OrderByDescending(src => src.Id)
                .AsNoTracking();
            var entities = await query.ToListAsync();
            return Ok(mapper.Map<List<EquipmentChangeOrderDto>>(entities));
        }

        //===============================发料看板===============================//

        /// <summary>
        /// 获取发料单列表
        /// </summary>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet("issueOrderList")]
        public async Task<IActionResult> IssueOrderListAsync(string? status)
        {
            var expression = ExpressionBuilder.GetTrue<MaterialIssueOrder>()
                .AndIf(!string.IsNullOrEmpty(status), src => src.Status == Enum.Parse<MioStatus>(status!));

            var query = materialIssueOrderService.GetQueryable(
                expression,
                include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!).ThenInclude(src => src.Organize!).Include(src => src.Sendor!).Include(src => src.Createor!))
                .OrderByDescending(x => x.Id)
                .AsNoTracking();

            int total = await query.CountAsync();
            var entities = await query.ToListAsync();
            var dtos = mapper.Map<List<MaterialIssueOrderDto>>(entities);
            return Ok(new { total, rows = dtos });
        }

        //===============================生产看板===============================//

        /// <summary>
        /// 获取已排配、生产中、昨日完成、在制数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("productionBoard")]
        public async Task<IActionResult> ProductionBoardAsync()
        {
            var query = productionOrderService.GetQueryable(expression: src => src.Status != ProductionOrderStatus.已暂停 && src.Status != ProductionOrderStatus.机台变更)
                .OrderByDescending(src => src.Id)
                .AsNoTracking();

            var entities = await query.ToListAsync();
            return Ok(new
            {
                sorted = entities.Where(src => src.Status == ProductionOrderStatus.已排配).Count(),
                making = entities.Where(src => src.Status == ProductionOrderStatus.生产中 || src.Status == ProductionOrderStatus.已收料).Count(),
                makingQuantity = entities.Where(src => src.Status == ProductionOrderStatus.生产中 || src.Status == ProductionOrderStatus.已收料).Sum(src => src.Quantity),
                complate = entities.Where(src => src.Status == ProductionOrderStatus.已完成 && src.CompletedTime.HasValue && src.CompletedTime.Value.Date == DateTime.Today.AddDays(-1).Date).Count()
            });
        }

        /// <summary>
        /// 获取设备总数与开机数
        /// </summary>
        /// <returns></returns>
        [HttpGet("runingEquiptments")]
        public async Task<IActionResult> RuningEquiptmentsAsync()
        {
            var equipments = await equipmentService.ListAsync(include: src => src.Include(src => src.ProductionOrders!));
            return Ok(new
            {
                runing = equipments.Where(src => src.ProductionOrders!.Any(po => po.Status == ProductionOrderStatus.生产中 || po.Status == ProductionOrderStatus.已收料)).Count(),
                total = equipments.Count
            });
        }

        /// <summary>
        /// 获取报工数据
        /// </summary>
        /// <param name="param">now | yesterday</param>
        /// <returns></returns>
        [HttpGet("workReport")]
        public async Task<IActionResult> WorkReportAsync(string param)
        {
            if (!param.Equals("now") && !param.Equals("yesterday"))
                return BadRequest("param参数错误，应为'now'或'yesterday'");

            string strWhere;
            if (param.Equals("now"))
                strWhere = " and create_time >= curdate() and create_time <= now()";
            else
                strWhere = " and create_time >= curdate() - interval 1 day and create_time < curdate()";

            var result = await businessService.ReportSummaryAsync(strWhere);
            return Ok(result);
        }

        /// <summary>
        /// 获取制令单列表
        /// </summary>
        /// <param name="areaCode">区域</param>
        /// <returns></returns>
        [HttpGet("productionOrders")]
        public async Task<IActionResult> ProductionOrdersAsync(string areaCode = "A")
        {
            var query = productionOrderService.GetQueryable(expression: src => src.Status != ProductionOrderStatus.已暂停 && src.Status != ProductionOrderStatus.机台变更 && src.Equipment!.Organize!.Code.Contains(areaCode))
                .Include(src => src.Equipment!).ThenInclude(src => src.Organize!)
                .Include(src => src.MaterialIssueOrders)
                .Include(src => src.WorkOrder)
                .OrderByDescending(src => src.Id)
                .AsNoTracking();
            int total = await query.CountAsync();
            var entities = await query.ToListAsync();
            return Ok(new
            {
                //生产中
                makingList = mapper.Map<List<ProductionOrderDto>>(entities.Where(src => src.Status == ProductionOrderStatus.生产中 || src.Status == ProductionOrderStatus.已收料)),
                //已排配
                scheduledList = mapper.Map<List<ProductionOrderDto>>(entities.Where(src => src.Status == ProductionOrderStatus.已排配)),
                //待发料
                waitingIssueList = mapper.Map<List<ProductionOrderDto>>(entities.Where(src => src.Status == ProductionOrderStatus.待发料))
            });
        }

        /// <summary>
        /// 可叫料制令单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("availableProductionOrders")]
        public async Task<IActionResult> AvailableProductionOrdersAsync()
        {
            var query = productionOrderService.GetQueryable(expression: src => !src.IsDc!.Value && (src.Status == ProductionOrderStatus.已排配 || src.Status == ProductionOrderStatus.待发料 || src.Status == ProductionOrderStatus.已收料 || src.Status == ProductionOrderStatus.生产中))
                .Include(src => src.Equipment!).ThenInclude(src => src.Organize!)
                .AsNoTracking();

            var allEntities = await query.ToListAsync();
            var availables = new List<ProductionOrder>();

            // 按设备分组处理
            var equipmentGroups = allEntities.GroupBy(src => src.EquipmentId);

            foreach (var group in equipmentGroups)
            {
                // 计算已有工时
                double workHours = group
                    .Where(src => src.Status == ProductionOrderStatus.生产中 || src.Status == ProductionOrderStatus.已收料 || src.Status == ProductionOrderStatus.待发料)
                    .Sum(src => src.WorkHours ?? 0);

                // 获取已排配的订单，按优先级排序
                var scheduledOrders = group
                    .Where(src => src.Status == ProductionOrderStatus.已排配)
                    .OrderBy(src => src.Id);             // 排序条件

                foreach (var order in scheduledOrders)
                {
                    workHours += order.WorkHours ?? 0;
                    if (workHours > 24)
                        break;
                    availables.Add(order);
                }
            }

            return Ok(mapper.Map<List<ProductionOrderDto>>(availables.OrderBy(src => src.EquipmentId)));
        }
    }
}
