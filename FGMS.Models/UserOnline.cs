using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FGMS.Models
{
    public class UserOnline
    {
        public UserOnline(IHttpContextAccessor httpContextAccessor)
        {
            this.Id = int.Parse(httpContextAccessor.HttpContext.User.FindFirst("UserId")!.Value);
            this.RoleCode = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)!.Value ?? string.Empty;
            this.Name = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)!.Value ?? string.Empty;
            this.WorkNo = httpContextAccessor.HttpContext.User.FindFirst("WorkNo")!.Value ?? string.Empty;
        }

        public int? Id { get; private set; }
        public string RoleCode { get; private set; }
        public string Name { get; private set; }
        public string WorkNo { get; private set; }
    }
}
