using CommUnityApp.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Business.Controllers
{
    [Area("Business")]
    public class BusinessWebAppController:Controller
    {
        private readonly IBusinessRepository _businessRepository;

        public BusinessWebAppController(IBusinessRepository businessRepository)
        {
            _businessRepository = businessRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("BusinessAdminAuthenticated") == "true")
                return RedirectToAction(nameof(Dashboard));

            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var authenticated = HttpContext.Session.GetString(
                "BusinessAdminAuthenticated");

            var businessIdString = HttpContext.Session.GetString("BusinessId");

            if (authenticated != "true" ||
                !long.TryParse(businessIdString, out long businessId) ||
                businessId <= 0)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("BusinessId");
            HttpContext.Session.Remove("BusinessAdminAuthenticated");

            return RedirectToAction(nameof(Login));
        }
    }
}
