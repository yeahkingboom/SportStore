using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SportStore.OrderQRCode
{
    public static class OrderQRCode
    {
        public static Bitmap GetQrCode(string domain,string controller,string action,long PaymentID)
        {
            Bitmap QrCodeImage;
            using(QRCodeGenerator qrCodeGenerator=new QRCodeGenerator())
            {
                var QrCodeData = qrCodeGenerator.CreateQrCode($"{domain}/{controller}/{action}/{PaymentID}", QRCodeGenerator.ECCLevel.H);
                QRCode qRCode = new QRCode(QrCodeData);
                QrCodeImage = qRCode.GetGraphic(20);
            }
            return QrCodeImage;
        }
    }
}
