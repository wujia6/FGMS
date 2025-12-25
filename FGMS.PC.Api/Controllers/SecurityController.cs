using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using Microsoft.OpenApi.Extensions;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 登录校验接口
    /// </summary>
    [ApiController]
    [Route("fgms/pc/security")]
    public class SecurityController : ControllerBase
    {
        private readonly IUserInfoService userInfoService;
        private readonly IMenuInfoService menuInfoService;
        private readonly ConfigHelper configHelper;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInfoService"></param>
        /// <param name="menuInfoService"></param>
        /// <param name="configHelper"></param>
        /// <param name="mapper"></param>
        public SecurityController(IUserInfoService userInfoService, IMenuInfoService menuInfoService, ConfigHelper configHelper, IMapper mapper)
        {
            this.userInfoService = userInfoService;
            this.menuInfoService = menuInfoService;
            this.configHelper = configHelper;
            this.mapper = mapper;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="param">{ 'account' : 'string', 'password' : 'string' }</param>
        /// <returns></returns>
        [HttpPost("signin")]
        public async Task<dynamic> SignInAsync([FromBody] dynamic param)
        {
            if (param == null || param!.account == string.Empty || param!.password == string.Empty)
                return new { success = false, message = "参数错误" };

            string workNo = param!.account;
            //string password = EncryptHelper.DesEncrypt(param!.password.ToString());
            string password = param!.password;
            var userInfo = await userInfoService.ModelAsync(
                expression: src => src.WorkNo.Equals(workNo) && src.Password.Equals(password),
                include: src => src.Include(src => src.RoleInfo!).ThenInclude(src => src.PermissionInfos!.Where(src => src.MenuInfo!.Client == Models.ClientType.电脑)).ThenInclude(src => src.MenuInfo!));

            if (userInfo == null) return new { success = false, message = "错误的用户名密码" };

            dynamic menuTree;

            if (userInfo.RoleInfo!.Code.Equals("Admin"))
            {
                var menus = await menuInfoService.ListAsync(expression: src => src.Client == Models.ClientType.电脑);
                menuTree = await BuildFullMenuTreeAsync(menus);
            }
            else
            {
                var permissionInfos = userInfo.RoleInfo!.PermissionInfos!.OrderBy(m => m.Id).ToList();
                menuTree = await BuildMenuTreeAsync(permissionInfos);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, userInfo.RoleInfo!.Code),
                new("UserId", userInfo.Id.ToString()),
                new(ClaimTypes.Name, userInfo.Name),
                new("WorkNo", userInfo.WorkNo),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configHelper.GetAppSettings<string>("JwtTokenOption:SecurityKey")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: configHelper.GetAppSettings<string>("JwtTokenOption:Issuer"),
                audience: configHelper.GetAppSettings<string>("JwtTokenOption:Audience"),
                expires: DateTime.Now.AddMinutes(int.Parse(configHelper.GetAppSettings<string>("JwtTokenOption:ExpireMinutes"))),
                claims: claims,
                signingCredentials: creds);

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return new
            {
                data = new { userId = userInfo.Id, roleCode = userInfo.RoleInfo.Code, userName = userInfo.Name },
                menuTree,
                token = "Bearer " + new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [HttpPost("signout")]
        public async Task<dynamic> SignOutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "登出成功" });
        }

        private static async Task<List<MenuInfoDto>> BuildMenuTreeAsync(List<PermissionInfo> permissionInfos, int? parentId = null)
        {
            var tree = permissionInfos
                .Where(m => m.MenuInfo!.ParentId == parentId)
                .Select(m => new MenuInfoDto
                {
                    Id = m.Id,
                    ParentId = m.MenuInfo!.ParentId,
                    Client = m.MenuInfo!.Client.GetDisplayName(),
                    Name = m.MenuInfo!.Name,
                    Code = m.MenuInfo!.Code,
                    Path = m.MenuInfo!.Path,
                    Icon = m.MenuInfo!.Icon,
                    IsVisible = m.MenuInfo!.IsVisible,
                    //ParentDto = null,
                    ChildrenDtos = BuildMenuTreeAsync(permissionInfos, m.MenuInfo.Id).Result,
                    //PermissionInfoDto = m.MenuInfo.ParentId is null ? null : new PermissionInfoDto
                    //{
                    //    CanView = m.CanView,
                    //    CanManagement = m.CanManagement,
                    //    CanAudits = m.CanAudits
                    //}
                })
                .ToList();
            return await Task.FromResult(tree);
        }

        private static async Task<List<MenuInfoDto>> BuildFullMenuTreeAsync(List<MenuInfo> menuInfos, int? parentId = null)
        {
            var tree = menuInfos
                .Where(m => m.ParentId == parentId)
                .Select(m => new MenuInfoDto 
                {
                    Id = m.Id,
                    ParentId = m.ParentId,
                    Client = m.Client.GetDisplayName(),
                    Name = m.Name,
                    Code = m.Code,
                    Path = m.Path,
                    Icon = m.Icon,
                    IsVisible = m.IsVisible,
                    //ParentDto = null,
                    ChildrenDtos = BuildFullMenuTreeAsync(menuInfos, m.Id).Result,
                })
                .ToList();
            return await Task.FromResult(tree);
        }
    }
}
