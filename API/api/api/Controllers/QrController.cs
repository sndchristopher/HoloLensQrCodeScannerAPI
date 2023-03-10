using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MessagingToolkit.QRCode;
using MessagingToolkit.QRCode.Codec.Data;
using System.Drawing;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrController : ControllerBase
    {
        [HttpGet(Name = "GetQrResult")]
        public QrCodeResult Get(QrCodeRequest qrCodeRequest)
        {
            QrCodeResult qrCodeResult = new QrCodeResult();
            qrCodeResult.Result = "Hello";

            MessagingToolkit.QRCode.Codec.QRCodeDecoder t = new MessagingToolkit.QRCode.Codec.QRCodeDecoder();

           Bitmap
            QRCodeBitmapImage bit = new QRCodeBitmapImage(qrCodeRequest.Code);

            t.Decode(bit);
            return qrCodeResult;       
        }
    }
}
