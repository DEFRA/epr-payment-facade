using Microsoft.AspNetCore.Mvc;

namespace EPR.Payment.Facade.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
