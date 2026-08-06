using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommUnityApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(AddBusiness));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            try
            {
                var result = await _unitOfWork.User.UserLogin(request);
                var roles = result?.Role?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    ?? Array.Empty<string>();

                if (result == null || result.ResultId == 0 || !roles.Contains("1"))
                {
                    ViewBag.Error = "Invalid admin email or password.";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                    new(ClaimTypes.Name, result.FullName ?? "Super Admin"),
                    new(ClaimTypes.Email, result.Email ?? string.Empty)
                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    });

                HttpContext.Session.SetString("AdminId", result.UserId.ToString());
                HttpContext.Session.SetString("AdminName", result.FullName ?? "Super Admin");

                return RedirectToAction(nameof(AddBusiness));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Database connection is not configured. Set ConnectionStrings:DefaultConnection in appsettings.Development.json.";
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home", new { area = "" });
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

        public IActionResult AuctionDetails()
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

        public IActionResult AddSmartQuiz()
        {
            return View();
        }

        public IActionResult SmartQuizList()
        {
            return View();
        }

        public IActionResult QuizGameplays()
        {
            return View();
        }

        public IActionResult SpinGameList()
        {
            return View();
        }

        public IActionResult AddSpinGame()
        {
            return View();
        }

        public IActionResult BrandGameList()
        {
            return View();
        }

        public IActionResult AddBrandGame()
        {
            return View(new AddUpdateBrandGameRequest());
        }
    }
}
