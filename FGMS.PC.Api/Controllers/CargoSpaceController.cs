using System.Linq.Expressions;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.PC.Api.Filters;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 货位接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/cargospace")]
    [PermissionAsync("cargo_space_management", "management", "电脑")]
    public class CargoSpaceController : ControllerBase
    {
        private readonly ICargoSpaceService cargoSpaceService;
        private readonly IElementEntityService elementEntityService;
        private readonly ITrackLogService trackLogService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cargoSpaceService"></param>
        /// <param name="elementEntityService"></param>
        /// <param name="trackLogService"></param>
        /// <param name="mapper"></param>
        public CargoSpaceController(ICargoSpaceService cargoSpaceService, IElementEntityService elementEntityService, ITrackLogService trackLogService, IMapper mapper)
        {
            this.cargoSpaceService = cargoSpaceService;
            this.elementEntityService = elementEntityService;
            this.trackLogService = trackLogService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 货位集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="organizeId">组织外键</param>
        /// <param name="code">货位编码</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize, int? organizeId, string? code)
        {
            var expression = ExpressionBuilder.GetTrue<CargoSpace>()
                .AndIf(organizeId.HasValue, src => src.OrganizeId == organizeId!.Value)
                .AndIf(!string.IsNullOrEmpty(code), src => src.Code!.Contains(code!));

            var query = cargoSpaceService.GetQueryable(
                expression,
                include: src => src.Include(src => src.Organize!).Include(src => src.Parent!).Include(src => src.Childrens!))
                .OrderByDescending(x => x.Id)
                .AsNoTracking();

            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            return new { total, rows = mapper.Map<List<CargoSpaceDto>>(entities) };
        }

        /// <summary>
        /// 货位明细集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="cmpCode">组编码</param>
        /// <param name="stdCode">标准编码</param>
        /// <returns></returns>
        [HttpGet("details")]
        public async Task<dynamic> DetailsAsync(int? pageIndex, int? pageSize, string? cmpCode, string? stdCode)
        {
            var expression = ExpressionBuilder.GetTrue<CargoSpace>()
                .AndIf(!string.IsNullOrEmpty(cmpCode), src => src.Components!.Any(src => src.Code!.Contains(cmpCode!)))
                .AndIf(!string.IsNullOrEmpty(stdCode), src => src.Components!.Any(src => src.Standard!.Code.Contains(stdCode!)));

            var query = cargoSpaceService.GetQueryable(
                expression,
                include: src => src.Include(src => src.Organize!).Include(src => src.ElementEntities!).Include(src => src.Components!).ThenInclude(src => src.Standard!))
                .OrderByDescending(x => x.Id)
                .AsNoTracking();

            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var records =  await query.ToListAsync();
            return new { total, rows = mapper.Map<List<CargoSpaceDto>>(records) };
        }

        /// <summary>
        /// 添加/更新
        /// </summary>
        /// <param name="dto">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<dynamic> SaveAsync([FromBody] CargoSpaceDto dto)
        {
            if (dto is null)
                return new { success = false, message = "参数错误" };
            var entity = mapper.Map<CargoSpace>(dto);
            bool success = entity.Id > 0 ? await cargoSpaceService.UpdateAsync(entity) : await cargoSpaceService.AddAsync(entity);
            return new { success, message = success ? "保存成功" : "保存失败" };
        }

        /// <summary>
        /// 工件绑定到指定货位
        /// </summary>
        /// <param name="paramJson">{ 'csid': int, 'eeids': [int] }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost("bind")]
        public async Task<dynamic> BindAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.csid is null || paramJson.eeids is null)
                throw new ArgumentNullException(nameof(paramJson));

            int csId = paramJson.csid;
            int[] eeIds = JsonConvert.DeserializeObject<int[]>(paramJson.eeids.ToString());
            var cs = await cargoSpaceService.ModelAsync(expression: src => src.Id == csId, include: src => src.Include(src => src.Components!));
            if (cs.IsStandard && cs.Components != null)
            {
                return new { success = false, message = $"{cs.Name}无法存放多个标准砂轮组" };
            }
            var ees = await elementEntityService.ListAsync(expression: src => eeIds.Contains(src.Id));
            if (cs == null && !ees.Any())
                return new { success = false, message = "未找到相关货位或工件" };
            var logs = new List<TrackLog>();
            ees.ForEach(ee => 
            {
                ee.CargoSpaceId = csId;
                ee.CargoSpaceHistory = csId;
                ee.Status = Models.ElementEntityStatus.在库;
                logs.Add(new TrackLog { Content = $"工件：{ee.MaterialNo} 已入库，货位 {cs!.Code}" });
            });
            bool success = await elementEntityService.UpdateAsync(ees, new Expression<Func<ElementEntity, object>>[] { src => src.CargoSpaceId!, src => src.CargoSpaceHistory!, src => src.Status });
            if (success)
                await trackLogService.AddAsync(logs);
            return new { success, message = success ? "已绑定指定货位" : "绑定指定货位失败" };
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
            int csId = param!.id;
            var entity = await cargoSpaceService.ModelAsync(expression: src => src.Id == csId);

            if (entity == null) 
                return new { success = false, message = "记录不存在或已删除" };
            else if(entity.Code!.Equals("NSG"))
                return new { success = false, message = "非标砂轮组货位是系统内置，无法删除" };

            bool success = await cargoSpaceService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
