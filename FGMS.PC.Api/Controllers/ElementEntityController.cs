using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
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
            var expression = ExpressionBuilder.GetTrue<ElementEntity>();
            if (!string.IsNullOrEmpty(material))
                expression = expression.And(src => src.MaterialNo.Contains(material));
            if (!string.IsNullOrEmpty(code))
                expression = expression.And(src => src.Code!.Contains(code));
            if (!string.IsNullOrEmpty(elementMaterialNo))
                expression = expression.And(src => src.MaterialNo.Contains(elementMaterialNo));
            if (!string.IsNullOrEmpty(modal))
                expression = expression.And(src => src.Element!.ModalNo.Contains(modal));
            if (!string.IsNullOrEmpty(category))
                expression = expression.And(src => src.Element!.Category == (ElementCategory)Enum.Parse(typeof(ElementCategory), category));
            if (!string.IsNullOrEmpty(status))
                expression = expression.And(src => src.Status == (ElementEntityStatus)Enum.Parse(typeof(ElementEntityStatus), status));
            if (isGroup.HasValue)
                expression = expression.And(src => src.IsGroup == isGroup.Value);
            if (startDate.HasValue && endDate.HasValue)
                expression = expression.And(src => src.DiscardTime >= startDate.Value && src.DiscardTime <= endDate.Value.AddHours(24));
            var entities = (await elementEntityService.ListAsync(
                expression, 
                include: src => src.Include(src => src.Element!).ThenInclude(src => src.Brand!).Include(src => src.Component!).ThenInclude(src => src.WorkOrder!).Include(src => src.CargoSpace!))).OrderByDescending(src => src.Id)
                .ToList();
            int total = entities.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                entities = entities.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<ElementEntityDto>>(entities) };
        }

        /// <summary>
        /// 按标准获取指定元件料号的工件
        /// </summary>
        /// <param name="standardId">标准ID</param>
        /// <returns></returns>
        [HttpGet("listbystandard")]
        public async Task<dynamic> ListByStandardAsync(int standardId)
        {
            var std = await standardService.ModelAsync(expression: src => src.Id == standardId);

            if (std is null)
                return new { success = false, message = "标准不存在" };

            var mainEntity = std.MainElementId.HasValue ? await elementService.ModelAsync(expression: src => src.Id == std.MainElementId!.Value, include: src => src.Include(src => src.ElementEntities!)) : null;
            var firstEntity = std.FirstElementId.HasValue ? await elementService.ModelAsync(expression: src => src.Id == std.FirstElementId!.Value, include: src => src.Include(src => src.ElementEntities!)) : null;
            var secondEntity = std.SecondElementId.HasValue ? await elementService.ModelAsync(expression: src => src.Id == std.SecondElementId, include: src => src.Include(src => src.ElementEntities!)) : null;
            var thirdEntity = std.ThirdElementId.HasValue ? await elementService.ModelAsync(expression: src => src.Id == std.ThirdElementId, include: src => src.Include(src => src.ElementEntities!)) : null;
            var fourthEntity = std.FourthElementId.HasValue ? await elementService.ModelAsync(expression: src => src.Id == std.FourthElementId, include: src => src.Include(src => src.ElementEntities!)) : null;
            var fifthEntity = std.FirstElementId.HasValue ? await elementService.ModelAsync(expression: src => src.Id == std.FifthElementId, include: src => src.Include(src => src.ElementEntities!)) : null;

            return new 
            {
                mains = mainEntity is null || mainEntity.ElementEntities is null || !mainEntity.ElementEntities.Any() ? null : mapper.Map<List<ElementEntityDto>>(mainEntity.ElementEntities!.Where(src => src.IsGroup == false && src.Status == ElementEntityStatus.在库)), 
                firsts = firstEntity is null || firstEntity.ElementEntities is null || !firstEntity.ElementEntities.Any() ? null : mapper.Map<List<ElementEntityDto>>(firstEntity.ElementEntities!.Where(src => src.IsGroup == false && src.Status == ElementEntityStatus.在库)),
                seconds = secondEntity is null || secondEntity.ElementEntities is null || !secondEntity.ElementEntities.Any() ? null : mapper.Map<List<ElementEntityDto>>(secondEntity.ElementEntities!.Where(src => src.IsGroup == false && src.Status == ElementEntityStatus.在库)),
                thirds = thirdEntity is null || thirdEntity.ElementEntities is null || !thirdEntity.ElementEntities.Any() ? null : mapper.Map<List<ElementEntityDto>>(thirdEntity.ElementEntities!.Where(src => src.IsGroup == false && src.Status == ElementEntityStatus.在库)),
                fourths = fourthEntity is null || fourthEntity.ElementEntities is null || !fourthEntity.ElementEntities.Any() ? null : mapper.Map<List<ElementEntityDto>>(fourthEntity.ElementEntities!.Where(src => src.IsGroup == false && src.Status == ElementEntityStatus.在库)),
                fifths = fifthEntity is null || fifthEntity.ElementEntities is null || !fifthEntity.ElementEntities.Any() ? null : mapper.Map<List<ElementEntityDto>>(fifthEntity.ElementEntities!.Where(src => src.IsGroup == false && src.Status == ElementEntityStatus.在库))
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
    }
}
