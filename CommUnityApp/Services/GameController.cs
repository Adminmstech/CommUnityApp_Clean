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
        private readonly ISpinGameRepository _spinGameRepository; // Added
        private readonly IConfiguration _configuration;

        public GameController(IBrandGameRepository brandGameRepository, ISpinGameRepository spinGameRepository, IConfiguration configuration) // Modified
        {
            _brandGameRepository = brandGameRepository;
            _spinGameRepository = spinGameRepository; // Added
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

        [HttpPost("AddUpdateSpinGame")] // New API endpoint for SpinGame
        public async Task<IActionResult> AddUpdateSpinGame([FromBody] AddUpdateSpinGameRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _spinGameRepository.AddUpdateSpinGameAsync(request);

            if (result.ResultId > 0)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result); // Internal server error or specific error from repository
            }
        }
        [HttpGet("GetActiveSpinGame")]
        public async Task<IActionResult> GetActiveSpinGame(int businessId)
        {
            if (businessId < 0) return BadRequest(new { resultId = 0, resultMessage = "Valid businessId is required. Pass 0 for all active games." });

            IEnumerable<SpinGameDto> games;
            if (businessId == 0)
            {
                games = await _spinGameRepository.GetAllSpinGamesAsync();
            }
            else
            {
                games = await _spinGameRepository.GetSpinGamesByBusinessAsync(businessId);
            }

            if (games == null || !games.Any())
            {
                return NotFound(new { resultId = 0, resultMessage = "No active spin games found." });
            }

            var fullyPopulatedGames = new List<object>();

            foreach (var game in games)
            {
                var config = await _spinGameRepository.GetConfigByIdAsync(game.ConfigId);
                var sections = await _spinGameRepository.GetSectionsByGameIdAsync(game.GameId);

                fullyPopulatedGames.Add(new
                {
                    game = game,
                    config = config,
                    sections = sections
                });
            }

            return Ok(new
            {
                resultId = 1,
                resultMessage = fullyPopulatedGames.Count + " Spin game(s) found.",
                games = fullyPopulatedGames
            });
        }

        [HttpGet("GetSpinGameDetails")]
        public async Task<IActionResult> GetSpinGameDetails(int gameId)
        {
            if (gameId <= 0) return BadRequest(new { resultId = 0, resultMessage = "Valid gameId is required." });

            var game = await _spinGameRepository.GetSpinGameByIdAsync(gameId);
            
            if (game == null)
            {
                return NotFound(new { resultId = 0, resultMessage = "Spin game not found." });
            }

            var config = await _spinGameRepository.GetConfigByIdAsync(game.ConfigId);
            var sections = await _spinGameRepository.GetSectionsByGameIdAsync(game.GameId);

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Spin game found.",
                game = game,
                config = config,
                sections = sections
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

        [HttpPost("PlaySpinGame")]
        public async Task<IActionResult> PlaySpinGame([FromBody] PlaySpinRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request == null || request.GameId <= 0)
            {
                return BadRequest(new { resultId = 0, resultMessage = "Valid gameId is required." });
            }

            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { resultId = 0, resultMessage = "Valid userId is required." });
            }

            if (request.SectionId <= 0)
            {
                return BadRequest(new { resultId = 0, resultMessage = "Valid sectionId is required." });
            }

            var result = await _spinGameRepository.PlaySpinGameAsync(request);

            if (result.ResultId > 0)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("GetGameSpinResults")]
        public async Task<IActionResult> GetGameSpinResults(int? gameId = null, Guid? userId = null)
        {
            var results = await _spinGameRepository.GetGameSpinResultsAsync(gameId, userId);
            
            if (results == null || !results.Any())
            {
                return NotFound(new { resultId = 0, resultMessage = "No spin results found." });
            }

            return Ok(new { resultId = 1, resultMessage = $"{results.Count()} result(s) found.", results = results });
        }

        [HttpGet("GetAllPrizes")]
        public async Task<IActionResult> GetAllPrizes()
        {
            var allPrizes = new List<PrizeDto>();
            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');

            // Fetch Brand Game Prizes (Scratch & Win)
            var brandGames = await _brandGameRepository.GetAllBrandGamesAsync();
            foreach (var game in brandGames)
            {
                if (game.PrimaryPrizeCount > 0)
                {
                    allPrizes.Add(new PrizeDto
                    {
                        Name = game.PrimaryOfferText,
                        Description = game.PrimaryWinMessage,
                        ImageUrl = BuildFullImageUrl(baseUrl, game.PrimaryPrizeImage),
                        GameType = "ScratchAndWin",
                        GameId = game.BrandGameID,
                        PrizeType = "Primary"
                    });
                }
                if (game.SecondaryPrizeCount > 0)
                {
                    allPrizes.Add(new PrizeDto
                    {
                        Name = game.OfferText, // Assuming OfferText is for secondary prize label
                        Description = game.SecondaryWinMessage,
                        ImageUrl = BuildFullImageUrl(baseUrl, game.SecondaryPrizeImage),
                        GameType = "ScratchAndWin",
                        GameId = game.BrandGameID,
                        PrizeType = "Secondary"
                    });
                }
                if (game.ConsolationPrizeCount > 0)
                {
                    allPrizes.Add(new PrizeDto
                    {
                        Name = game.OfferText, // Assuming OfferText is for consolation prize label
                        Description = game.ConsolationMessage,
                        ImageUrl = BuildFullImageUrl(baseUrl, game.ConsolationPrizeImage),
                        GameType = "ScratchAndWin",
                        GameId = game.BrandGameID,
                        PrizeType = "Consolation"
                    });
                }
            }

            // Fetch Spin Game Prizes
            var spinGames = await _spinGameRepository.GetAllSpinGamesAsync();
            foreach (var spinGame in spinGames)
            {
                var sections = await _spinGameRepository.GetSectionsByGameIdAsync(spinGame.GameId);
                foreach (var section in sections)
                {
                    allPrizes.Add(new PrizeDto
                    {
                        Name = section.PrizeText, // Use PrizeText for Name
                        Description = section.PrizeText, // Use PrizeText for Description
                        ImageUrl = BuildFullImageUrl(baseUrl, spinGame.GameImage), // Use SpinGameDto's GameImage
                        GameType = "SpinAndWin",
                        GameId = spinGame.GameId,
                        PrizeType = "Section"
                    });
                }
            }

            return Ok(new { resultId = 1, resultMessage = $"{allPrizes.Count} prizes found.", prizes = allPrizes });
        }

        }
}
