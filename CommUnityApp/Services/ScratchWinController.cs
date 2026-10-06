using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScratchWinController : ControllerBase
    {
        private readonly IScratchWinRepository _scratchWinRepository;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public ScratchWinController(
            IScratchWinRepository scratchWinRepository,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _scratchWinRepository = scratchWinRepository;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet("GetActiveGame")]
        public async Task<IActionResult> GetActiveGame()
        {
            var game = await _scratchWinRepository.GetActiveScratchWinGameAsync();
            if (game == null)
            {
                return NotFound(new
                {
                    resultId = 0,
                    resultMessage = "No active Scratch & Win game found."
                });
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "").TrimEnd('/');

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Active Scratch & Win game retrieved successfully.",
                gameId = game.GameId,
                gameName = game.GameName,
                gameTitle = game.GameTitle,
                description = game.GameDescription,
                terms = game.TermsAndConditions,
                startDate = game.StartDate,
                endDate = game.EndDate,
                chanceCount = game.ChanceCount,
                onceIn = game.OnceIn,
                businessId = game.BusinessId,
                businessName = game.BusinessName,
                businessLocation = game.BusinessLocation,
                rewards = game.Rewards.Select(r => new
                {
                    rewardId = r.RewardId,
                    order = r.RewardOrder,
                    type = r.RewardType,
                    name = r.RewardName,
                    coinValue = r.CoinValue,
                    probability = r.ProbabilityPercentage,
                    availableStock = r.AvailableStock,
                    businessId = (r.BusinessId.HasValue && r.BusinessId.Value > 0) ? r.BusinessId.Value : game.BusinessId,
                    businessName = !string.IsNullOrWhiteSpace(r.BusinessName) ? r.BusinessName : game.BusinessName,
                    businessLocation = !string.IsNullOrWhiteSpace(r.BusinessLocation) ? r.BusinessLocation : game.BusinessLocation,
                    image = BuildFullImageUrl(baseUrl, r.RewardImage),
                    description = r.Description,
                    winMessage = r.WinMessage
                })
            });
        }

        [HttpGet("GetGameDetails")]
        public async Task<IActionResult> GetGameDetails([FromQuery] int gameId)
        {
            if (gameId <= 0)
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Valid gameId is required."
                });
            }

            var game = await _scratchWinRepository.GetScratchWinGameByIdAsync(gameId);
            if (game == null)
            {
                return NotFound(new
                {
                    resultId = 0,
                    resultMessage = "Game not found."
                });
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "").TrimEnd('/');

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Game details retrieved successfully.",
                gameId = game.GameId,
                gameName = game.GameName,
                gameTitle = game.GameTitle,
                description = game.GameDescription,
                terms = game.TermsAndConditions,
                startDate = game.StartDate,
                endDate = game.EndDate,
                chanceCount = game.ChanceCount,
                onceIn = game.OnceIn,
                status = game.Status,
                businessId = game.BusinessId,
                businessName = game.BusinessName,
                businessLocation = game.BusinessLocation,
                rewards = game.Rewards.Select(r => new
                {
                    rewardId = r.RewardId,
                    order = r.RewardOrder,
                    type = r.RewardType,
                    name = r.RewardName,
                    coinValue = r.CoinValue,
                    probability = r.ProbabilityPercentage,
                    availableStock = r.AvailableStock,
                    businessId = (r.BusinessId.HasValue && r.BusinessId.Value > 0) ? r.BusinessId.Value : game.BusinessId,
                    businessName = !string.IsNullOrWhiteSpace(r.BusinessName) ? r.BusinessName : game.BusinessName,
                    businessLocation = !string.IsNullOrWhiteSpace(r.BusinessLocation) ? r.BusinessLocation : game.BusinessLocation,
                    image = BuildFullImageUrl(baseUrl, r.RewardImage),
                    description = r.Description,
                    winMessage = r.WinMessage
                })
            });
        }

        [HttpPost("SubmitScratchGame")]
        public async Task<IActionResult> SubmitScratchGame([FromBody] PlayScratchGameRequest request)
        {
            if (request == null || request.GameId <= 0)
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Valid gameId is required."
                });
            }

            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Valid userId is required."
                });
            }

            var game = await _scratchWinRepository.GetScratchWinGameByIdAsync(request.GameId);
            if (game == null || game.Status != 1)
            {
                return NotFound(new
                {
                    resultId = 0,
                    resultMessage = "Game not found or inactive."
                });
            }

            if (game.Rewards == null || !game.Rewards.Any())
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "No rewards configured for this game."
                });
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "").TrimEnd('/');

            var onceIn = game.OnceIn <= 0 ? 1 : game.OnceIn;
            int totalPlays = await _scratchWinRepository.GetTotalPlaysCountAsync(game.GameId);
            var attemptNumber = totalPlays + 1;
            bool isWinningAttempt = (attemptNumber % onceIn == 0);

            // Find consolation / "Almost There" reward as fallback
            var consolationReward = game.Rewards.FirstOrDefault(r => r.RewardType == "Consolation")
                                    ?? game.Rewards.OrderByDescending(r => r.RewardOrder).FirstOrDefault()!;

            // Filter active rewards with positive probability
            var activeRewards = game.Rewards
                .Where(r => r.IsActive && r.ProbabilityPercentage > 0)
                .OrderBy(r => r.RewardOrder)
                .ToList();

            ScratchWinRewardDto selectedReward = consolationReward;

            if (activeRewards.Any())
            {
                decimal totalWeight = activeRewards.Sum(r => r.ProbabilityPercentage);
                if (totalWeight <= 0) totalWeight = 100.0m;

                // High-precision cryptographic random roll scaled to totalWeight
                // 1,000,000 discrete steps gives 0.0001% precision with uniform distribution
                int randomInt = RandomNumberGenerator.GetInt32(0, 1000000);
                decimal roll = ((decimal)randomInt / 1000000.0m) * totalWeight;

                decimal cumulative = 0m;
                foreach (var reward in activeRewards)
                {
                    cumulative += reward.ProbabilityPercentage;
                    if (roll < cumulative)
                    {
                        selectedReward = reward;
                        break;
                    }
                }
            }

            // If selected is a physical prize (1st, 2nd, 3rd)
            if (selectedReward.RewardType == "Prize")
            {
                var stock = selectedReward.AvailableStock.GetValueOrDefault(0);

                // When OnceIn <= 1: Pure probability mode (Admin-configured probability directly governs every win)
                // When OnceIn > 1: Gated mode (Prizes additionally restricted to every Nth attempt)
                bool attemptAllowed = (onceIn <= 1) || isWinningAttempt;

                if (!attemptAllowed || stock <= 0)
                {
                    // Fall back to Almost There / Consolation
                    selectedReward = consolationReward;
                }
            }

            var verificationToken = GenerateVerificationToken(game.GameId, request.UserId, selectedReward.RewardId, attemptNumber);

            int? effectiveBusinessId = selectedReward.RewardType == "Prize"
                ? (selectedReward.BusinessId.HasValue && selectedReward.BusinessId.Value > 0 ? selectedReward.BusinessId.Value : game.BusinessId)
                : null;
            string? effectiveLocation = selectedReward.RewardType == "Prize"
                ? (!string.IsNullOrWhiteSpace(selectedReward.BusinessLocation) ? selectedReward.BusinessLocation : game.BusinessLocation)
                : null;
            string? effectiveBusinessName = selectedReward.RewardType == "Prize"
                ? (!string.IsNullOrWhiteSpace(selectedReward.BusinessName) ? selectedReward.BusinessName : game.BusinessName)
                : null;

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Prize revealed successfully.",
                gameId = game.GameId,
                userId = request.UserId,
                rewardId = selectedReward.RewardId,
                rewardOrder = selectedReward.RewardOrder,
                rewardType = selectedReward.RewardType,
                rewardName = selectedReward.RewardName,
                coinValue = selectedReward.CoinValue,
                description = selectedReward.Description,
                winMessage = selectedReward.WinMessage,
                rewardImage = BuildFullImageUrl(baseUrl, selectedReward.RewardImage),
                businessId = effectiveBusinessId,
                businessName = effectiveBusinessName,
                businessLocation = effectiveLocation,
                attemptNumber = attemptNumber,
                verificationToken = verificationToken
            });
        }

        [HttpPost("RedeemScratchPrize")]
        public async Task<IActionResult> RedeemScratchPrize([FromBody] RedeemScratchPrizeRequest request)
        {
            if (request == null || request.GameId <= 0 || request.UserId == Guid.Empty || !request.RewardId.HasValue || string.IsNullOrEmpty(request.VerificationToken))
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Valid GameId, UserId, RewardId, and VerificationToken are required."
                });
            }

            // Verify verification token
            var expectedToken = GenerateVerificationToken(request.GameId, request.UserId, request.RewardId.Value, request.AttemptNumber);
            if (request.VerificationToken != expectedToken)
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Invalid or tampered verification token."
                });
            }

            var game = await _scratchWinRepository.GetScratchWinGameByIdAsync(request.GameId);
            if (game == null)
            {
                return NotFound(new
                {
                    resultId = 0,
                    resultMessage = "Game not found."
                });
            }

            var reward = game.Rewards.FirstOrDefault(r => r.RewardId == request.RewardId.Value);
            if (reward == null)
            {
                return NotFound(new
                {
                    resultId = 0,
                    resultMessage = "Reward not found."
                });
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "").TrimEnd('/');

            bool isWinner = false;
            int coinsEarned = 0;
            string? redeemCode = null;
            string? qrCodePath = null;

            if (reward.RewardType == "Prize")
            {
                // Try consuming stock atomically
                bool consumed = await _scratchWinRepository.TryConsumeRewardStockAsync(reward.RewardId);
                if (consumed)
                {
                    isWinner = true;
                    // Generate Redeem code and QR code ONLY for physical prizes
                    redeemCode = GenerateRedeemCode();
                    qrCodePath = GenerateQRCode(redeemCode);
                }
                else
                {
                    // Fall back to Almost There (Consolation)
                    var consolation = game.Rewards.FirstOrDefault(r => r.RewardType == "Consolation") ?? reward;
                    reward = consolation;
                    isWinner = false;
                    coinsEarned = reward.CoinValue > 0 ? reward.CoinValue : 5;
                }
            }
            else if (reward.RewardType == "Coins")
            {
                isWinner = true;
                coinsEarned = reward.CoinValue;
                // No QR or Redeem code for coins
                redeemCode = null;
                qrCodePath = null;
            }
            else // Consolation / Almost There
            {
                isWinner = false;
                coinsEarned = reward.CoinValue > 0 ? reward.CoinValue : 5;
                // No QR or Redeem code for consolation
                redeemCode = null;
                qrCodePath = null;
            }

            // Location is only added for physical prize redeem codes, not for coins
            string? prizeLocation = !string.IsNullOrWhiteSpace(reward.BusinessLocation) ? reward.BusinessLocation : game.BusinessLocation;
            string? prizeBusinessName = !string.IsNullOrWhiteSpace(reward.BusinessName) ? reward.BusinessName : game.BusinessName;
            int? effectiveBusinessId = (isWinner && reward.RewardType == "Prize") 
                ? (reward.BusinessId.HasValue && reward.BusinessId.Value > 0 ? reward.BusinessId.Value : game.BusinessId) 
                : null;
            string? redeemLocation = (isWinner && reward.RewardType == "Prize") ? prizeLocation : null;
            string? redeemBusinessName = (isWinner && reward.RewardType == "Prize") ? prizeBusinessName : null;

            // Track gameplay permanently in history
            await _scratchWinRepository.TrackGameplayAsync(
                game.GameId,
                request.UserId,
                reward.RewardId,
                reward.RewardName,
                reward.RewardType,
                coinsEarned,
                isWinner,
                request.AttemptNumber,
                redeemCode,
                qrCodePath,
                redeemLocation,
                effectiveBusinessId);

            // Add coins to user wallet
            if (coinsEarned > 0)
            {
                await _scratchWinRepository.AddRewardCoinsAsync(
                    request.UserId,
                    coinsEarned,
                    game.GameId);
            }

            // Get fresh game details for stock
            var freshGame = await _scratchWinRepository.GetScratchWinGameByIdAsync(game.GameId);
            var freshReward = freshGame?.Rewards.FirstOrDefault(r => r.RewardId == reward.RewardId);

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Prize redeemed successfully.",
                gameId = game.GameId,
                userId = request.UserId,
                isWinner = isWinner,
                rewardId = reward.RewardId,
                rewardName = reward.RewardName,
                rewardType = reward.RewardType,
                coinValue = coinsEarned,
                rewardImage = BuildFullImageUrl(baseUrl, reward.RewardImage),
                winMessage = reward.WinMessage,
                description = reward.Description,
                redeemCode = redeemCode,
                redeemQrCode = BuildFullImageUrl(baseUrl, qrCodePath),
                redeemLocation = redeemLocation,
                businessLocation = redeemLocation,
                businessId = effectiveBusinessId,
                businessName = redeemBusinessName,
                availableStock = freshReward?.AvailableStock
            });
        }

        private string GenerateVerificationToken(int gameId, Guid userId, int rewardId, int attemptNumber)
        {
            var salt = "CommUnityApp_ScratchWin_Salt_2026";
            var raw = $"{gameId}:{userId}:{rewardId}:{attemptNumber}:{salt}";
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateRedeemCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[6];
            rng.GetBytes(bytes);
            var result = new char[6];
            for (int i = 0; i < 6; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }
            return new string(result);
        }

        private string GenerateQRCode(string redeemCode)
        {
            string folder = Path.Combine(
                _environment.WebRootPath,
                "Images",
                "scratch_win",
                "QR");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = $"{redeemCode}.png";
            string fullPath = Path.Combine(folder, fileName);

            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(redeemCode, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            byte[] bytes = qr.GetGraphic(20);

            System.IO.File.WriteAllBytes(fullPath, bytes);

            return $"Images/scratch_win/QR/{fileName}";
        }

        private string BuildFullImageUrl(string baseUrl, string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return string.Empty;

            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;

            return $"{baseUrl}/{imagePath.TrimStart('/')}";
        }
    }
}
