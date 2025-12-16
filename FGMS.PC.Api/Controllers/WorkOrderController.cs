using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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
    /// 工单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/workorder")]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderService workOrderService;
        private readonly IWorkOrderStandardService workOrderStandardService;
        private readonly IComponentService componentService;
        private readonly ICargoSpaceService cargoSpaceService;
        private readonly GenerateRandomNumber randomNumber;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workOrderService"></param>
        /// <param name="workOrderStandardService"></param>
        /// <param name="componentService"></param>
        /// <param name="cargoSpaceService"></param>
        /// <param name="randomNumber"></param>
        /// <param name="mapper"></param>
        public WorkOrderController(
            IWorkOrderService workOrderService, 
            IWorkOrderStandardService workOrderStandardService, 
            IComponentService componentService, 
            ICargoSpaceService cargoSpaceService, 
            GenerateRandomNumber randomNumber, 
            IMapper mapper)
        {
            this.workOrderService = workOrderService;
            this.workOrderStandardService = workOrderStandardService;
            this.componentService = componentService;
            this.cargoSpaceService = cargoSpaceService;
            this.randomNumber = randomNumber;
            this.mapper = mapper;
        }

        /// <summary>
        /// 集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="type">类型</param>
        /// <param name="orderNo">工单号</param>
        /// <param name="equipmentCode">设备号</param>
        /// <param name="materialNo">料号</param>
        /// <param name="status">状态</param>
        /// <param name="date">日期</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize, string? type, string? orderNo, string? equipmentCode, string? materialNo, string? status, DateTime? date)
        {
            var expression = ExpressionBuilder.GetTrue<WorkOrder>()
                .AndIf(!string.IsNullOrEmpty(type), src => src.Type == Enum.Parse<WorkOrderType>(type!))
                .AndIf(!string.IsNullOrEmpty(orderNo), src => src.OrderNo.Equals(orderNo!))
                .AndIf(!string.IsNullOrEmpty(equipmentCode), src => src.ProductionOrder!.Equipment!.Code.Equals(equipmentCode!))
                .AndIf(!string.IsNullOrEmpty(materialNo), src => src.MaterialNo.Contains(materialNo!))
                .AndIf(!string.IsNullOrEmpty(status), src => src.Status == Enum.Parse<WorkOrderStatus>(status!))
                .AndIf(date.HasValue, src => src.CreateDate.Date == date!.Value);

            var query = workOrderService.GetQueryable(expression, include: src => src
                    .Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!.Organize)
                    .Include(src => src.Components!).ThenInclude(src => src.ElementEntities!.OrderBy(src => src.Position)).ThenInclude(src => src.Element!)
                    .Include(src => src.Parent!).ThenInclude(src => src.ProductionOrder!.Equipment!)
                    .Include(src => src.UserInfo!))
                .OrderByDescending(src => src.Priority)
                .ThenByDescending(src => src.CreateDate)
                .AsNoTracking();

            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            return new { total, rows = mapper.Map<List<WorkOrderDto>>(entities) };
        }

        /// <summary>
        /// 按工件编码查找工单
        /// </summary>
        /// <param name="eeCode">工件编码</param>
        /// <returns></returns>
        [HttpGet("single")]
        public async Task<dynamic> SingleAsync(string eeCode)
        {
            var entity = await workOrderService.ModelAsync(
                expression: src => src.Status != WorkOrderStatus.工单结束 && src.Components!.Any(src => src.ElementEntities!.FirstOrDefault(src => src.Code!.Equals(eeCode)) != null),
                include: src => src.Include(src => src.ProductionOrder!.Equipment!).Include(src => src.UserInfo!).Include(src => src.Components!).ThenInclude(src => src.ElementEntities!));
            if (entity is null)
                return new { success = false, message = "工单不存在或已删除" };
            return mapper.Map<WorkOrderDto>(entity);
        }

        /// <summary>
        /// 添加工单
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<dynamic> AddAsync([FromBody] WorkOrderDto dto)
        {
            var entity = mapper.Map<WorkOrder>(dto);
            entity.OrderNo = $"WO{randomNumber.CreateOrderNum()}";
            //entity.AgvTaskCode = Guid.NewGuid().ToString("N")[..16];
            entity.Type = WorkOrderType.砂轮申领;
            entity.Status = WorkOrderStatus.待审;
            bool success = await workOrderService.AddAsync(entity);
            return new { success, message = success ? "添加成功" : "添加失败" };
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="paramJson">{ 'woId' : int , 'nonStdCount': int, 'stdIds': [int] }</param>
        /// <returns></returns>
        [HttpPut("audit")]
        public async Task<dynamic> AuditAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson!.woId is null && paramJson!.status is null)
                return new { success = false, message = "参数错误" };
            return await workOrderService.AuditAsync(paramJson);
        }
        
        /// <summary>
        /// 驳回
        /// </summary>
        /// <param name="paramJson">{ 'woId' : int, 'remark': 'string' }</param>
        /// <returns></returns>
        [HttpPut("overrule")]
        public async Task<dynamic> OverruleAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.woId is null)
                throw new ArgumentNullException(nameof(paramJson));

            int woId = paramJson.woId;
            var entity = await workOrderService.ModelAsync(expression: src => src.Id == woId);

            if (entity is null)
                return new { success = false, message = "工单不存在" };

            var expression = new List<Expression<Func<WorkOrder, object>>> { src => src.Status };
            entity.Status = WorkOrderStatus.驳回;
            if(paramJson.remark is not null)
            {
                entity.Remark = paramJson.remark;
                expression.Add(src => src.Remark!);
            }
            bool success = await workOrderService.UpdateAsync(entity, expression.ToArray());
            return new { success, message = success ? "操作成功" : "操作失败" };
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="paramJson">{ 'id' : int }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpDelete("remove")]
        public async Task<dynamic> RemoveAsync([FromBody] dynamic paramJson)
        {
            if (paramJson == null || paramJson!.id is null)
                throw new ArgumentNullException(nameof(paramJson));
            int id = paramJson!.id;
            var entity = await workOrderService.ModelAsync(expression: src => src.Id == id);
            if (entity == null)
                return new { success = false, message = "工单不存在或已删除" };
            else if(entity.Status > 0)
                return new { success = false, message = "工单流程已开始，不能删除" };
            bool success = await workOrderService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="id">工单主键</param>
        /// <returns></returns>
        [HttpPut("cancel/{id:int}")]
        public async Task<dynamic> CancelAsync(int id)
        {
            return await workOrderService.CancelAsync(id);
        }
    }
}
