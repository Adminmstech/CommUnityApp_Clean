using CommUnityApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Community.Controllers
{
    [Area("Community")]
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            

            return View();
        }

        public IActionResult AddEvent()
        {
            return View();
        }
        public IActionResult ViewEvents()
        {
            return View();
        }
        public IActionResult PostEvent()
        {
            return View();
        }
    }
}
