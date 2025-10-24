using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// 工件接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/[controller]/[action]")]
    public class ElementEntityController : ControllerBase
    {
        private readonly IElementEntityService elementEntityService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementEntityService"></param>
        /// <param name="mapper"></param>
        public ElementEntityController(IElementEntityService elementEntityService, IMapper mapper)
        {
            this.elementEntityService = elementEntityService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 扫描工件编码
        /// </summary>
        /// <param name="code">编码</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<dynamic> ScanAsync(string code, string? status)
        {
            var expression = ExpressionBuilder.GetTrue<ElementEntity>().And(src => src.Code!.Equals(code));

            if (!string.IsNullOrEmpty(status))
                expression = expression.And(src => src.Status == (ElementEntityStatus)Enum.Parse(typeof(ElementEntityStatus), status));

            var entity = await elementEntityService.ModelAsync(expression, include: src => src.Include(src => src.Element!));

            if (entity == null)
                return new { success = false, message = "未知工件，或该工件已出库" };

            else if (entity.IsGroup && entity.ComponentId.HasValue)
            {
                var ees = await elementEntityService.ListAsync(expression: src => src.ComponentId == entity.ComponentId.Value, include: src => src.Include(src => src.Element!));
                return mapper.Map<List<ElementEntityDto>>(ees);
            }
            else
                return mapper.Map<List<ElementEntityDto>>(new List<ElementEntity> { entity });
        }
    }
}
