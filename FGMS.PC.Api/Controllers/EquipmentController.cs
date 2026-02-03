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
using Microsoft.OpenApi.Extensions;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 设备接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/equipment")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentService equipmentService;
        private readonly IElementEntityService elementEntityService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipmentService"></param>
        /// <param name="elementEntityService"></param>
        /// <param name="mapper"></param>
        public EquipmentController(IEquipmentService equipmentService, IElementEntityService elementEntityService, IMapper mapper)
        {
            this.equipmentService = equipmentService;
            this.elementEntityService = elementEntityService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <returns></returns>
        [HttpGet("list")]
        [PermissionAsync("equipment_management", "view", "电脑")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize)
        {
            var entities = await equipmentService.ListAsync(include: src => src.Include(src => src.Organize!));
            int total = entities.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                entities = entities.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<EquipmentDto>>(entities) };
        }

        /// <summary>
        /// 查询机台挂载的工单、组、工件
        /// </summary>
        /// <param name="eeCode">砂轮编码</param>
        /// <param name="equCode">设备编码</param>
        /// <param name="areaCode">区域</param>
        /// <returns></returns>
        [HttpGet("mountList")]
        public async Task<dynamic> MountListAsync(string? eeCode, string? equCode, string areaCode = "A")
        {
            var equipments = await equipmentService.ListAsync(
                expression: src => src.Code.Contains(areaCode), 
                include: src => src.Include(src => src.ProductionOrders!)
                    .ThenInclude(src => src.WorkOrder!)
                    .ThenInclude(src => src.Components!)
                    .ThenInclude(src => src.ElementEntities!));

            var dtos = mapper.Map<List<EquipmentDto>>(equipments);
            var equDtos = dtos.Select(e => new
            {
                e.Name,
                e.OrganizeName,
                e.Code,
                e.Enabled,
                e.Mount,
                WorkOrderDtos = e.ProductionOrderDtos!.Select(src => src.WorkOrderDto).OfType<WorkOrderDto>().Where(src => src.Status == "机台接收")
            });

            if (!string.IsNullOrEmpty(eeCode))
                return equDtos.Where(enu => enu.WorkOrderDtos.Any(wo => wo.ComponentDtos!.Any(cmp => cmp.ElementEntityDtos!.Any(ee => ee.Code!.Equals(eeCode)))));

            if (!string.IsNullOrEmpty(equCode))
                return equDtos.Where(enu => enu.Code.Equals(equCode));

            return equDtos;
        }

        /// <summary>
        /// 机台实时砂轮工单
        /// </summary>
        /// <param name="areaCode">区域编码</param>
        /// <param name="mount">是否挂载</param>
        /// <returns></returns>
        [HttpGet("imminentList")]
        [PermissionAsync("equipment_management", "view", "电脑")]
        public async Task<dynamic> ImminentListAsync(string? areaCode, bool? mount)
        {
            var expression = ExpressionBuilder.GetTrue<Equipment>()
                .AndIf(!string.IsNullOrEmpty(areaCode), src => src.Code.Contains(areaCode!))
                .AndIf(mount.HasValue, src => src.Mount == mount!.Value);

            var equipments = await equipmentService.ListAsync(
                expression,
                include: src => src.Include(src => src.ProductionOrders!)
                    .ThenInclude(src => src.WorkOrder!)
                    .ThenInclude(src => src.Components!)
                    .ThenInclude(src => src.ElementEntities!));

            var returnList = new List<dynamic>();
            foreach (var equipment in equipments)
            {
                string currentUpper = string.Empty;
                dynamic receiveList = new List<string>();
                dynamic downList = new List<string>();
                if (equipment.ProductionOrders is not null && equipment.ProductionOrders.Any())
                {
                    var wos = equipment.ProductionOrders!
                        .Where(src => src.WorkOrder != null && src.WorkOrder.Status == WorkOrderStatus.机台接收)
                        .Select(src => src.WorkOrder!);

                    if (wos.Any())
                    {
                        var cmps = wos.SelectMany(src => src.Components!);
                        var ees = cmps.SelectMany(src => src.ElementEntities!);
                        if (cmps != null && cmps.Any() && ees != null && ees.Any())
                        {
                            currentUpper = ees.FirstOrDefault(src => src.Status == ElementEntityStatus.上机) == null 
                                ? string.Empty 
                                : ees.FirstOrDefault(src => src.Status == ElementEntityStatus.上机)!.Component!.WorkOrder!.OrderNo;
                            receiveList = wos.Select(src => src.OrderNo);
                            downList = wos.Where(src => src.Components!.Where(src => src.ElementEntities!.Where(src => src.Status == ElementEntityStatus.下机).Any()).Any()).Select(src => src.OrderNo);
                        }
                    }
                }
                var equ = new { equipment.Name, equipment.Code, equipment.Enabled, equipment.Mount, currentUpper, receiveList, downList };
                returnList.Add(equ);
            }
            return new { total = returnList.Count, rows = returnList };
        }

        /// <summary>
        /// 机台实时制令单
        /// </summary>
        /// <param name="areaCode">区域</param>
        /// <returns></returns>
        [HttpGet("imminentRecords")]
        [PermissionAsync("equipment_management", "view", "电脑")]
        public async Task<IActionResult> ImminentRecordsAsync(string areaCode = "A")
        {
            var machines = await equipmentService.ListAsync(
                expression: src => src.Code.Contains(areaCode),
                include: src => src
                    .Include(src => src.ProductionOrders!.Where(src => src.Status != ProductionOrderStatus.已排配 && src.Status != ProductionOrderStatus.已完成))
                        .ThenInclude(src => src.MaterialIssueOrders!)
                    .Include(src => src.ProductionOrders!)
                        .ThenInclude(src => src.WorkOrder!));

            return Ok(machines.Select(machine => new
            {
                machine.Name,
                machine.Code,
                machine.Enabled,
                ProductionOrders = machine.ProductionOrders!.Select(po => new
                {
                    po.Id,
                    po.OrderNo,
                    //po.MaterialName,
                    //po.MaterialSpec,
                    //po.Quantity,
                    Status = po.Status.GetDisplayName(),
                    po.WorkHours,
                    MaterialIssueOrders = po.MaterialIssueOrders!.Select(mio => new
                    {
                        mio.Id,
                        mio.OrderNo,
                        Status = mio.Status.GetDisplayName()
                    }),
                    WorkOrder = po.WorkOrder == null ? null : new
                    {
                        po.WorkOrder.Id,
                        po.WorkOrder.OrderNo,
                        Status = po.WorkOrder.Status.GetDisplayName()
                    }
                })
            }));
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        [PermissionAsync("equipment_management", "management", "电脑")]
        public async Task<dynamic> SaveAsync([FromBody] EquipmentDto model)
        {
            var entity = mapper.Map<Equipment>(model);
            bool success = entity.Id > 0 ? await equipmentService.UpdateAsync(entity) : await equipmentService.AddAsync(entity);
            return new { success, message = success ? "保存成功" : "保存失败" };
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="param">{ 'id' : int }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpDelete("remove")]
        [PermissionAsync("equipment_management", "management", "电脑")]
        public async Task<dynamic> RemoveAsync([FromBody] dynamic param)
        {
            if (param == null || param!.id is null) throw new ArgumentNullException(nameof(param));
            int brandId = param!.id;
            var entity = await equipmentService.ModelAsync(expression: src => src.Id == brandId);
            if (entity == null) return new { success = false, message = "记录不存在或已删除" };
            bool success = await equipmentService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
