using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Community.Controllers
{
    [Area("Community")]
    public class AccountController : Controller
    {
        private readonly ICommunityRepository _communityRepository;

        public AccountController(ICommunityRepository communityRepository)
        {
            _communityRepository = communityRepository;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var request = new CommunityLoginRequest { UserName = userName, Password = password };
            var response = await _communityRepository.LoginAsync(request);

            if (response != null && response.CommunityId > 0)
            {
                HttpContext.Session.SetString("CommunityId", response.CommunityId.ToString());
                HttpContext.Session.SetString("CommunityName", response.CommunityName);

                return RedirectToAction("ViewEvents", "Home", new { area = "Community" });
            }

            ViewBag.Error = response?.ResultMessage ?? "Invalid username or password";
            return View();
        }

        public IActionResult CommunityLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CommunityLogin(string userName, string password)
        {
            var request = new CommunityLoginRequest { UserName = userName, Password = password };
            var response = await _communityRepository.LoginAsync(request);

            if (response != null && response.CommunityId > 0)
            {
                HttpContext.Session.SetString("CommunityId", response.CommunityId.ToString());
                HttpContext.Session.SetString("CommunityName", response.CommunityName);

                return RedirectToAction("AddEvent", "Home", new { area = "Community" });
            }

            ViewBag.Error = response?.ResultMessage ?? "Invalid username or password";
            return View();
        }
    }
}
