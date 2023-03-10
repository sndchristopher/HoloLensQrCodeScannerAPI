using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrController : ControllerBase
    {
        [HttpGet(Name = "GetQrResult")]
        public QrCodeResult Get()
        {
            QrCodeResult qrCodeResult = new QrCodeResult();
            qrCodeResult.Result = "Hello";

            return qrCodeResult;       
        }
    }
}
