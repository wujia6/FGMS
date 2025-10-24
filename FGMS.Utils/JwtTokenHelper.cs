using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FGMS.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace FGMS.Utils
{
    public class JwtTokenHelper
    {
        private readonly IOptionsMonitor<JwtTokenOption> option;

        public JwtTokenHelper(IOptionsMonitor<JwtTokenOption> option)
        {
            this.option = option;
        }

        public string GetToken(dynamic appUser)
        {
            if (appUser == null) return "获取token失败";
            //解决dynamic对象跨程序集传值
            var user = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(appUser, Formatting.Indented));
            //有效载荷
            var claims = new List<Claim>
            {
                new(type: "UserId",user.id.ToString()),
                new(type: "UserName", user.name.ToString()),
                new(type: "RoleCode", user.roleCode.ToString()),
                new(type: "RoleName", user.roleName.ToString())
            };
            //生成token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.CurrentValue.SecurityKey!));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: option.CurrentValue.Issuer!,
                audience: option.CurrentValue.Audience!,
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(option.CurrentValue.ExpireMinutes),
                signingCredentials: signingCredentials);
            string returnToken = new JwtSecurityTokenHandler().WriteToken(token);
            return "Bearer " + returnToken;
        }
    }
}
