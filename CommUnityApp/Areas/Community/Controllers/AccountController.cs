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
            var request = new CommunityLoginRequest
            {
                UserName = userName,
                Password = password
            };

            var response = await _communityRepository.LoginAsync(request);

            if (response != null && response.CommunityId > 0)
            {
                HttpContext.Session.SetString("CommunityId", response.CommunityId.ToString());
                HttpContext.Session.SetString("CommunityName", response.CommunityName);

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                };

                Response.Cookies.Append(
                    "CommunityId",
                    response.CommunityId.ToString(),
                    cookieOptions);

                Response.Cookies.Append(
                    "CommunityName",
                    response.CommunityName,
                    cookieOptions);

                return RedirectToAction(
                    "ViewEvents",
                    "Home",
                    new { area = "Community" });
            }

            ViewBag.Error = response?.ResultMessage ?? "Invalid username or password";

            return View("CommunityLogin");
        }

        public IActionResult CommunityLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CommunityLogin(string userName, string password)
        {
            var request = new CommunityLoginRequest
            {
                UserName = userName,
                Password = password
            };

            var response = await _communityRepository.LoginAsync(request);

            if (response != null && response.CommunityId > 0)
            {
                HttpContext.Session.SetString("CommunityId", response.CommunityId.ToString());
                HttpContext.Session.SetString("CommunityName", response.CommunityName);

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                };

                Response.Cookies.Append(
                    "CommunityId",
                    response.CommunityId.ToString(),
                    cookieOptions);

                Response.Cookies.Append(
                    "CommunityName",
                    response.CommunityName,
                    cookieOptions);

                return RedirectToAction(
                    "AddEvent",
                    "Home",
                    new { area = "Community" });
            }

            ViewBag.Error = response?.ResultMessage ?? "Invalid username or password";

            return View();
        }
    }
}
