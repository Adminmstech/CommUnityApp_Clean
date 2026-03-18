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

        public IActionResult BusinessProfile()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult BusinessOrders()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult OrderDetails()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult BusinessCustomers()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult Communication()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult AddAuction()
        {
            return View();
        }

        public IActionResult Auctions()
        {
            return View();
        }

        public IActionResult AuctionDetails()
        {
            return View();
        }
    }
}
