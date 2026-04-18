using CommUnityApp.ApplicationCore.Models;
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

        public IActionResult ViewRegistrations()
        {
            return View();
        }

        public IActionResult CharityList()
        {
            long communityId = 0;

            var sessionValue = HttpContext.Session.GetString("CommunityId");

            if (!string.IsNullOrEmpty(sessionValue))
            {
                communityId = Convert.ToInt64(sessionValue);
            }

            ViewBag.CommunityId = communityId;

            return View();
        }
        public IActionResult ItemRequestList()
        {
            long communityId = 0;

            var sessionValue = HttpContext.Session.GetString("CommunityId");

            if (!string.IsNullOrEmpty(sessionValue))
            {
                communityId = Convert.ToInt64(sessionValue);
            }

            ViewBag.CommunityId = communityId;

            return View();
        }

        public IActionResult DeliveryReport()
        {
            return View();
        }

        public IActionResult AddEventSponsors()
        {
           
            return View();
        }

        public IActionResult EventSponsors()
        {
            return View();
        }
        public IActionResult AddSponsorsToEvent()
        {
            return View();
        }
        public IActionResult ViewMembers()
        {
            return View();
        }
        public IActionResult CommunityChat()
        {
            return View();
        }

        public IActionResult ViewMessageBoardPosts()
        {

           return View();
        }

         public IActionResult CreateMessageBoardPost()
        {
            return View();}

    }
}
