using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Business.Controllers
{
    [Area("Business")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("BusinessId")))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
    }
}
