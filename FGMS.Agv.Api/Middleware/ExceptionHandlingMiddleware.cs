using System.Net;
using Newtonsoft.Json;

namespace FGMS.Agv.Api.Middleware
{
    /// <summary>
    /// 全局异常中间件
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// ioc
        /// </summary>
        /// <param name="next"></param>
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = new
            {
                success = false,
                statusCode = context.Response.StatusCode,
                message = "服务器内部错误", // 可以自定义错误消息
                detailed = exception.Message // 可选，详细信息（注意不要在生产环境中泄露详细信息）
            };
            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
