using FGMS.Android.Api.Filters;
using FGMS.Models.Dtos;
using FGMS.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// 砂轮组接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/[controller]/[action]")]
    [PermissionAsync("m_standard_management", "management", "移动")]
    public class ComponentController : ControllerBase
    {
        private readonly IComponentService componentService;
        private readonly IElementEntityService elementEntityService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentService"></param>
        /// <param name="elementEntityService"></param>
        /// <param name="mapper"></param>
        public ComponentController(IComponentService componentService, IElementEntityService elementEntityService, IMapper mapper)
        {
            this.componentService = componentService;
            this.elementEntityService = elementEntityService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 按工件编码查询砂轮组
        /// </summary>
        /// <param name="elementEntityCode">工件编码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> FindByCodeAsync(string elementEntityCode)
        {
            var component = await componentService.ModelAsync(
                expression: src => src.ElementEntities!.Any(src => src.Code!.Equals(elementEntityCode)), 
                include: src => src.Include(src => src.ElementEntities!));

            if (component is null)
                return NotFound("未找到对应的砂轮组");

            return Ok(mapper.Map<ComponentDto>(component));
        }

        /// <summary>
        /// 砂轮组拆分
        /// </summary>
        /// <param name="elementEntityCode">工件编码</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> SplitAsync(string elementEntityCode)
        {
            var ee = await elementEntityService.ModelAsync(expression: src => src.Code!.Equals(elementEntityCode), include: src => src.Include(src => src.Component!));
            if (!ee.ComponentId.HasValue)
                return new { success = false, message = "未知砂轮组" };
            if (ee.Component!.IsStandard)
                return new { success = false, message = "手持端不能拆分标准砂轮组" };
            return await componentService.SplitAsync(ee.ComponentId.Value);
        }
    }
}
