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

        [HttpPost]
        public async Task<IActionResult> ImportDefaultGame(string templateType = "welcome")
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

                // Get business name for personalization
                var businessName = HttpContext.Session.GetString("BusinessName") ?? "My Business";

                // Create template based on type
                var model = GetTemplateModel(templateType, businessId, businessName);

                // Create the game (status = 0 draft so they can customize before launching)
                model.Status = 0;
                model.IsReleased = 0;

                var response = await _unitOfWork.BrandGames.AddUpdateBrandGameAsync(
                    model, "", "", "", "", "");

                if (response != null && response.ResultId > 0)
                {
                    // Redirect to edit mode so they can customize and upload images
                    return Json(new
                    {
                        success = true,
                        brandGameId = response.ResultId,
                        message = "Template imported successfully! Customize and add images to launch.",
                        redirectUrl = Url.Action("Create", "Game", new { brandGameId = response.ResultId })
                    });
                }

                return Json(new { success = false, message = response?.ResultMessage ?? "Failed to import template." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        private AddUpdateBrandGameRequest GetTemplateModel(string templateType, int businessId, string businessName)
        {
            return templateType.ToLower() switch
            {
                "holiday" => new AddUpdateBrandGameRequest
                {
                    BusinessId = businessId,
                    BrandGameName = $"Holiday Special - {businessName}",
                    BrandGameTitle = "Celebrate & Win Big!",
                    BrandGameDesc = "Join our holiday celebration and win exclusive prizes!",
                    ConditionsApply = "Limited time offer. Terms and conditions apply. Must be 18+.",
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddDays(14),
                    PanelCount = 5,
                    PanelOpeningLimit = 2,
                    ChanceCount = 5,
                    PrimaryOfferText = "50% OFF Grand Prize!",
                    OfferText = "20% OFF Special",
                    PrimaryWinMessage = "AMAZING! You won 50% OFF!",
                    SecondaryWinMessage = "Great! You won 20% OFF!",
                    ConsolationMessage = "Happy Holidays! Thanks for playing!",
                    PointsAwarded = 25,
                    FormColor = "#C41E3A",
                    TextColor = "#FFFFFF",
                    PrimaryPrizeCount = 3,
                    SecondaryPrizeCount = 15,
                    ConsolationPrizeCount = 100
                },
                "loyalty" => new AddUpdateBrandGameRequest
                {
                    BusinessId = businessId,
                    BrandGameName = $"Loyalty Rewards - {businessName}",
                    BrandGameTitle = "Thank You for Being Our Customer!",
                    BrandGameDesc = "Exclusive game for our loyal customers. Play daily!",
                    ConditionsApply = "Available to registered members only.",
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddYears(1),
                    PanelCount = 4,
                    PanelOpeningLimit = 1,
                    ChanceCount = 1,
                    PrimaryOfferText = "Free Product!",
                    OfferText = "Bonus Points",
                    PrimaryWinMessage = "WOW! You won a FREE PRODUCT!",
                    SecondaryWinMessage = "You earned BONUS POINTS!",
                    ConsolationMessage = "Come back tomorrow for another chance!",
                    PointsAwarded = 50,
                    FormColor = "#6B5B95",
                    TextColor = "#FFFFFF",
                    PrimaryPrizeCount = 12,
                    SecondaryPrizeCount = 365,
                    ConsolationPrizeCount = 1000
                },
                "newcustomer" => new AddUpdateBrandGameRequest
                {
                    BusinessId = businessId,
                    BrandGameName = $"New Customer Welcome - {businessName}",
                    BrandGameTitle = "Welcome! Here's Your First Reward!",
                    BrandGameDesc = "As a new customer, enjoy a special welcome game with guaranteed rewards!",
                    ConditionsApply = "For new registered customers only. One-time play.",
                    DateStart = DateTime.Now,
                    DateEnd = null,
                    PanelCount = 3,
                    PanelOpeningLimit = 3,
                    ChanceCount = 1,
                    PrimaryOfferText = "Welcome Gift!",
                    OfferText = "15% Off First Purchase",
                    PrimaryWinMessage = "Your Welcome Gift is Ready!",
                    SecondaryWinMessage = "Enjoy 15% off your first purchase!",
                    ConsolationMessage = "Welcome aboard! Enjoy 10% off!",
                    PointsAwarded = 100,
                    FormColor = "#008080",
                    TextColor = "#FFFFFF",
                    PrimaryPrizeCount = 10,
                    SecondaryPrizeCount = 100,
                    ConsolationPrizeCount = 9999
                },
                _ => new AddUpdateBrandGameRequest // "welcome" default
                {
                    BusinessId = businessId,
                    BrandGameName = $"Welcome Game - {businessName}",
                    BrandGameTitle = "Play & Win Exciting Prizes!",
                    BrandGameDesc = "Welcome to our scratch and win game! Try your luck and win amazing prizes.",
                    ConditionsApply = "Terms and conditions apply. One play per customer per day. Must be 18+.",
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddMonths(3),
                    PanelCount = 3,
                    PanelOpeningLimit = 1,
                    ChanceCount = 3,
                    PrimaryOfferText = "Grand Prize - 10% Discount",
                    OfferText = "5% Discount",
                    PrimaryWinMessage = "Congratulations! You won our Grand Prize!",
                    SecondaryWinMessage = "Great job! You won a 5% discount!",
                    ConsolationMessage = "Better luck next time! Thanks for playing.",
                    PointsAwarded = 10,
                    FormColor = "#4A90D9",
                    TextColor = "#FFFFFF",
                    PrimaryPrizeCount = 5,
                    SecondaryPrizeCount = 20,
                    ConsolationPrizeCount = 100
                }
            };
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