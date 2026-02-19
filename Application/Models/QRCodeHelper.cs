using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace CommUnityApp.ApplicationCore.Models
{
    public static class QRCodeHelper
    {
        public static void GenerateQRCode(string qrText, string fullPath)
        {
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(data);
            using var bitmap = qrCode.GetGraphic(20);
            bitmap.Save(fullPath, ImageFormat.Png);
        }
    }
}
