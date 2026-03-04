using System;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Http;

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

            if (string.IsNullOrWhiteSpace(model.BrandGameName) || string.IsNullOrWhiteSpace(model.BrandGameTitle))
            {
                ViewBag.Error = "Game Name and Subtitle are required.";
                return View(model);
            }

            string brandGameImagePath = "";
            string unsuccessfulImagePath = "";

            if (model.BrandGameID > 0)
            {
                var existing = await _unitOfWork.BrandGames.GetBrandGameByIdAsync(model.BrandGameID);
                if (existing != null)
                {
                    brandGameImagePath = existing.BrandGameImage;
                    unsuccessfulImagePath = existing.UnSuccessfulImage;
                }
            }

            if (model.BrandGameImageFile != null && model.BrandGameImageFile.Length > 0)
            {
                brandGameImagePath = await SaveFile(model.BrandGameImageFile, "BrandGames");
            }

            if (model.UnSuccessfulImageFile != null && model.UnSuccessfulImageFile.Length > 0)
            {
                unsuccessfulImagePath = await SaveFile(model.UnSuccessfulImageFile, "BrandGames/Unsuccessful");
            }

            var response = await _unitOfWork.BrandGames.AddUpdateBrandGameAsync(model, brandGameImagePath, unsuccessfulImagePath);

            if (response.ResultId > 0)
            {
                TempData["Success"] = "Game saved successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.Error = response.ResultMessage;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AjaxSave([FromForm] AddUpdateBrandGameRequest model)
        {
            try
            {
                var businessIdStr = HttpContext.Session.GetString("BusinessId");
                if (string.IsNullOrEmpty(businessIdStr))
                {
                    return Json(new { success = false, message = "Session expired. Please login again." });
                }

                if (!int.TryParse(businessIdStr, out int businessId))
                {
                    return Json(new { success = false, message = "Invalid Business Session ID." });
                }

                if (model == null)
                {
                    return Json(new { success = false, message = "Form data is empty or invalid binding." });
                }

                // Strict manual validation for critical fields as per user request to block nulls
                if (string.IsNullOrWhiteSpace(model.BrandGameName))
                {
                    return Json(new { success = false, message = "Game Name is required and cannot be empty." });
                }
                if (string.IsNullOrWhiteSpace(model.BrandGameTitle))
                {
                    return Json(new { success = false, message = "Game Title is required and cannot be empty." });
                }
                
                model.BusinessId = businessId;

                string brandGameImagePath = "";
                string unsuccessfulImagePath = "";

                if (model.BrandGameID > 0)
                {
                    var existing = await _unitOfWork.BrandGames.GetBrandGameByIdAsync(model.BrandGameID);
                    if (existing != null)
                    {
                        brandGameImagePath = existing.BrandGameImage;
                        unsuccessfulImagePath = existing.UnSuccessfulImage;
                    }
                }

                if (model.BrandGameImageFile != null && model.BrandGameImageFile.Length > 0)
                {
                    brandGameImagePath = await SaveFile(model.BrandGameImageFile, "BrandGames");
                }

                if (model.UnSuccessfulImageFile != null && model.UnSuccessfulImageFile.Length > 0)
                {
                    unsuccessfulImagePath = await SaveFile(model.UnSuccessfulImageFile, "BrandGames/Unsuccessful");
                }

                var response = await _unitOfWork.BrandGames.AddUpdateBrandGameAsync(model, brandGameImagePath, unsuccessfulImagePath);

                if (response != null && response.ResultId > 0)
                {
                    return Json(new { 
                        success = true, 
                        brandGameId = response.ResultId, 
                        message = "Draft saved successfully."
                    });
                }

                return Json(new { success = false, message = response?.ResultMessage ?? "Database failed to save draft." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message + (ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "") });
            }
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
