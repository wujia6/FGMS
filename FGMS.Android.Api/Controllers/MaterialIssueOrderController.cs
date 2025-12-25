using System.Linq.Expressions;
using FGMS.Android.Api.Filters;
using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Mx.Models;
using FGMS.Mx.Services;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// 发料单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/materialIssueOrder")]
    [PermissionAsync("m_material_issue_order_management", "management", "移动")]
    public class MaterialIssueOrderController : ControllerBase
    {
        private readonly IProductionOrderService productionOrderService;
        private readonly IMaterialIssueOrderService materialIssueOrderService;
        private readonly IBusinessService businessService;
        private readonly GenerateRandomNumber randomNumber;
        private readonly UserOnline userOnline;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionOrderService"></param>
        /// <param name="materialIssueOrderService"></param>
        /// <param name="businessService"></param>
        /// <param name="randomNumber"></param>
        /// <param name="userOnline"></param>
        /// <param name="mapper"></param>
        public MaterialIssueOrderController(
            IProductionOrderService productionOrderService,
            IMaterialIssueOrderService materialIssueOrderService,
            IBusinessService businessService,
            GenerateRandomNumber randomNumber,
            UserOnline userOnline,
            IMapper mapper)
        {
            this.productionOrderService = productionOrderService;
            this.materialIssueOrderService = materialIssueOrderService;
            this.businessService = businessService;
            this.randomNumber = randomNumber;
            this.userOnline = userOnline;
            this.mapper = mapper;
        }

        /// <summary>
        /// 获取发货单列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="equipmentId">机台ID</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> ListAsync(int? pageIndex, int? pageSize, int? equipmentId, string? status)
        {
            var expression = ExpressionBuilder.GetTrue<MaterialIssueOrder>()
                .AndIf(equipmentId.HasValue, src => src.ProductionOrder!.EquipmentId == equipmentId!.Value)
                .AndIf(!string.IsNullOrEmpty(status), src => src.Status == Enum.Parse<MioStatus>(status!));

            var query = materialIssueOrderService.GetQueryable(expression, include: src => src.Include(src => src.Sendor!).Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!))
                .OrderByDescending(src => src.Id)
                .AsNoTracking();

            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            var pageList = mapper.Map<List<MaterialIssueOrderDto>>(entities);
            return Ok(new { success = true, rows = pageList });
        }

        /// <summary>
        /// 扫码获取发料单信息
        /// </summary>
        /// <param name="barCode">出库物料码</param>
        /// <returns></returns>
        [HttpGet("sacn")]
        public async Task<IActionResult> ScanAsync(string barCode)
        {
            var mio = await materialIssueOrderService.ModelAsync(
                expression: src => src.MxBarCode!.Contains(barCode) && src.Status == MioStatus.已出库,
                include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!));

            if (mio is null)
                return BadRequest(new { success = false, message = "未知发料单" });

            var dto = mapper.Map<MaterialIssueOrderDto>(mio);
            return Ok(new { success = true, data = dto });
        }

        /// <summary>
        /// 创建发料单
        /// </summary>
        /// <param name="paramJson">{ 'ecode': 'string', 'poid': int, 'type': 'string', 'qty': int }</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.ecode is null || paramJson.poid is null || paramJson.type is null)
                return BadRequest(new { success = false, message = "参数错误" });

            StoragePosition? storagePosition = null;
            string type = paramJson.type, equCode = paramJson.ecode;
            int poid = paramJson.poid;
            int? qty = paramJson.qty;

            if (Enum.Parse<MioType>(type) == MioType.补料 && qty == null)
                return BadRequest(new { success = false, message = "补料数量不能为0" });

            var pos = await productionOrderService.ListAsync(expression: src => src.Equipment!.Code.Equals(equCode));
            var po = pos.FirstOrDefault(src => src.Id == poid);

            if (po is null)
                return BadRequest(new { success = false, message = "未知制令单" });

            if (Enum.Parse<MioType>(type) == MioType.发料)
            {
                //制令单顺序检查
                if (SequenceCheck(pos, poid))
                    return BadRequest(new { success = false, message = "请按制令单顺序叫料" });

                //工时超额检查
                if (ExcessiveCheck(pos, poid))
                    return BadRequest(new { success = false, message = "完工工时超出24H限制，无法叫料" });

                //获取墨心库存位置
                storagePosition = await businessService.GetStoragePositionsAsync(po.OrderNo);
            }

            var mio = new MaterialIssueOrder
            {
                ProductionOrderId = poid,
                CreateorId = userOnline.Id!.Value,
                Type = Enum.Parse<MioType>(type),
                OrderNo = $"MIO{randomNumber.CreateOrderNum()}",
                MaterialNo = po.MaterialCode,
                MaterialName = po.MaterialName,
                MaterialSpce = po.MaterialSpec,
                Quantity = Enum.Parse<MioType>(type) == MioType.发料 ? po.Quantity : qty.Value,
                MxWareHouse = storagePosition?.Warehouse ?? null,
                MxCargoSpace = storagePosition?.CargoSpace ?? null
            };
            po.Status = ProductionOrderStatus.待发料;
            bool success = await materialIssueOrderService.AddAsync(mio);
            if (success && Enum.Parse<MioType>(type) == MioType.发料)
                await productionOrderService.UpdateAsync(po, new Expression<Func<ProductionOrder, object>>[] { src => src.Status });
            return success ? Ok(new { success = true, message = $"已创建发料单：{mio.OrderNo}" }) : BadRequest(new { success = false, message = "叫料失败" });
        }

        /// <summary>
        /// 机台接收
        /// </summary>
        /// <param name="paramJson">{ 'mioid': int }</param>
        /// <returns></returns>
        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.mioid is null)
                return BadRequest(new { success = false, message = "参数错误" });
            int mioId = paramJson.mioid;
            int operatorId = userOnline.Id!.Value;
            return Ok(await materialIssueOrderService.EquipmentReceiveAsync(mioId, operatorId));
        }

        /// <summary>
        /// 分拣完成
        /// </summary>
        /// <param name="paramJson">{ 'ids':[int] }</param>
        /// <returns></returns>
        [HttpPut("sorted")]
        public async Task<IActionResult> SortedAsync([FromBody] dynamic paramJson)
        {
            if(paramJson is null || paramJson.ids is null)
                return BadRequest(new { success = false, message = "参数错误" });

            int[] ids = JsonConvert.DeserializeObject<int[]>(paramJson.ids.ToString());
            var mios = await materialIssueOrderService.ListAsync(src => ids.Contains(src.Id));

            if (!mios.Any())
                return BadRequest(new { success = false, message = "未知发料单" });

            mios.ForEach(src => src.Status = MioStatus.待出库);
            bool success = await materialIssueOrderService.UpdateAsync(mios, new Expression<Func<MaterialIssueOrder, object>>[] { src => src.Status });
            return success ? Ok(new { success = true, message = "分拣完成，发料单状态已更新" }) : BadRequest(new { success = false, message = "操作失败" });
        }

        //制令单顺序检查
        private static bool SequenceCheck(List<ProductionOrder> pos, int poId)
        {
            //如果没有找到任何制令单，直接返回true
            if (pos is null || pos.Count == 0)
                return true;

            //过滤出比当前制令单ID小的制令单，检查其状态是否都为已排配
            bool correct = pos.Where(src => src.Id < poId).Any(src => src.Status == ProductionOrderStatus.已排配);
            return correct;
        }

        //工时超额检查
        private static bool ExcessiveCheck(List<ProductionOrder> pos, int poId)
        {
            double totalHours = pos.Where(src => src.Status != ProductionOrderStatus.已排配 && src.Status != ProductionOrderStatus.已完成).Sum(src => src.WorkHours!.Value);
            totalHours += pos.FirstOrDefault(src => src.Id == poId)!.WorkHours!.Value;
            return totalHours > 24;
        }
    }
}
