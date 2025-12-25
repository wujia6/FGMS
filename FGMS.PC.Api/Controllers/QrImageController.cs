using FGMS.PC.Api.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 二维码图接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/qrimage")]
    [PermissionAsync("element_management", "management", "电脑")]
    public class QrImageController : ControllerBase
    {
        private readonly string imageFolderPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        public QrImageController(IWebHostEnvironment env)
        {
            this.imageFolderPath = Path.Combine(env.ContentRootPath, "wwwroot", "images");
        }

        /// <summary>
        /// 二维码
        /// </summary>
        /// <param name="imageName">图片名称</param>
        /// <returns></returns>
        [HttpGet("getimage")]
        public async Task<IActionResult> GetImage(string imageName)
        {
            var filePath = Path.Combine(imageFolderPath, imageName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(); // 返回 404 Not Found
            var contentType = GetContentType(imageName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, contentType); // 返回图片文件 
        }

        /// <summary>
        /// 下载二维码图片
        /// </summary>
        /// <param name="fileName">图片名称</param>
        /// <returns></returns>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name must be provided.");

            var filePath = Path.Combine(imageFolderPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("文件不存在");

            var memory = new MemoryStream();
            try
            {
                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                await stream.CopyToAsync(memory);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"读取文件失败: {ex.Message}");
            }
            memory.Position = 0;
            // 根据实际情况确定Content-Type，可以用MimeMapping或者固定类型
            var contentType = "application/octet-stream";
            return File(memory, contentType, fileName);
        }

        private static string GetContentType(string imageName)
        {
            var extension = Path.GetExtension(imageName).ToLowerInvariant();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream" // 默认类型  
            };
        }
    }
}
