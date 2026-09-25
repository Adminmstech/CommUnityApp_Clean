using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class ScratchWinController : Controller
    {
        private readonly IScratchWinRepository _scratchWinRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ScratchWinController(IScratchWinRepository scratchWinRepository, IWebHostEnvironment webHostEnvironment)
        {
            _scratchWinRepository = scratchWinRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var games = await _scratchWinRepository.GetAllScratchWinGamesAsync();
            return View(games);
        }

        [HttpGet("Create")]
        [HttpGet("Create/{id?}")]
        public async Task<IActionResult> Create(int? id = null, int? gameId = null)
        {
            int targetGameId = (id.HasValue && id.Value > 0) ? id.Value : (gameId ?? 0);

            var model = new AddUpdateScratchWinGameRequest
            {
                GameId = targetGameId,
                GameName = "Scratch & Win Exciting Rewards",
                GameTitle = "Scratch & Win Daily Rewards!",
                GameDescription = "Scratch the card to reveal your prize! Win Free Coffee, Burgers, Mystery Gifts, or IndoCoins!",
                TermsAndConditions = "Terms and conditions apply. One play per member per day. Must be 18+ to claim prizes.",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(6),
                ChanceCount = 1,
                OnceIn = 1,
                Status = 1,
                BusinessLocation = "Main Store",
                Rewards = new List<AddUpdateScratchWinRewardRequest>()
            };

            if (targetGameId > 0)
            {
                var existing = await _scratchWinRepository.GetScratchWinGameByIdAsync(targetGameId);
                if (existing != null)
                {
                    model.GameId = existing.GameId;
                    model.GameName = existing.GameName;
                    model.GameTitle = existing.GameTitle;
                    model.GameDescription = existing.GameDescription;
                    model.TermsAndConditions = existing.TermsAndConditions;
                    model.StartDate = existing.StartDate;
                    model.EndDate = existing.EndDate;
                    model.ChanceCount = existing.ChanceCount;
                    model.OnceIn = existing.OnceIn;
                    model.Status = existing.Status;
                    model.BusinessId = existing.BusinessId;
                    model.BusinessLocation = existing.BusinessLocation;
                    model.Rewards = existing.Rewards.Select(r => new AddUpdateScratchWinRewardRequest
                    {
                        RewardId = r.RewardId,
                        RewardOrder = r.RewardOrder,
                        RewardType = r.RewardType,
                        RewardName = r.RewardName,
                        CoinValue = r.CoinValue,
                        ProbabilityPercentage = r.ProbabilityPercentage,
                        TotalStock = r.TotalStock,
                        AvailableStock = r.AvailableStock,
                        RewardImage = r.RewardImage,
                        Description = r.Description,
                        WinMessage = r.WinMessage,
                        IsActive = r.IsActive
                    }).ToList();
                }
            }

            // If new game or no rewards, populate the 8 default rewards with default images bound from Untitled design (1) 1
            if (!model.Rewards.Any())
            {
                model.Rewards = GetDefaultRewards();
            }

            return View("Create", model);
        }

        [HttpGet("Edit")]
        [HttpGet("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id = null, int? gameId = null)
        {
            return await Create(id, gameId);
        }

        [HttpPost("Create")]
        [HttpPost("Create/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUpdateScratchWinGameRequest model, int? id = null, int? gameId = null)
        {
            if ((model.GameId <= 0) && (id.HasValue && id.Value > 0))
            {
                model.GameId = id.Value;
            }
            else if ((model.GameId <= 0) && (gameId.HasValue && gameId.Value > 0))
            {
                model.GameId = gameId.Value;
            }

            if (model.Rewards == null || !model.Rewards.Any())
            {
                model.Rewards = GetDefaultRewards();
            }

            // Percentage validation: sum must be 100%
            var totalPercentage = model.Rewards.Sum(r => r.ProbabilityPercentage);
            if (Math.Abs(totalPercentage - 100.00m) > 0.01m)
            {
                ModelState.AddModelError("", $"Total reward probability must sum to exactly 100%. Current total: {totalPercentage}%.");
                return View("Create", model);
            }

            // Resolve web root directory safely
            string webRoot = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
            string uploadsFolder = Path.Combine(webRoot, "Images", "scratch_win", "custom");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            // Handle reward image file uploads
            for (int i = 0; i < model.Rewards.Count; i++)
            {
                var reward = model.Rewards[i];

                // Check model-bound file first, then fall back to Request.Form.Files
                IFormFile? file = reward.RewardImageFile;

                if (file == null || file.Length == 0)
                {
                    file = Request.Form.Files.GetFile($"Rewards[{i}].RewardImageFile")
                        ?? Request.Form.Files.GetFile($"Rewards_{i}__RewardImageFile")
                        ?? Request.Form.Files.GetFile($"RewardImageFile_{i}")
                        ?? Request.Form.Files.GetFile($"RewardImageFile_{reward.RewardOrder}")
                        ?? Request.Form.Files.FirstOrDefault(f =>
                            f.Name.Equals($"Rewards[{i}].RewardImageFile", StringComparison.OrdinalIgnoreCase) ||
                            f.Name.EndsWith($"[{i}].RewardImageFile", StringComparison.OrdinalIgnoreCase) ||
                            f.Name.Equals($"RewardImageFile_{i}", StringComparison.OrdinalIgnoreCase) ||
                            f.Name.Equals($"RewardImageFile_{reward.RewardOrder}", StringComparison.OrdinalIgnoreCase));
                }

                if (file != null && file.Length > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                    {
                        fileExtension = ".png";
                    }

                    string uniqueFileName = $"reward_{reward.RewardOrder}_{Guid.NewGuid():N}{fileExtension}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    reward.RewardImage = $"Images/scratch_win/custom/{uniqueFileName}";
                }
                else if (string.IsNullOrWhiteSpace(reward.RewardImage))
                {
                    // Fallback to default bound image
                    var defaults = GetDefaultRewards();
                    var match = defaults.FirstOrDefault(d => d.RewardOrder == reward.RewardOrder);
                    if (match != null)
                    {
                        reward.RewardImage = match.RewardImage;
                    }
                }
            }

            var result = await _scratchWinRepository.AddUpdateScratchWinGameAsync(model);

            if (result.ResultId == 1)
            {
                TempData["SuccessMessage"] = "Scratch & Win game saved successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.ResultMessage ?? "Failed to save Scratch & Win game.");
            return View("Create", model);
        }

        [HttpPost("Edit")]
        [HttpPost("Edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddUpdateScratchWinGameRequest model, int? id = null, int? gameId = null)
        {
            return await Create(model, id, gameId);
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _scratchWinRepository.DeleteScratchWinGameAsync(id);
            return Json(result);
        }

        [HttpPost("ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _scratchWinRepository.ToggleGameStatusAsync(id);
            return Json(result);
        }

        private List<AddUpdateScratchWinRewardRequest> GetDefaultRewards()
        {
            return new List<AddUpdateScratchWinRewardRequest>
            {
                new AddUpdateScratchWinRewardRequest
                {
                    RewardOrder = 1,
                    RewardType = "Prize",
                    RewardName = "Free Coffee",
                    CoinValue = 0,
                    ProbabilityPercentage = 15.00m,
                    TotalStock = 100,
                    RewardImage = "Images/scratch_win/default/free_coffee.png",
                    Description = "1st Prize: Fresh brewed coffee",
                    WinMessage = "Congratulations! You won a Free Coffee!",
                    IsActive = true
                },
                new AddUpdateScratchWinRewardRequest
                {
                    RewardOrder = 2,
                    RewardType = "Prize",
                    RewardName = "Free Burger",
                    CoinValue = 0,
                    ProbabilityPercentage = 8.00m,
                    TotalStock = 50,
                    RewardImage = "Images/scratch_win/default/free_burger.png",
                    Description = "2nd Prize: Delicious burger",
                    WinMessage = "Congratulations! You won a Free Burger!",
                    IsActive = true
                },
                new AddUpdateScratchWinRewardRequest
                {
                    RewardOrder = 3,
                    RewardType = "Prize",
                    RewardName = "Mystery Gift",
                    CoinValue = 0,
                    ProbabilityPercentage = 5.00m,
                    TotalStock = 25,
                    RewardImage = "Images/scratch_win/default/mystery_gift.png",
                    Description = "3rd Prize: Exclusive surprise gift",
                    WinMessage = "Congratulations! You won a Mystery Gift!",
                    IsActive = true
                },
                new AddUpdateScratchWinRewardRequest
                {
                    RewardOrder = 4,
                    RewardType = "Coins",
                    RewardName = "200 IndoCoins",
                    CoinValue = 200,
                    ProbabilityPercentage = 1.00m,
                    TotalStock = null,
                    RewardImage = "Images/scratch_win/default/200_indocoins.png",
                    Description = "Jackpot reward of 200 IC",
                    WinMessage = "Jackpot! You won 200 IndoCoins!",
                    IsActive = true
                },
                new AddUpdateScratchWinRewardRequest
                {
                    RewardOrder = 5,
                    RewardType = "Coins",
                    RewardName = "100 IndoCoins",
                    CoinValue = 100,
                    ProbabilityPercentage = 3.00m,
                    TotalStock = null,
                    RewardImage = "Images/scratch_win/default/100_indocoins.png",
                    Description = "Rare reward of 100 IC",
                    WinMessage = "Awesome! You won 100 IndoCoins!",
                    IsActive = true
                },
                new AddUpdateScratchWinRewardRequest
                {
                    RewardOrder = 6,
                    RewardType = "Coins",
                    RewardName = "50 IndoCoins",
                    CoinValue = 50,
                    ProbabilityPercentage = 8.00m,
                    TotalStock = null,
                    RewardImage = "Images/scratch_win/default/50_indocoins.png",
                    Description = "Medium reward of 50 IC",
                    WinMessage = "Great! You won 50 IndoCoins!",
                    IsActive = true
                },
                new AddUpdateScratchWinRewardRequest
                {
                    RewardOrder = 7,
                    RewardType = "Coins",
                    RewardName = "25 IndoCoins",
                    CoinValue = 25,
                    ProbabilityPercentage = 20.00m,
                    TotalStock = null,
                    RewardImage = "Images/scratch_win/default/25_indocoins.png",
                    Description = "Frequent reward of 25 IC",
                    WinMessage = "Nice! You won 25 IndoCoins!",
                    IsActive = true
                },
                new AddUpdateScratchWinRewardRequest
                {
                    RewardOrder = 8,
                    RewardType = "Consolation",
                    RewardName = "Almost There",
                    CoinValue = 5,
                    ProbabilityPercentage = 40.00m,
                    TotalStock = null,
                    RewardImage = "Images/scratch_win/default/almost_there.png",
                    Description = "Earned 5 IC",
                    WinMessage = "Almost There! Earned 5 IC",
                    IsActive = true
                }
            };
        }
    }
}
