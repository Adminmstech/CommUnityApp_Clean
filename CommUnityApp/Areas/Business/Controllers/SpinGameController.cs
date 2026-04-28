using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.Areas.Business.Controllers
{
    [Area("Business")]
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
            var businessIdStr = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessIdStr)) return RedirectToAction("Login", "Account");

            int businessId = int.Parse(businessIdStr);
            var games = await _spinGameRepository.GetSpinGamesByBusinessAsync(businessId);
            return View(games);
        }

        public async Task<IActionResult> Create(int? spinGameId = null)
        {
            var businessIdStr = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessIdStr)) return RedirectToAction("Login", "Account");

            var model = new AddUpdateSpinGameRequest
            {
                GameId = spinGameId ?? 0,
                BusinessId = int.Parse(businessIdStr)
            };

            if (model.GameId > 0)
            {
                // Load existing data for edit
                var game = await _spinGameRepository.GetSpinGameByIdAsync(model.GameId);
                if (game != null)
                {
                    model.GameName = game.GameName ?? "";
                    model.Description = game.Description;
                    model.GameImage = game.GameImage;
                    model.ConfigId = game.ConfigId;
                    model.IsActive = game.IsActive;

                    var config = await _spinGameRepository.GetConfigByIdAsync(model.ConfigId);
                    if (config != null)
                    {
                        model.Configs.Add(config);
                    }

                    model.Sections = (await _spinGameRepository.GetSectionsByGameIdAsync(model.GameId)).ToList();
                }
            }
            else
            {
                // Default config for new game
                model.Configs.Add(new SpinGameConfigRequest { NumberOfSections = 8 });
                for (int i = 1; i <= 8; i++)
                {
                    model.Sections.Add(new SpinSectionRequest { SectionNumber = i });
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddUpdateSpinGameRequest model)
        {
            var businessIdStr = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessIdStr)) return RedirectToAction("Login", "Account");

            model.BusinessId = int.Parse(businessIdStr);

            if (string.IsNullOrWhiteSpace(model.GameName))
            {
                ViewBag.Error = "Game Name is required.";
                return View(model);
            }

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
            var businessIdStr = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessIdStr)) return Unauthorized();

            var game = await _spinGameRepository.GetSpinGameByIdAsync(id);
            if (game == null) return NotFound();

            if (game.BusinessId != int.Parse(businessIdStr)) return Forbid();

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

