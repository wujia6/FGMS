using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;

namespace FGMS.Utils
{
    public class QRCoderHelper
    {
        public async Task<string> CreateAndSaveAsync(string content, string savePath)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using Bitmap qrCodeImage = qrCode.GetGraphic(32);
            string fileName = $"{Guid.NewGuid():N}.png";
            string filePath = Path.Combine(savePath, fileName);
            // 使用流异步保存图像为PNG文件
            await Task.Run(() =>
            {
                using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                qrCodeImage.Save(stream, ImageFormat.Png);
            });
            return fileName;
        }
    }
}
