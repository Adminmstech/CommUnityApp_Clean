
using Microsoft.AspNetCore.Mvc;
using CommUnityApp.ApplicationCore.Interfaces;

namespace CommUnityApp.Areas.Community.Controllers
{
    [Area("Community")]
    public class CommunityWebAppController : Controller
    {
        private readonly ICommunityRepository _communityRepository;

        public CommunityWebAppController(
            ICommunityRepository communityRepository)
        {
            _communityRepository = communityRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("CommunityAdminAuthenticated") == "true")
                return RedirectToAction(nameof(Dashboard));

            return View();
        }
        [HttpGet]
        public IActionResult Dashboard()
        {
            var authenticated = HttpContext.Session.GetString(
                "CommunityAdminAuthenticated");

            var communityIdString = HttpContext.Session.GetString("CommunityId");

            if (authenticated != "true" ||
                !int.TryParse(communityIdString, out int communityId) ||
                communityId <= 0)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.CommunityId = communityId;

            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("CommunityId");
            HttpContext.Session.Remove("CommunityAdminAuthenticated");

            return RedirectToAction(nameof(Login));
        }
    }
}
