using System.Text;
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
using MiniExcelLibs;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 元件接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/element")]
    public class ElementController : ControllerBase
    {
        private readonly IElementService elementService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementService"></param>
        /// <param name="mapper"></param>
        public ElementController(IElementService elementService, IMapper mapper)
        {
            this.elementService = elementService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 元件集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="category">类型</param>
        /// <param name="param1">品牌</param>
        /// <param name="param2">料号</param>
        /// <param name="param3">直径</param>
        /// <param name="param4">砂轮宽</param>
        /// <param name="param5">角度</param>
        /// <param name="param6">内孔直径</param>
        /// <param name="param7">粒度</param>
        /// <param name="param8">长度</param>
        /// <returns></returns>
        [HttpGet("list")]
        [PermissionAsync("material_management", "view", "电脑")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize, string? category, string? param1, string? param2, string? param3, string? param4, string? param5, string? param6, string? param7, string? param8)
        {
            //这个地方需要（品牌、料号、直径、砂轮宽、角度、内孔直径、砂轮粒度）
            var expression = ExpressionBuilder.GetTrue<Element>()
                .AndIf(!string.IsNullOrEmpty(category), src => src.Category == Enum.Parse<ElementCategory>(category))
                .AndIf(!string.IsNullOrEmpty(param1), src => src.Brand!.Name.Contains(param1))
                .AndIf(!string.IsNullOrEmpty(param2), src => src.MaterialNo.Contains(param2))
                .AndIf(!string.IsNullOrEmpty(param3), src => src.Diameter!.Contains(param3))
                .AndIf(!string.IsNullOrEmpty(param4), src => src.WheelWidth!.Contains(param4))
                .AndIf(!string.IsNullOrEmpty(param5), src => src.Angle!.Contains(param5))
                .AndIf(!string.IsNullOrEmpty(param6), src => src.InnerBoreDiameter!.Contains(param6))
                .AndIf(!string.IsNullOrEmpty(param7), src => src.Granularity!.Contains(param7))
                .AndIf(!string.IsNullOrEmpty(param8), src => src.Granularity!.Contains(param8));
            var query = elementService.GetQueryable(expression, include: src => src.Include(src => src.Brand!)).OrderBy(src => src.MaterialNo).AsNoTracking();
            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            return new { total, rows = mapper.Map<List<ElementDto>>(entities) };
        }

        /// <summary>
        /// 库存信息
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="category">类型</param>
        /// <param name="material">料号</param>
        /// <param name="modal">型号</param>
        /// <param name="spec">规格</param>
        [HttpGet("storagelist")]
        [PermissionAsync("material_management", "view", "电脑")]
        public async Task<dynamic> StorageListAsync(int? pageIndex, int? pageSize, string? category, string? material, string? modal, string? spec)
        {
            var expression = ExpressionBuilder.GetTrue<Element>()
                .AndIf(!string.IsNullOrEmpty(category), src => src.Category == Enum.Parse<ElementCategory>(category))
                .AndIf(!string.IsNullOrEmpty(material), src => src.MaterialNo.Contains(material))
                .AndIf(!string.IsNullOrEmpty(modal), src => src.ModalNo.Contains(modal))
                .AndIf(!string.IsNullOrEmpty(spec), src => src.Spec.Contains(spec));
            var query = elementService.GetQueryable(
                expression,
                include: src => src.Include(src => src.Brand!).Include(src => src.ElementEntities!.Where(dst => dst.Status != ElementEntityStatus.报废)).ThenInclude(dst => dst.CargoSpace!))
                .OrderByDescending(src => src.Id)
                .AsNoTracking();
            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            return new { total, rows = mapper.Map<List<ElementDto>>(entities) };
        }

        /// <summary>
        /// 添加/更新
        /// </summary>
        /// <param name="dto">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        [PermissionAsync("material_management", "management", "电脑")]
        public async Task<dynamic> SaveAsync([FromBody] ElementDto dto)
        {
            if (dto is null)
                return new { success = false, message = "参数错误" };
            var entity = mapper.Map<Element>(dto);
            bool success = entity.Id > 0 ? await elementService.UpdateAsync(entity) : await elementService.AddAsync(entity);
            return new { success, message = success ? "保存成功" : "保存失败" };
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("import")]
        [PermissionAsync("material_management", "management", "电脑")]
        public async Task<dynamic> ImportAsync([FromForm] UploadParam param)
        {
            if (param == null)
                return new { success = false, message = "参数错误" };

            string[] arr = { ".xls", ".xlsx" };
            if (!arr.Contains(Path.GetExtension(param.ExcelFile.FileName)))
                return new { success = false, message = "只能是excel文件" };

            try
            {
                using var stearm = param.ExcelFile.OpenReadStream();
                var dtos = stearm.Query<ElementDto>(startCell: "A1").ToList();

                if (!dtos.Any()) 
                    return new { success = false, message = "空数据" };

                var originals = mapper.Map<List<Element>>(dtos);
                var entities = new List<Element>();
                var repeats = new StringBuilder();
                originals.ForEach(e => 
                {
                    var exists = elementService.ModelAsync(expression: src => src.MaterialNo.Equals(e.MaterialNo)).Result;
                    if (exists is null)
                    {
                        e.BrandId = param.BrandId;
                        entities.Add(e);
                    }
                    else
                        repeats.Append(e.MaterialNo + ',');
                });
                bool success = await elementService.AddAsync(entities);
                if (repeats.Length > 0)
                {
                    repeats.Insert(0, "，重复料号：");
                    repeats.Append("已过滤");
                }
                return new { success, message = success ? "导入成功" + repeats.ToString().TrimEnd(',') : "导入失败" };
            }
            catch (Exception ex)
            {
                return new { success = false, code = 500, message = ex.Message };
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="param">{ 'id' : int }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpDelete("remove")]
        [PermissionAsync("material_management", "management", "电脑")]
        public async Task<dynamic> RemoveAsync([FromBody] dynamic param)
        {
            if (param == null || param!.id is null) throw new ArgumentNullException(nameof(param));
            int eleId = param!.id;
            var entity = await elementService.ModelAsync(expression: src => src.Id == eleId);
            if (entity == null) return new { success = false, message = "记录不存在或已删除" };
            bool success = await elementService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
