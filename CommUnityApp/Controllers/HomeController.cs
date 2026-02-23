using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace CommUnityApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Auctions()
        {
            return View();
        }

        public IActionResult AddAuction()
        {
            return View();
        }

        public IActionResult AuctionDetails()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Email and Password are required."
                });
            }

            var result = await _unitOfWork.User.UserLogin(request);

            if (result == null || result.ResultId == 0)
            {
                return Unauthorized(new
                {
                    ResultId = 0,
                    ResultMessage = "Invalid email or password."
                });
            }

            var roles = result.Role.Split(',');

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.FullName),
                new Claim(ClaimTypes.Email, result.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // Role-based redirection
            if (roles.Contains("1")) // Admin
            {
                return RedirectToAction("AddBusiness", "Home", new { area = "Admin" });
            }
            else if (roles.Contains("2")) // Business
            {
                return RedirectToAction("Auctions", "Home", new { area = "Business" });
            }
            else if (roles.Contains("3")) // Member
            {
                return RedirectToAction("Index", "Member", new { area = "Member" });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
