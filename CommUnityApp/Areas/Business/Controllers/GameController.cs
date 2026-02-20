using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace CommUnityApp.Areas.Business.Controllers
{
    [Area("Business")]
    public class GameController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GameController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            var games = await _unitOfWork.BrandGames.GetBrandGamesByMerchantAsync(int.Parse(businessId));
            return View(games);
        }

        public IActionResult Create()
        {
            var businessId = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }
            return View(new AddUpdateBrandGameRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddUpdateBrandGameRequest model)
        {
            var businessId = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            model.BusinessId = int.Parse(businessId);

            string brandGameImagePath = "";
            string unsuccessfulImagePath = "";

            if (model.BrandGameImageFile != null)
            {
                brandGameImagePath = await SaveFile(model.BrandGameImageFile, "BrandGames");
            }

            if (model.UnSuccessfulImageFile != null)
            {
                unsuccessfulImagePath = await SaveFile(model.UnSuccessfulImageFile, "BrandGames/Unsuccessful");
            }

            var response = await _unitOfWork.BrandGames.AddUpdateBrandGameAsync(model, brandGameImagePath, unsuccessfulImagePath);

            if (response.ResultId > 0)
            {
                TempData["Success"] = "Game created successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.Error = response.ResultMessage;
            return View(model);
        }

        private async Task<string> SaveFile(IFormFile file, string subFolder)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Path.Combine("uploads", subFolder, uniqueFileName).Replace("\\", "/");
        }
    }
}
