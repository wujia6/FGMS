using System.Diagnostics;
using System.Text;

namespace FGMS.Android.Api.Middleware
{
    /// <summary>
    /// 请求响应中间件
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private const int MaxBodyLength = 4096;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 启用请求体多次读取
            context.Request.EnableBuffering();

            // 读取请求体内容
            string requestBody = await ReadStreamAsync(context.Request.Body);
            requestBody = Truncate(requestBody, MaxBodyLength);

            // 记录请求信息
            _logger.LogInformation("HTTP Request {Method} {Path}{QueryString} Body: {Body}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                requestBody);

            // 记录请求处理时间
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            // 记录响应信息
            _logger.LogInformation("HTTP Response {StatusCode} responded in {ElapsedMilliseconds}ms",
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            //// 启用请求体多次读取
            //context.Request.EnableBuffering();

            //string requestBody = await ReadStreamAsync(context.Request.Body);
            //requestBody = Truncate(requestBody, MaxBodyLength);

            //_logger.LogInformation("HTTP Request {Method} {Path}{QueryString} Body: {Body}",
            //    context.Request.Method,
            //    context.Request.Path,
            //    context.Request.QueryString,
            //    requestBody);

            //// 替换响应体流，便于捕获响应内容
            //var originalResponseBody = context.Response.Body;
            //await using var memStream = new MemoryStream();
            //context.Response.Body = memStream;

            //var stopwatch = Stopwatch.StartNew();
            //await _next(context);
            //stopwatch.Stop();

            //// 读取响应体内容
            //memStream.Seek(0, SeekOrigin.Begin);
            //string responseBody = await new StreamReader(memStream).ReadToEndAsync();
            //responseBody = Truncate(responseBody, MaxBodyLength);
            //memStream.Seek(0, SeekOrigin.Begin);

            //// 将响应体写回真实流
            //await memStream.CopyToAsync(originalResponseBody);
            //context.Response.Body = originalResponseBody;

            //_logger.LogInformation("HTTP Response {StatusCode} responded in {ElapsedMilliseconds}ms Body: {Body}",
            //    context.Response.StatusCode,
            //    stopwatch.ElapsedMilliseconds,
            //    responseBody);
        }

        private static async Task<string> ReadStreamAsync(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            string text = await reader.ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);
            return text;
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...(truncated)";
        }
    }
}
