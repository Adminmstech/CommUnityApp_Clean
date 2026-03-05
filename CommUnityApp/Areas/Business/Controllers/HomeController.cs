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

        public IActionResult AddProduct()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult Promotions()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }
    }
}
