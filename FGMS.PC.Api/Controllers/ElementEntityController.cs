using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.PC.Api.Filters;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 工件接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/elemententity")]
    [PermissionAsync("element_management", "management", "电脑")]
    public class ElementEntityController : ControllerBase
    {
        private readonly IWebHostEnvironment webHost;
        private readonly IElementService elementService;
        private readonly IElementEntityService elementEntityService;
        private readonly IStandardService standardService;
        private readonly ITrackLogService trackLogService;
        private readonly QRCoderHelper coderHelper;
        private readonly GenerateRandomNumber randomNumber;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webHost"></param>
        /// <param name="elementService"></param>
        /// <param name="elementEntityService"></param>
        /// <param name="standardService"></param>
        /// <param name="trackLogService"></param>
        /// <param name="coderHelper"></param>
        /// <param name="randomNumber"></param>
        /// <param name="mapper"></param>
        public ElementEntityController(
            IWebHostEnvironment webHost,
            IElementService elementService,
            IElementEntityService elementEntityService,
            IStandardService standardService,
            ITrackLogService trackLogService,
            QRCoderHelper coderHelper,
            GenerateRandomNumber randomNumber,
            IMapper mapper)
        {
            this.webHost = webHost;
            this.elementService = elementService;
            this.elementEntityService = elementEntityService;
            this.standardService = standardService;
            this.trackLogService = trackLogService;
            this.coderHelper = coderHelper;
            this.randomNumber = randomNumber;
            this.mapper = mapper;
        }

        /// <summary>
        /// 工件集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="material">料号</param>
        /// <param name="code">编码</param>
        /// <param name="elementMaterialNo"></param>
        /// <param name="modal">型号</param>
        /// <param name="category">类型</param>
        /// <param name="status">状态</param>
        /// <param name="isGroup">是否组成员</param>
        /// <param name="startDate">报废日期（开始）</param>
        /// <param name="endDate">报废日期（结束）</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(
            int? pageIndex, int? pageSize, string? material, string? code, string? elementMaterialNo, string? modal, string? category, string? status, bool? isGroup, DateTime? startDate, DateTime? endDate)
        {
            var expression = ExpressionBuilder.GetTrue<ElementEntity>()
                .AndIf(!string.IsNullOrEmpty(material), src => src.MaterialNo.Contains(material!))
                .AndIf(!string.IsNullOrEmpty(code), src => src.Code!.Contains(code!))
                .AndIf(!string.IsNullOrEmpty(elementMaterialNo), src => src.MaterialNo.Contains(elementMaterialNo!))
                .AndIf(!string.IsNullOrEmpty(modal), src => src.Element!.ModalNo.Contains(modal!))
                .AndIf(!string.IsNullOrEmpty(category), src => src.Element!.Category == Enum.Parse<ElementCategory>(category!))
                .AndIf(!string.IsNullOrEmpty(status), src => src.Status == Enum.Parse<ElementEntityStatus>(status!))
                .AndIf(isGroup.HasValue, src => src.IsGroup == isGroup!.Value)
                .AndIf(startDate.HasValue && endDate.HasValue, src => src.DiscardTime >= startDate!.Value && src.DiscardTime <= endDate!.Value.AddHours(24));
            var query = elementEntityService.GetQueryable(
                expression,
                include: src => src.Include(src => src.Element!).ThenInclude(src => src.Brand!).Include(src => src.Component!).ThenInclude(src => src.WorkOrder!).Include(src => src.CargoSpace!))
                .OrderByDescending(src => src.Id)
                .AsNoTracking();
            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            return new { total, rows = mapper.Map<List<ElementEntityDto>>(entities) };
        }

        /// <summary>
        /// 按标准获取指定元件料号的工件
        /// </summary>
        /// <param name="standardId">标准ID</param>
        /// <returns></returns>
        [HttpGet("listbystandard")]
        public async Task<StandardElementsResult> ListByStandardAsync(int standardId)
        {
            var std = await standardService.ModelAsync(expression: src => src.Id == standardId);
            if (std is null)
                return new StandardElementsResult { Success = false, Message = "标准不存在" };

            var elementIds = GetElementIds(std);
            if (!elementIds.Any())
                return new StandardElementsResult { Success = true };

            var elements = await elementService.ListAsync(expression: src => elementIds.Contains(src.Id),include: src => src.Include(src => src.ElementEntities!));
            var elementDict = elements.ToDictionary(x => x.Id, x => x);

            return new StandardElementsResult
            {
                Success = true,
                Mains = GetElementEntitiesDto(elementDict, std.MainElementId),
                Firsts = GetElementEntitiesDto(elementDict, std.FirstElementId),
                Seconds = GetElementEntitiesDto(elementDict, std.SecondElementId),
                Thirds = GetElementEntitiesDto(elementDict, std.ThirdElementId),
                Fourths = GetElementEntitiesDto(elementDict, std.FourthElementId),
                Fifths = GetElementEntitiesDto(elementDict, std.FifthElementId)
            };
        }

        /// <summary>
        /// 生成工件
        /// </summary>
        /// <param name="paramJson">{ 'elementId':int, 'quantity': int }</param>
        /// <returns></returns>
        [HttpPost("build")]
        public async Task<dynamic> BuildAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.elementId is null || paramJson.quantity is null)
                return new { success = false, message = "参数错误" };

            int elementId = paramJson.elementId;
            int quantity = paramJson.quantity;
            var emt = await elementService.ModelAsync(expression: src => src.Id == elementId);
            var loglist = new List<TrackLog>();
            var eelist = new List<ElementEntity>();
            string root = webHost.ContentRootPath;
            string rootPath = Path.Combine(root, "wwwroot", "images");

            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            for (int i = 0; i < quantity; i++)
            {
                string randomNum = randomNumber.Create();
                //string code = emt.Category == ElementCategory.砂轮 ?
                //    $"{emt.ModalNo}-{emt.Diameter}-{emt.WheelWidth}-{emt.RingWidth}-{emt.Thickness}-{emt.Angle}-{emt.InnerBoreDiameter}-{emt.Granularity}-{randomNum}({emt.Binders})" :
                //    $"{emt.ModalNo}-{emt.Diameter}-{emt.Lengths}-{randomNum}";
                string code = $"{emt.Spec}-{randomNum}";
                string codeImage = await coderHelper.CreateAndSaveAsync(code, rootPath);
                eelist.Add(new ElementEntity 
                { 
                    ElementId = elementId,
                    MaterialNo = $"{emt.MaterialNo}-{randomNum}",
                    Code = code,
                    QrCodeImage = codeImage
                });
                loglist.Add(new TrackLog { Content = $"生成工件：{code} " });
            }
            bool success = await elementEntityService.AddAsync(eelist);
            if (success)
                await trackLogService.AddAsync(loglist);
            return new { success, message = success ? "生成成功" : "生成失败", data = success ? eelist : null };
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="dto">JSON</param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<dynamic> UpdateAsync([FromBody] ElementEntityDto dto)
        {
            if (dto is null)
                return new { success = false, message = "参数错误" };
            var entity = mapper.Map<ElementEntity>(dto);
            bool success = await elementEntityService.UpdateAsync(entity);
            if (success)
                await trackLogService.AddAsync(new TrackLog { Content = $"{entity.Code} 工件已更新" });
            return new { success, message = success ? "保存成功" : "保存失败" };
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="param">{ 'id' : int }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpDelete("remove")]
        public async Task<dynamic> RemoveAsync([FromBody] dynamic param)
        {
            if (param == null || param!.id is null) throw new ArgumentNullException(nameof(param));
            int eeId = param!.id;
            var entity = await elementEntityService.ModelAsync(expression: src => src.Id == eeId);
            if (entity == null) return new { success = false, message = "记录不存在或已删除" };
            bool success = await elementEntityService.RemoveAsync(entity);
            if (success)
                await trackLogService.AddAsync(new TrackLog { Content = $"{entity.Code} 工件已删除" });
            return new { success, message = success ? "删除成功" : "删除失败" };
        }

        private List<ElementEntityDto>? GetElementEntitiesDto(Dictionary<int, Element> elementDict,int? elementId)
        {
            if (!elementId.HasValue || !elementDict.TryGetValue(elementId.Value, out var element))
                return null;
            var entities = element.ElementEntities?.Where(src => src.IsGroup == false && src.Status == ElementEntityStatus.在库).ToList();
            return entities is null || !entities.Any() ? null : mapper.Map<List<ElementEntityDto>>(entities);
        }

        private static List<int> GetElementIds(Standard std) => new[]
        {
            std.MainElementId,
            std.FirstElementId,
            std.SecondElementId,
            std.ThirdElementId,
            std.FourthElementId,
            std.FifthElementId
        }
        .Where(id => id.HasValue)
        .Select(id => id!.Value)
        .Distinct()
        .ToList();
    }
}
