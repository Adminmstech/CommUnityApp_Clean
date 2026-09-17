using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Mvc;


namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameResultsController:ControllerBase
    {
        private readonly IConfiguration _configuration;

      

        private readonly IGameResultsRepository _gameResultsRepository;

        public GameResultsController(IGameResultsRepository gameResultsRepository,IConfiguration configuration)
        {
            _gameResultsRepository = gameResultsRepository;
            _configuration = configuration;
        }

        [HttpGet("GetBrandGamePlayMembers")]
        public async Task<IActionResult> GetGamePlayMembers(int page = 1, int size = 10, string search = "")
        {
            var result = await _gameResultsRepository.GetGamePlayMembers(page, size, search);

            return Ok(new
            {
                data = result.Data,          
                totalCount = result.Total
            });
        }

        [HttpGet("GetSpinGameResults")]
        public async Task<IActionResult> GetSpinGameResults(int page = 1, int size = 10, string search = "")
        {
            var result = await _gameResultsRepository.GetSpinGameResults(page, size, search);

            return Ok(new
            {
                data = result.Data,
                totalCount = result.Total
            });
        }

        [HttpGet("GetQuizRankings")]
        public async Task<IActionResult> GetQuizRankings(string? quizType = null, int? quizId = null)
        {
            try
            {
                var result = await _gameResultsRepository.GetQuizRankings(quizType, quizId);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

       

        [HttpPost("AssignPrize")]
        public async Task<IActionResult> AssignPrize([FromBody] AssignPrizeModel model)
        {
            var result = await _gameResultsRepository.AssignPrize(model);
            return Ok(new { status = result }); 
        }


        [HttpGet("GetUserGameHistory")]
        [HttpGet("GetMyPrizes")]
        [HttpGet("GetUserPrizes")]
        [HttpGet("/api/Game/GetMyPrizes")]
        [HttpGet("/api/Game/GetUserGameHistory")]
        public async Task<IActionResult> GetUserGameHistory(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Invalid UserId."
                });
            }

            var history = await _gameResultsRepository.GetUserGameHistory(userId);

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "").TrimEnd('/');

            var result = history.Select(x =>
            {
                string category = !string.IsNullOrWhiteSpace(x.PrizeCategory)
                    ? x.PrizeCategory
                    : (x.RewardValue?.Contains("Voucher", StringComparison.OrdinalIgnoreCase) == true || x.RewardValue?.Contains("Off", StringComparison.OrdinalIgnoreCase) == true
                        ? "Voucher"
                        : (x.RewardValue?.Contains("Free", StringComparison.OrdinalIgnoreCase) == true ? "Freebie" : "Promo"));

                string qrUrl = string.IsNullOrEmpty(x.QRCodePath)
                    ? null
                    : BuildFullImageUrl(baseUrl, x.QRCodePath);

                return new
                {
                    x.GameResultId,
                    x.GameType,
                    x.GameId,
                    x.GameName,
                    x.GameTitle,
                    prizeTitle = x.RewardValue,
                    prizeCategory = category,
                    isRedeemable = !string.IsNullOrEmpty(x.RedeemCode),

                    GameImage = BuildFullImageUrl(baseUrl, x.GameImage),
                    PrizeImage = BuildFullImageUrl(baseUrl, x.PrizeImage),
                    x.PlayedAt,
                    x.RewardValue,
                    x.RedeemCode,
                    RedeemQRCode = qrUrl,
                    redeemQrCode = qrUrl,
                    x.IsWinner,
                    x.PointsAwarded,
                    BusinessLocation = !string.IsNullOrEmpty(x.RedeemCode) ? x.BusinessLocation : null,
                    businessLocation = !string.IsNullOrEmpty(x.RedeemCode) ? x.BusinessLocation : null,
                    x.SectionId,
                    SectionImage = BuildFullImageUrl(baseUrl, x.SectionImage)
                };
            }).ToList();

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Data = result,
                prizes = result
            });
        }

        private string BuildFullImageUrl(string baseUrl, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return string.Empty;

            imagePath = imagePath.Replace("\\", "/");

            if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            return $"{baseUrl}/{imagePath.TrimStart('/')}";
        }
    }
}
