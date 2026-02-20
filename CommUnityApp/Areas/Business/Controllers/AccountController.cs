using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CommUnityApp.Areas.Business.Controllers
{
    [Area("Business")]
    public class AccountController : Controller
    {
        private readonly IBusinessRepository _businessRepository;

        public AccountController(IBusinessRepository businessRepository)
        {
            _businessRepository = businessRepository;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var request = new BusinessLoginRequest { Email = email, Password = password };
            var response = await _businessRepository.LoginAsync(request);

            if (response != null && response.BusinessId > 0)
            {
                HttpContext.Session.SetString("BusinessId", response.BusinessId.ToString());
                HttpContext.Session.SetString("BusinessName", response.BusinessName);

                return RedirectToAction("Index", "Home", new { area = "Business" });
            }

            ViewBag.Error = response?.ResultMessage ?? "Invalid email or password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(AddUpdateBusinessRequest model)
        {
            var response = await _businessRepository.RegisterAsync(model);
            if (response.ResultId > 0)
            {
                 ViewBag.Message = response.ResultMessage;
            }
            else
            {
                ViewBag.Error = response.ResultMessage;
            }
            
            return View();
        }
    }
}
