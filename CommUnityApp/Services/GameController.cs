using CommUnityApp.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IBrandGameRepository _brandGameRepository;
        private readonly IConfiguration _configuration;

        public GameController(IBrandGameRepository brandGameRepository, IConfiguration configuration)
        {
            _brandGameRepository = brandGameRepository;
            _configuration = configuration;
        }

        [HttpGet("GetAllGames")]
        public async Task<IActionResult> GetAllGames()
        {
            var games = await _brandGameRepository.GetAllBrandGamesAsync();
            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');

            var basicGames = games.Select(game => new
            {
                gameId = game.BrandGameID,
                gameName = game.BrandGameName,
                gameTitle = game.BrandGameTitle,
                gameImage = BuildFullImageUrl(baseUrl, game.BrandGameImage),
                dateStart = game.DateStart,
                dateEnd = game.DateEnd,
                status = game.Status
            });

            return Ok(basicGames);
        }

        [HttpPost("PlayGame")]
        public async Task<IActionResult> PlayGame([FromBody] PlayGameRequest request)
        {
            if (request == null || request.GameId <= 0)
            {
                return BadRequest(new { resultId = 0, resultMessage = "Valid gameId is required." });
            }

            if (request.MemberId <= 0)
            {
                return BadRequest(new { resultId = 0, resultMessage = "Valid memberId is required." });
            }

            var game = await _brandGameRepository.GetBrandGameByIdAsync(request.GameId);
            if (game == null)
            {
                return NotFound(new { resultId = 0, resultMessage = "Game not found." });
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
            var onceIn = game.OnceIn.GetValueOrDefault(1);
            if (onceIn <= 0)
            {
                onceIn = 1;
            }

            var isReleased = game.IsReleased.GetValueOrDefault(0) == 1;
            var attemptNumber = request.AttemptNumber.GetValueOrDefault(0);
            var isWinningAttempt = attemptNumber > 0
                ? attemptNumber % onceIn == 0
                : Random.Shared.Next(1, onceIn + 1) == 1;

            var primaryBalance = game.PrimaryPrizeBalCount.GetValueOrDefault() > 0
                ? game.PrimaryPrizeBalCount.GetValueOrDefault()
                : game.PrimaryPrizeCount.GetValueOrDefault();

            var secondaryBalance = game.SecondaryPrizeBalCount.GetValueOrDefault() > 0
                ? game.SecondaryPrizeBalCount.GetValueOrDefault()
                : game.SecondaryPrizeCount.GetValueOrDefault();

            var desiredPrizeType = "ConsolationPrize";
            var prizeMessage = game.ConsolationMessage;
            var prizeLabel = game.OfferText;
            var prizeImagePath = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage;

            if (isReleased && isWinningAttempt)
            {
                if (primaryBalance > 0)
                {
                    desiredPrizeType = "PrimaryPrize";
                    prizeMessage = game.PrimaryWinMessage;
                    prizeLabel = game.PrimaryOfferText;
                    prizeImagePath = game.PrimaryPrizeImage ?? game.BrandGameImage;
                }
                else if (secondaryBalance > 0)
                {
                    desiredPrizeType = "SecondaryPrize";
                    prizeMessage = game.SecondaryWinMessage;
                    prizeLabel = game.OfferText;
                    prizeImagePath = game.SecondaryPrizeImage ?? game.BrandGameImage;
                }
            }

            var consumeResult = await _brandGameRepository.TryConsumePrizeAsync(game.BrandGameID, desiredPrizeType);
            var finalPrizeType = desiredPrizeType;

            if (!consumeResult.IsConsumed && desiredPrizeType != "ConsolationPrize")
            {
                finalPrizeType = "ConsolationPrize";
                prizeMessage = game.ConsolationMessage;
                prizeLabel = game.OfferText;
                prizeImagePath = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage;
                consumeResult = await _brandGameRepository.TryConsumePrizeAsync(game.BrandGameID, finalPrizeType);
            }

            if (!consumeResult.IsConsumed)
            {
                await _brandGameRepository.TrackGameplayAsync(
                    game.BrandGameID,
                    request.MemberId,
                    "NoPrize",
                    false,
                    attemptNumber > 0 ? attemptNumber : null
                );

                return Ok(new
                {
                    resultId = 0,
                    resultMessage = "No prize balance available.",
                    gameId = game.BrandGameID,
                    memberId = request.MemberId,
                    onceIn,
                    attemptNumber = attemptNumber > 0 ? (int?)attemptNumber : null,
                    isReleased,
                    isWinner = false,
                    prizeType = "NoPrize",
                    prizeLabel = string.Empty,
                    prizeMessage = "Prize stock is over.",
                    prizeImage = BuildFullImageUrl(baseUrl, game.UnSuccessfulImage ?? game.BrandGameImage),
                    prizeBalances = new
                    {
                        primary = consumeResult.PrimaryPrizeBalCount,
                        secondary = consumeResult.SecondaryPrizeBalCount,
                        consolation = consumeResult.ConsolationPrizeBalCount
                    }
                });
            }

            var isWinner = finalPrizeType != "ConsolationPrize";
            await _brandGameRepository.TrackGameplayAsync(
                game.BrandGameID,
                request.MemberId,
                finalPrizeType,
                isWinner,
                attemptNumber > 0 ? attemptNumber : null
            );

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Game played successfully.",
                gameId = game.BrandGameID,
                memberId = request.MemberId,
                onceIn,
                attemptNumber = attemptNumber > 0 ? (int?)attemptNumber : null,
                isReleased,
                isWinner,
                prizeType = finalPrizeType,
                prizeLabel,
                prizeMessage,
                prizeImage = BuildFullImageUrl(baseUrl, prizeImagePath),
                prizeBalances = new
                {
                    primary = consumeResult.PrimaryPrizeBalCount,
                    secondary = consumeResult.SecondaryPrizeBalCount,
                    consolation = consumeResult.ConsolationPrizeBalCount
                }
            });
        }

        private static string BuildFullImageUrl(string baseUrl, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return imagePath;
            }

            if (Uri.TryCreate(imagePath, UriKind.Absolute, out _))
            {
                return imagePath;
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return imagePath;
            }

            var normalizedImagePath = imagePath.TrimStart('/');
            return $"{baseUrl}/{normalizedImagePath}";
        }

       
    }
}
