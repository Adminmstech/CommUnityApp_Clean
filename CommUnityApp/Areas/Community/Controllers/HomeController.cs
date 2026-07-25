using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace CommUnityApp.Areas.Community.Controllers
{
    [Area("Community")]
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

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
        public IActionResult AddEventTicketType()
        {
            return View();
        }
        public IActionResult EventTicketBookings(int eventId)
        {
            ViewBag.EventId = eventId;
            return View();
        }
        public IActionResult ViewEventTicketType()
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

         public IActionResult ViewBrandGameResults()
        {
            return View();}

        public IActionResult ViewSpinGameGameResults()
        {
            return View();
        }

        public IActionResult AddCommunityPosts()
        {
            return View();
        }

        public IActionResult CommunityPosts()
        {
            return View();
        }

        public IActionResult CareConnectList()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetCareConnectList()
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                $"{_configuration["ApiBaseUrl"]}/api/CareConnect/GetCareConnectDashboard");

            var json = await response.Content.ReadAsStringAsync();

            return Json(json);
        }


    }
}
