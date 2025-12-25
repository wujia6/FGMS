using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.PC.Api.Filters;
using FGMS.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 菜单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/menuInfo")]
    [PermissionAsync("menu_management", "management", "电脑")]
    public class MenuInfoController : ControllerBase
    {
        private readonly IMenuInfoService menuInfoService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuInfoService"></param>
        /// <param name="mapper"></param>
        public MenuInfoController(IMenuInfoService menuInfoService, IMapper mapper)
        {
            this.menuInfoService = menuInfoService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="clientType">客户端</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> ListAsync(int? pageIndex, int? pageSize, string clientType)
        {
            var query = menuInfoService.GetQueryable(expression : src => src.Client == Enum.Parse<ClientType>(clientType))
                .OrderBy(src => src.Id).ThenBy(src => src.ParentId)
                .AsNoTracking();
            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            return Ok(new { total, rows = mapper.Map<List<MenuInfoDto>>(entities) });
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="dto">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<IActionResult> SaveAsync([FromBody] MenuInfoDto dto)
        {
            var entity = mapper.Map<MenuInfo>(dto);
            var success = entity.Id > 0 ? await menuInfoService.UpdateAsync(entity) : await menuInfoService.AddAsync(entity);
            return success ? Ok(new { success, message = "保存成功" }) : BadRequest(new { success, message = "保存失败" });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveAsync(int id)
        {
            var entity = await menuInfoService.ModelAsync(expression: src => src.Id == id);
            if (entity == null) 
                return BadRequest(new { success = false, message = "记录不存在或已删除" });
            bool success = await menuInfoService.RemoveAsync(entity);
            return success ? Ok(new { success, message = "删除成功" }) : BadRequest(new { success, message = "删除失败" });
        }
    }
}
