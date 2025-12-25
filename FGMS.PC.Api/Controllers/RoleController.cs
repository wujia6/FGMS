using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.PC.Api.Filters;
using FGMS.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 角色接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/role")]
    [PermissionAsync("role_management", "management", "电脑")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleInfoService roleInfoService;
        private readonly IPermissionInfoService permissionInfoService;
        private readonly IMenuInfoService menuInfoService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleInfoService"></param>
        /// <param name="permissionInfoService"></param>
        /// <param name="menuInfoService"></param>
        /// <param name="mapper"></param>
        public RoleController(IRoleInfoService roleInfoService, IPermissionInfoService permissionInfoService, IMenuInfoService menuInfoService, IMapper mapper)
        {
            this.roleInfoService = roleInfoService;
            this.permissionInfoService = permissionInfoService;
            this.menuInfoService = menuInfoService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize)
        {
            var result = await roleInfoService.ListAsync(expression: src => !src.Code.Equals("Admin"), include: src => src.Include(src => src.Organize!));
            int total = result.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                result = result.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<RoleInfoDto>>(result) };
        }

        /// <summary>
        /// 角色权限
        /// </summary>
        /// <param name="roleInfoId">角色主键</param>
        /// <param name="client">客户端</param>
        /// <returns></returns>
        [HttpGet("permissions")]
        public async Task<IActionResult> PermissionsAsync(int roleInfoId, string client)
        {
            var permissions = await permissionInfoService.ListAsync(expression: src => src.RoleInfoId == roleInfoId && src.MenuInfo!.Client == Enum.Parse<ClientType>(client));
            var menus = await menuInfoService.ListAsync(expression: src => src.Client == Enum.Parse<ClientType>(client));

            List<dynamic> menuPermissions = new();
            menus.ForEach(m =>
            {
                var pInfo = permissions.FirstOrDefault(src => src.MenuInfoId == m.Id && src.RoleInfoId == roleInfoId);
                menuPermissions.Add(new 
                { 
                    m.Id,
                    m.ParentId,
                    m.Code,
                    m.Name,
                    m.Path,
                    m.Icon,
                    m.IsVisible,
                    permissions = new 
                    {
                        canView = pInfo is not null && pInfo.CanView,
                        canManagement = pInfo is not null && pInfo.CanManagement,
                        canAudits = pInfo is not null && pInfo.CanAudits
                    }
                });
            });
            return Ok(menuPermissions);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<dynamic> SaveAsync([FromBody] RoleInfoDto model)
        {
            var entity = mapper.Map<RoleInfo>(model);
            bool success = entity.Id > 0 ? await roleInfoService.UpdateAsync(entity) : await roleInfoService.AddAsync(entity);
            return new { success, message = success ? "保存成功" : "保存失败" };
        }

        /// <summary>
        /// 授权
        /// </summary>
        /// <param name="dtos">JSON</param>
        /// <returns></returns>
        [HttpPost("auth")]
        public async Task<IActionResult> AuthAsync([FromBody] List<PermissionInfoDto> dtos)
        {
            int roleId = dtos.FirstOrDefault()!.RoleInfoId;
            var permissions = await permissionInfoService.ListAsync(expression: src => src.RoleInfoId == roleId && src.MenuInfo!.Client == Models.ClientType.电脑);

            if (permissions != null && permissions.Any())
                await permissionInfoService.RemoveAsync(permissions);

            var entities = mapper.Map<List<PermissionInfo>>(dtos);
            bool success = await permissionInfoService.AddAsync(entities);
            return success ? Ok(new { success, message = "授权成功" }) : BadRequest(new { success, message = "授权失败" });
        }

        /// <summary>
        /// 移动端授权
        /// </summary>
        /// <param name="dtos">JSON</param>
        /// <returns></returns>
        [HttpPost("mobileAuth")]
        public async Task<dynamic> MobileAuthAsync([FromBody] List<PermissionInfoDto> dtos)
        {
            int roleId = dtos.FirstOrDefault()!.RoleInfoId;
            var permissions = await permissionInfoService.ListAsync(expression: src => src.RoleInfoId == roleId && src.MenuInfo!.Client == Models.ClientType.移动);

            if (permissions != null && permissions.Any())
                await permissionInfoService.RemoveAsync(permissions);

            var entities = mapper.Map<List<PermissionInfo>>(dtos);
            bool success = await permissionInfoService.AddAsync(entities);
            return success ? Ok(new { success, message = "授权成功" }) : BadRequest(new { success, message = "授权失败" });
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
            int roleId = param!.id;
            var entity = await roleInfoService.ModelAsync(expression: src => src.Id == roleId);
            if (entity == null) return new { success = false, message = "记录不存在或已删除" };
            bool success = await roleInfoService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
