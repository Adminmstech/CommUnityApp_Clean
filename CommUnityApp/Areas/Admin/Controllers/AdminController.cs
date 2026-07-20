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


        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Communication()
        {
            return View();
        }

        public IActionResult Promotions()
        {
            return View();
        }

        public IActionResult LiveAuctions()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }
        public IActionResult AddService()
        {
            return View();
        }
        public IActionResult CharityItems()
        {
            return View();
        }

        public IActionResult AddCharityItem()
        {
            return View();
        }

        public IActionResult AllocateCoins()
        {
            return View();
        }
        public IActionResult AddTextQuiz()
        {
            return View();
        }
        public IActionResult TextQuizList()
        {
            return View();
        }
    }
}