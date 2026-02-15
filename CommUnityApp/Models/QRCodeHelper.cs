using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;

namespace CommUnityApp.Models
{
    public class QRCodeHelper
    {
        public static void GenerateQRCode(
      string text,
      string savePath)
        {
            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(
                text,
                QRCodeGenerator.ECCLevel.Q);

            QRCode qrCode = new QRCode(data);

            using Bitmap bitmap = qrCode.GetGraphic(20);
            bitmap.Save(savePath, ImageFormat.Png);
        }
    }
}
