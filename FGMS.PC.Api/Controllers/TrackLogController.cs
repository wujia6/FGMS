using FGMS.Models.Dtos;
using FGMS.Models.Entities;
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
    /// 日志接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/tracklog")]
    public class TrackLogController : ControllerBase
    {
        private readonly ITrackLogService trackLogService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackLogService"></param>
        /// <param name="mapper"></param>
        public TrackLogController(ITrackLogService trackLogService, IMapper mapper)
        {
            this.trackLogService = trackLogService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="type">类别</param>
        /// <param name="name">名称</param>
        /// <param name="code">编码</param>
        /// <param name="spec">规格</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize, string? type, string? name, string? code, string? spec, DateTime? startDate, DateTime? endDate)
        {
            var expression = ExpressionBuilder.GetTrue<TrackLog>()
                .AndIf(!string.IsNullOrEmpty(type), src => src.Type.GetDisplayName() == type)
                .AndIf(!string.IsNullOrEmpty(name), src => src.Content.Contains(name!))
                .AndIf(!string.IsNullOrEmpty(code), src => src.Content.Contains(code!))
                .AndIf(!string.IsNullOrEmpty(spec), src => src.Content.Contains(spec!))
                .AndIf(startDate.HasValue && endDate.HasValue, src => src.Date >= startDate!.Value && src.Date <= endDate!.Value.AddHours(24));
            var query = trackLogService.GetQueryable(expression).OrderByDescending(src => src.Id).AsNoTracking();
            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex!.Value - 1) * pageSize!.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            return new { total, rows = mapper.Map<List<TrackLogDto>>(entities) };
        }
    }
}
