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

        public IActionResult Create(int? brandGameId = null)
        {
            var businessId = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Login", "Account");
            }

            // If an id is provided, pass it through so the client script can hydrate the form for editing
            return View(new AddUpdateBrandGameRequest
            {
                BrandGameID = brandGameId ?? 0
            });
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

            // Log the received BrandGameID for debugging
            System.Diagnostics.Debug.WriteLine($"Create POST received BrandGameID: {model.BrandGameID}");

            if (string.IsNullOrWhiteSpace(model.BrandGameName) || string.IsNullOrWhiteSpace(model.BrandGameTitle))
            {
                ViewBag.Error = "Game Name and Subtitle are required.";
                return View(model);
            }

            // For new games, require uploads for all prize image slots. When editing, existing assets can be re-used.
            var isNewGame = model.BrandGameID == 0;
            if (isNewGame && (model.PrimaryPrizeImageFile == null || model.SecondaryPrizeImageFile == null || model.ConsolationPrizeImageFile == null))
            {
                ViewBag.Error = "Please upload images for all prize types (Primary, Secondary, and Consolation) before launching the game.";
                return View(model);
            }

            string brandGameImagePath = "";
            string unsuccessfulImagePath = "";
            string primaryPrizeImagePath = "";
            string secondaryPrizeImagePath = "";
            string consolationPrizeImagePath = "";

            try
            {
                int brandGameId = model.BrandGameID;

                // For existing games, preserve current image paths
                if (brandGameId > 0)
                {
                    var existing = await _unitOfWork.BrandGames.GetBrandGameByIdAsync(brandGameId);
                    if (existing != null)
                    {
                        brandGameImagePath = existing.BrandGameImage;
                        unsuccessfulImagePath = existing.UnSuccessfulImage;
                    }
                }
                else
                {
                    // For new games, create record first to get the ID
                    model.Status = 0; // Draft initially
                    model.IsReleased = 0;
                    var createResponse = await _unitOfWork.BrandGames.AddUpdateBrandGameAsync(
                        model, "", "", "", "", "");
                    
                    if (createResponse.ResultId <= 0)
                    {
                        ViewBag.Error = createResponse.ResultMessage ?? "Failed to create game record.";
                        return View(model);
                    }
                    brandGameId = createResponse.ResultId;
                    model.BrandGameID = brandGameId;
                }

                // Now save images using the brandGameId
                if (model.BrandGameImageFile != null && model.BrandGameImageFile.Length > 0)
                {
                    brandGameImagePath = await SaveFile(model.BrandGameImageFile, brandGameId, "main");
                }

                if (model.UnSuccessfulImageFile != null && model.UnSuccessfulImageFile.Length > 0)
                {
                    unsuccessfulImagePath = await SaveFile(model.UnSuccessfulImageFile, brandGameId, "unsuccessful");
                }

                if (model.PrimaryPrizeImageFile != null && model.PrimaryPrizeImageFile.Length > 0)
                {
                    primaryPrizeImagePath = await SaveFile(model.PrimaryPrizeImageFile, brandGameId, "primary");
                }

                if (model.SecondaryPrizeImageFile != null && model.SecondaryPrizeImageFile.Length > 0)
                {
                    secondaryPrizeImagePath = await SaveFile(model.SecondaryPrizeImageFile, brandGameId, "secondary");
                }

                if (model.ConsolationPrizeImageFile != null && model.ConsolationPrizeImageFile.Length > 0)
                {
                    consolationPrizeImagePath = await SaveFile(model.ConsolationPrizeImageFile, brandGameId, "consolation");
                }

                // Set the game status to active (1) since all images are uploaded
                model.Status = 1;
                model.IsReleased = 1;

                var response = await _unitOfWork.BrandGames.AddUpdateBrandGameAsync(
                    model,
                    brandGameImagePath,
                    unsuccessfulImagePath,
                    primaryPrizeImagePath,
                    secondaryPrizeImagePath,
                    consolationPrizeImagePath
                );

                if (response.ResultId > 0)
                {
                    var actionVerb = isNewGame ? "created" : "updated";
                    TempData["Success"] = $"Game {actionVerb} and activated successfully!";
                    return RedirectToAction("Index");
                }

                ViewBag.Error = response.ResultMessage;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error uploading images: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var businessId = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessId))
            {
                return Unauthorized();
            }

            var game = await _unitOfWork.BrandGames.GetBrandGameByIdAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            if (game.BusinessId.HasValue && game.BusinessId.Value.ToString() != businessId)
            {
                return Forbid();
            }

            return Json(game);
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
                string primaryPrizeImagePath = "";
                string secondaryPrizeImagePath = "";
                string consolationPrizeImagePath = "";

                int brandGameId = model.BrandGameID;

                // For existing games, preserve current image paths
                if (brandGameId > 0)
                {
                    var existing = await _unitOfWork.BrandGames.GetBrandGameByIdAsync(brandGameId);
                    if (existing != null)
                    {
                        brandGameImagePath = existing.BrandGameImage;
                        unsuccessfulImagePath = existing.UnSuccessfulImage;
                    }
                }
                else
                {
                    // For new games, create record first to get the ID
                    model.Status = 0;
                    model.IsReleased = 0;
                    var createResponse = await _unitOfWork.BrandGames.AddUpdateBrandGameAsync(
                        model, "", "", "", "", "");
                    
                    if (createResponse == null || createResponse.ResultId <= 0)
                    {
                        return Json(new { success = false, message = createResponse?.ResultMessage ?? "Failed to create game record." });
                    }
                    brandGameId = createResponse.ResultId;
                    model.BrandGameID = brandGameId;
                }

                // Now save images using the brandGameId
                if (model.BrandGameImageFile != null && model.BrandGameImageFile.Length > 0)
                {
                    brandGameImagePath = await SaveFile(model.BrandGameImageFile, brandGameId, "main");
                }

                if (model.UnSuccessfulImageFile != null && model.UnSuccessfulImageFile.Length > 0)
                {
                    unsuccessfulImagePath = await SaveFile(model.UnSuccessfulImageFile, brandGameId, "unsuccessful");
                }

                if (model.PrimaryPrizeImageFile != null && model.PrimaryPrizeImageFile.Length > 0)
                {
                    primaryPrizeImagePath = await SaveFile(model.PrimaryPrizeImageFile, brandGameId, "primary");
                }

                if (model.SecondaryPrizeImageFile != null && model.SecondaryPrizeImageFile.Length > 0)
                {
                    secondaryPrizeImagePath = await SaveFile(model.SecondaryPrizeImageFile, brandGameId, "secondary");
                }

                if (model.ConsolationPrizeImageFile != null && model.ConsolationPrizeImageFile.Length > 0)
                {
                    consolationPrizeImagePath = await SaveFile(model.ConsolationPrizeImageFile, brandGameId, "consolation");
                }

                // Keep status as draft (0) during AJAX saves
                model.Status = 0;
                model.IsReleased = 0;

                var response = await _unitOfWork.BrandGames.AddUpdateBrandGameAsync(
                    model,
                    brandGameImagePath,
                    unsuccessfulImagePath,
                    primaryPrizeImagePath,
                    secondaryPrizeImagePath,
                    consolationPrizeImagePath
                );

                if (response != null && response.ResultId > 0)
                {
                    return Json(new
                    {
                        success = true,
                        brandGameId = response.ResultId,
                        message = "Draft saved successfully with uploaded images."
                    });
                }

                return Json(new { success = false, message = response?.ResultMessage ?? "Database failed to save draft." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message + (ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "") });
            }
        }

        private async Task<string> SaveFile(IFormFile file, int brandGameId, string imageType)
        {
            // Validate file size (5MB limit)
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("File size cannot exceed 5MB.");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("Only JPG, PNG, and GIF files are allowed.");
            }

            // Save to wwwroot/Images/brandgames/{brandGameId}/
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "brandgames", brandGameId.ToString());
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Use image type as prefix for organization (e.g., main_, primary_, secondary_, consolation_, unsuccessful_)
            string uniqueFileName = $"{imageType}_{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"Images/brandgames/{brandGameId}/{uniqueFileName}";
        }
    }
}