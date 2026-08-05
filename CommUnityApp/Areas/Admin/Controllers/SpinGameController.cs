using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SpinGameController : Controller
    {
        private readonly ISpinGameRepository _spinGameRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SpinGameController(ISpinGameRepository spinGameRepository, IWebHostEnvironment webHostEnvironment)
        {
            _spinGameRepository = spinGameRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var games = await _spinGameRepository.GetAllSpinGamesAsync(); 
            return View(games);
        }

        public async Task<IActionResult> Create(int? spinGameId = null)
        {
            var model = new AddUpdateSpinGameRequest
            {
                GameId = spinGameId ?? 0,
                BusinessId = 0,
                Configs = new List<SpinGameConfigRequest>(),
                Sections = new List<SpinSectionRequest>()
            };

            if (model.GameId > 0)
            {
                var game = await _spinGameRepository.GetSpinGameByIdAsync(model.GameId);
                if (game != null)
                {
                    model.GameName = game.GameName ?? "";
                    model.Description = game.Description;
                    model.GameImage = game.GameImage;
                    model.ConfigId = game.ConfigId;
                    model.IsActive = game.IsActive;
                    model.BusinessId = game.BusinessId; // was previously stuck at 0 on edit
                    model.BusinessLocation = game.BusinessLocation; // was previously stuck at 0 on edit

                    var config = await _spinGameRepository.GetConfigByIdAsync(model.ConfigId);
                    if (config != null)
                    {
                        model.Configs.Add(config);
                    }
                    else
                    {
                        // Fallback so the Configuration tab always has something to bind to,
                        // even if the config record is missing or ConfigId is 0.
                        model.Configs.Add(new SpinGameConfigRequest
                        {
                            ConfigId = model.ConfigId,
                            MaxSpinsPerDay = 5,
                            NumberOfSections = 8,
                            IsActive = true
                        });
                    }

                    var sections = (await _spinGameRepository.GetSectionsByGameIdAsync(model.GameId)).ToList();
                    if (sections.Any())
                    {
                        model.Sections = sections;
                    }
                    else
                    {
                        // Fallback so the Sections tab isn't left empty for an existing game
                        // that has no saved sections yet.
                        for (int i = 1; i <= 8; i++)
                        {
                            model.Sections.Add(new SpinSectionRequest { SectionNumber = i });
                        }
                    }
                }
            }
            else
            {
                model.Configs.Add(new SpinGameConfigRequest { NumberOfSections = 8 });
                for (int i = 1; i <= 8; i++)
                {
                    model.Sections.Add(new SpinSectionRequest
                    {
                        SectionNumber = i
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddUpdateSpinGameRequest model)
        {
            //var businessIdStr = HttpContext.Session.GetString("BusinessId");
            //if (string.IsNullOrEmpty(businessIdStr)) return RedirectToAction("Login", "Account");
            model.BusinessId = int.Parse("0");

            if (string.IsNullOrWhiteSpace(model.GameName))
            {
                ViewBag.Error = "Game Name is required.";
                return View(model);
            }

            // ---- Server-side section validation (mirrors client-side rules) ----
            if (model.Sections == null || model.Sections.Count < 4)
            {
                ViewBag.Error = "At least 4 wheel sections are required.";
                return View(model);
            }

            for (int i = 0; i < model.Sections.Count; i++)
            {
                var section = model.Sections[i];

                if (string.IsNullOrWhiteSpace(section.PrizeText))
                {
                    ViewBag.Error = $"Prize text is required for section {i + 1}.";
                    return View(model);
                }

                bool hasExistingImage = !string.IsNullOrWhiteSpace(section.SectionImage);
                bool hasNewImage = section.SectionImageFile != null && section.SectionImageFile.Length > 0;

                if (!hasExistingImage && !hasNewImage)
                {
                    ViewBag.Error = $"An image is required for section {i + 1}.";
                    return View(model);
                }
            }

            // ---- Game cover image ----
            if (model.GameImageFile != null && model.GameImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "spingames");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.GameImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.GameImageFile.CopyToAsync(fileStream);
                }
                model.GameImage = "/images/spingames/" + uniqueFileName;
            }

            // ---- Section images ----
            var sectionsUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "spingames", "sections");
            if (!Directory.Exists(sectionsUploadsFolder)) Directory.CreateDirectory(sectionsUploadsFolder);

            foreach (var section in model.Sections)
            {
                if (section.SectionImageFile != null && section.SectionImageFile.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(section.SectionImageFile.FileName);
                    var filePath = Path.Combine(sectionsUploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await section.SectionImageFile.CopyToAsync(fileStream);
                    }
                    section.SectionImage = "/images/spingames/sections/" + uniqueFileName;
                }
                // else: keep the existing section.SectionImage value posted from the hidden field
            }

            bool isNewGame = model.GameId == 0;

            // Save game, config, and sections in one transaction
            var gameResponse = await _spinGameRepository.AddUpdateSpinGameAsync(model);
            if (gameResponse.ResultId <= 0)
            {
                ViewBag.Error = gameResponse.ResultMessage;
                return View(model);
            }

            model.GameId = gameResponse.ResultId;
            TempData["Success"] = isNewGame ? "Spin game created successfully!" : "Spin game updated successfully!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteSection(int sectionId, int gameId)
        {
            var response = await _spinGameRepository.DeleteSectionAsync(sectionId);
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var game = await _spinGameRepository.GetSpinGameByIdAsync(id);

            if (game == null)
                return NotFound();

            return Json(game);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _spinGameRepository.DeleteSpinGameAsync(id);
            return Json(response);
        }
    }
}
