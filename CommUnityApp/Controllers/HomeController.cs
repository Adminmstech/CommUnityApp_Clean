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
        private readonly IBusinessRepository _businessRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IUnitOfWork unitOfWork,
            IBusinessRepository businessRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _businessRepository = businessRepository;
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

        public IActionResult CreateSpinGame()
        {
            return View("~/Views/Game/Create.cshtml");
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
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

            LoginResponse result;

            try
            {
                result = await _unitOfWork.User.UserLogin(request);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Database connection is not configured. Set ConnectionStrings:DefaultConnection in appsettings.Development.json."
                });
            }

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
                HttpContext.Session.SetString("AdminId", result.UserId.ToString());
                HttpContext.Session.SetString("AdminName", result.FullName ?? "Super Admin");
                return RedirectToAction("AddBusiness", "Admin", new { area = "Admin" });
            }
            else if (roles.Contains("2")) // Business
            {
                BusinessLoginResponse? business;

                try
                {
                    business = await _businessRepository.LoginAsync(new BusinessLoginRequest
                    {
                        Email = request.Email,
                        Password = request.Password
                    });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "Database connection is not configured. Set ConnectionStrings:DefaultConnection in appsettings.Development.json."
                    });
                }

                if (business == null || business.BusinessId <= 0)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return RedirectToAction("Login", "Account", new { area = "Business" });
                }

                HttpContext.Session.SetString("BusinessId", business.BusinessId.ToString());
                HttpContext.Session.SetString("BusinessName", business.BusinessName ?? result.FullName ?? "Business");

                return RedirectToAction("BusinessPromotions", "Home", new { area = "Business" });
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

        public IActionResult PromotionShare(int promotionId, Guid s)
        {
            ViewBag.PromotionId = promotionId;
            ViewBag.ShareToken = s;

            return View();
        }
    }
}
