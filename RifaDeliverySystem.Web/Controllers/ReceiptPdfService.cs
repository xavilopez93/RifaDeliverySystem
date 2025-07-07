using Microsoft.AspNetCore.Mvc;

namespace RifaDeliverySystem.Web.Controllers
{
    public class ReceiptPdfService : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
