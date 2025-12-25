using System.Security.Claims;
using FGMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FGMS.Android.Api.Filters
{
    /// <summary>
    /// 异步权限过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class PermissionAsyncAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string menuCode;
        private readonly string permissionType;
        private readonly string clientType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuCode"></param>
        /// <param name="permissionType"></param>
        /// <param name="clientType"></param>
        public PermissionAsyncAttribute(string menuCode, string permissionType, string clientType)
        {
            this.menuCode = menuCode;
            this.permissionType = permissionType;
            this.clientType = clientType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            if (!user.Identity!.IsAuthenticated)
            {
                context.Result = new JsonResult(new { success = false, message = "未授权" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // 检查角色声明是否存在
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                context.Result = new JsonResult(new { success = false, message = "角色信息缺失" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            string roleCode = roleClaim.Value;

            // Admin 角色跳过权限检查
            if (string.Equals(roleCode, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var permissionInfoService = httpContext.RequestServices.GetService<IPermissionInfoService>();
            if (!await permissionInfoService!.CheckPermissionAsync(roleCode, menuCode, clientType, permissionType))
            {
                context.Result = new JsonResult(new { success = false, message = "没有权限访问此资源" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
    }
}
