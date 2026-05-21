using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddBusiness()
        {
            return View();
        }

        public IActionResult Business()
        {
            return View();
        }

        public IActionResult AddCommunity()
        {
            return View();
        }

        public IActionResult ManageCommunity()
        {
            return View();
        }
    }
}