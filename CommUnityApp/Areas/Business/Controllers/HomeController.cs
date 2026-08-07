using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Business.Controllers
{
    [Area("Business")]
    public class HomeController : Controller
    {
        private string? CurrentBusinessId => HttpContext.Session.GetString("BusinessId");

        private IActionResult? RequireBusinessSession()
        {
            var businessId = CurrentBusinessId;

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;
            return null;
        }

        public IActionResult Index()
        {
            var redirect = RequireBusinessSession();
            if (redirect != null)
            {
                return redirect;
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
            var redirect = RequireBusinessSession();
            if (redirect != null)
            {
                return redirect;
            }

            return View();
        }

        public IActionResult Auctions()
        {
            var redirect = RequireBusinessSession();
            if (redirect != null)
            {
                return redirect;
            }

            return View();
        }

        public IActionResult AuctionDetails()
        {
            var redirect = RequireBusinessSession();
            if (redirect != null)
            {
                return redirect;
            }

            return View();
        }


        public IActionResult Appointments()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult AddService()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;
            ViewBag.ServiceId = Request.Query["serviceId"].ToString();

            return View();
        }

        public IActionResult BusinessPosts()
        {
            var redirect = RequireBusinessSession();
            if (redirect != null)
            {
                return redirect;
            }

            return View();
        }

        public IActionResult GetBusinessService()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult AddCampaign()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult Campaigns()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult AddPromotion()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult BusinessPromotions()
        {
            var redirect = RequireBusinessSession();
            if (redirect != null)
            {
                return redirect;
            }

            return View();
        }

        public IActionResult PromotionDetails()
        {
            var redirect = RequireBusinessSession();
            if (redirect != null)
            {
                return redirect;
            }

            return View();
        }

        public IActionResult Services()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BusinessId = businessId;

            return View();
        }

        public IActionResult CoinManagement()
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
