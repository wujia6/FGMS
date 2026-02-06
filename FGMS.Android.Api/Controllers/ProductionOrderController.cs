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
    /// 制令单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/productionOrder")]
    [PermissionAsync("m_production_order_management", "management", "移动")]
    public class ProductionOrderController : ControllerBase
    {
        private readonly IProductionOrderService productionOrderService;
        private readonly IMaterialIssueOrderService materialIssueOrderService;
        private readonly IBusinessService businessService;
        private readonly IMapper mapper;

        private readonly UserOnline userOnline;
        private readonly GenerateRandomNumber randomNumber;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionOrderService"></param>
        /// <param name="materialIssueOrderService"></param>
        /// <param name="businessService"></param>
        /// <param name="userOnline"></param>
        /// <param name="randomNumber"></param>
        /// <param name="mapper"></param>
        public ProductionOrderController(
            IProductionOrderService productionOrderService,
            IMaterialIssueOrderService materialIssueOrderService,
            IBusinessService businessService,
            IMapper mapper,
            UserOnline userOnline,
            GenerateRandomNumber randomNumber)
        {
            this.productionOrderService = productionOrderService;
            this.materialIssueOrderService = materialIssueOrderService;
            this.businessService = businessService;
            this.mapper = mapper;
            this.userOnline = userOnline;
            this.randomNumber = randomNumber;
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
        /// 扫码获取发料单信息
        /// </summary>
        /// <param name="barcode">出库物料码</param>
        /// <returns></returns>
        [HttpGet("scan-barcode")]
        public async Task<IActionResult> ScanBarcodeAsync(string barcode)
        {
            var mio = await materialIssueOrderService.ModelAsync(
                expression: src => src.MxBarCode!.Contains(barcode) && src.Status == MioStatus.已出库,
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
        [HttpPost("create-issue-order")]
        public async Task<IActionResult> CreateIssueOrderAsync([FromBody] dynamic paramJson)
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

                if (storagePosition == null || string.IsNullOrEmpty(storagePosition.OutStoreOrderCode))
                    return BadRequest(new { success = false, message = "墨心系统中未找到对应的出库申请，无法叫料" });
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
                MxCargoSpace = storagePosition?.CargoSpace ?? null,
                MxOutStoreOrderNo = storagePosition?.OutStoreOrderCode ?? null
            };
            po.Status = ProductionOrderStatus.待发料;
            bool success = await materialIssueOrderService.AddAsync(mio);
            if (success && Enum.Parse<MioType>(type) == MioType.发料)
                await productionOrderService.UpdateAsync(po, new Expression<Func<ProductionOrder, object>>[] { src => src.Status });
            return success ? Ok(new { success = true, message = $"已创建发料单：{mio.OrderNo}" }) : BadRequest(new { success = false, message = "叫料失败" });
        }

        /// <summary>
        /// 接收发料单
        /// </summary>
        /// <param name="paramJson">{ 'mioid': int }</param>
        /// <returns></returns>
        [HttpPost("receive-issue-order")]
        public async Task<IActionResult> ReceiveIssueOrderAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.mioid is null)
                return BadRequest(new { success = false, message = "参数错误" });
            int mioId = paramJson.mioid;
            int operatorId = userOnline.Id!.Value;
            return Ok(await materialIssueOrderService.EquipmentReceiveAsync(mioId, operatorId));
        }

        /// <summary>
        /// 开工
        /// </summary>
        /// <param name="paramJson">{ 'poid': int }</param>
        /// <returns></returns>
        [HttpPost("madestart")]
        public async Task<IActionResult> MadeStartAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.poid is null)
                return BadRequest(new { success = false, message = "参数错误" });

            var result = await productionOrderService.MadeBeginAsync((int)paramJson.poid);
            var resultJson = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result));
            bool success = resultJson.success;
            if (success)
            {
                string orderNo = resultJson.data;
                await businessService.UpdateProductionOrderStatus(orderNo, "生产中");   // 同步状态到MES
            }
            return success ? Ok(new { success, resultJson.message }) : BadRequest(new { success, resultJson.message });
        }

        ///// <summary>
        ///// 机台变更
        ///// </summary>
        ///// <param name="paramJson">{ 'poId': int, 'newEquId': int, 'oldEquCode': 'string', 'reason': 'string' }</param>
        ///// <returns></returns>
        //[HttpPost("equipment-change")]
        //public Task<IActionResult> EquipmentChangeAsync([FromBody] dynamic paramJson)
        //{
        //    if (paramJson is null || paramJson.poId is null || paramJson.newEquId is null || paramJson.oldEquCode is null || paramJson.reason is null)
        //        return Task.FromResult<IActionResult>(BadRequest(new { success = false, message = "参数错误" }));

        //    int poId = paramJson.poId;
        //    string newEquId = paramJson.newEquId;
        //    string oldEquCode = paramJson.oldEquCode;
        //    string reason = paramJson.reason;
        //}

        //制令单顺序检查
        private static bool SequenceCheck(List<ProductionOrder> pos, int poId)
        {
            //如果没有找到任何制令单，直接返回true
            if (pos is null || !pos.Any())
                return true;

            //过滤出比当前制令单ID小的制令单，检查其状态是否都为已排配
            bool correct = pos.Where(src => src.Id < poId && !src.IsDc!.Value).Any(src => src.Status == ProductionOrderStatus.已排配);
            return correct;
        }

        //工时超额检查
        private static bool ExcessiveCheck(List<ProductionOrder> pos, int poId)
        {
            if (pos is null || !pos.Any())
                return false;

            double totalHours = pos.Where(src => src.Status != ProductionOrderStatus.已排配 && src.Status != ProductionOrderStatus.已完成).Sum(src => src.WorkHours!.Value);
            if (totalHours > 24)
                return true;
            return false;
            //totalHours += pos.FirstOrDefault(src => src.Id == poId)!.WorkHours!.Value;
            //return totalHours > 24;
        }
    }
}
