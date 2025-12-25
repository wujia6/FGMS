using FGMS.Android.Api.Filters;
using FGMS.Services.Interfaces;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentService"></param>
        /// <param name="elementEntityService"></param>
        public ComponentController(IComponentService componentService, IElementEntityService elementEntityService)
        {
            this.componentService = componentService;
            this.elementEntityService = elementEntityService;
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
