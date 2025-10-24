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
        private readonly ConfigHelper configHelper;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInfoService"></param>
        /// <param name="configHelper"></param>
        /// <param name="mapper"></param>
        public SecurityController(IUserInfoService userInfoService, ConfigHelper configHelper, IMapper mapper)
        {
            this.userInfoService = userInfoService;
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
            var userInfo = await userInfoService.ModelAsync(expression: src => src.WorkNo.Equals(workNo) && src.Password.Equals(password), include: src => src.Include(src => src.RoleInfo!));

            if (userInfo == null) return new { success = false, message = "错误的用户名密码" };

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
    }
}
