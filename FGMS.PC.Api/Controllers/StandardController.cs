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
    /// 标准组接口
    /// </summary>
    //[Authorize]
    [ApiController]
    [Route("fgms/pc/standard")]
    public class StandardController : ControllerBase
    {
        private readonly IStandardService standardService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="standardService"></param>
        /// <param name="mapper"></param>
        public StandardController(IStandardService standardService, IMapper mapper)
        {
            this.standardService = standardService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 标准集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="code">标准码</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize, string? code)
        {
            var expression = ExpressionBuilder.GetTrue<Standard>().AndIf(!string.IsNullOrEmpty(code), src => src.Code.Contains(code!));
            var query = standardService.GetQueryable(expression)
                .Include(src => src.MainElement)
                .Include(src => src.FirstElement!)
                .Include(src => src.SecondElement!)
                .Include(src => src.ThirdElement!)
                .Include(src => src.FourthElement!)
                .Include(src => src.FifthElement!)
                .OrderByDescending(src => src.Id)
                .AsNoTracking();
            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            return new { total, rows = mapper.Map<List<StandardDto>>(entities) };
        }

        /// <summary>
        /// 添加/更新
        /// </summary>
        /// <param name="dto">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<dynamic> SaveAsync([FromBody] StandardDto dto)
        {
            if (dto is null)
                return new { success = false, message = "参数错误" };
            var entity = mapper.Map<Standard>(dto);
            bool success = entity.Id > 0 ? await standardService.UpdateAsync(entity) : await standardService.AddAsync(entity);
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
            int stdId = param!.id;
            var entity = await standardService.ModelAsync(expression: src => src.Id == stdId, include: src => src.Include(src => src.Components!));
            if (entity == null) 
                return new { success = false, message = "记录不存在或已删除" };
            if (entity.Components is not null && entity.Components.Any())
                return new { success = false, message = "已关联标准组，无法删除" };
            bool success = await standardService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
