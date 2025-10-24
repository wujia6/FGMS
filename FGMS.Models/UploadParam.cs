using Microsoft.AspNetCore.Http;

namespace FGMS.Models
{
    public class UploadParam
    {
        public int BrandId { get; set; }
        public IFormFile ExcelFile { get; set; }
    }
}
