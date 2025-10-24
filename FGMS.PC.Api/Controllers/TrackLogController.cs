using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var expression = ExpressionBuilder.GetTrue<TrackLog>();
            if (!string.IsNullOrEmpty(type))
                expression = expression.And(src => src.Type.GetDisplayName() == type);
            if (!string.IsNullOrEmpty(name))
                expression = expression.And(src => src.Content.Contains(name));
            if (!string.IsNullOrEmpty(code))
                expression = expression.And(src => src.Content.Contains(code));
            if (!string.IsNullOrEmpty(spec))
                expression = expression.And(src => src.Content.Contains(spec));
            if (startDate.HasValue && endDate.HasValue)
                expression = expression.And(src => src.Date >= startDate.Value && src.Date <= endDate.Value.AddHours(24));
            var entities = (await trackLogService.ListAsync(expression)).OrderByDescending(src => src.Id).ToList();
            int total = entities.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                entities = entities.Skip((pageIndex!.Value - 1) * pageSize!.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<TrackLogDto>>(entities) };
        }
    }
}
