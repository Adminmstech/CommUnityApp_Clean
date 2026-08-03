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

            if (request.UserId == Guid.Empty)
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
                var trackresult=await _brandGameRepository.TrackGameplayAsync(
                    game.BrandGameID,
                    request.UserId,
                    "NoPrize",
                    false,
                    attemptNumber > 0 ? attemptNumber : null
                );

                return Ok(new
                {
                    RedeemCode= trackresult.ResultMessage,
                    resultId = 0,
                    resultMessage = "No prize balance available.",
                    gameId = game.BrandGameID,
                    memberId = request.UserId,
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
            var trackresult1 = await _brandGameRepository.TrackGameplayAsync(
                game.BrandGameID,
                request.UserId,
                finalPrizeType, 
                isWinner,
                attemptNumber > 0 ? attemptNumber : null
            );
            await _brandGameRepository.AddRewardCoinsAsync(
            request.UserId,
            game.PointsAwarded.GetValueOrDefault(),
            game.BrandGameID);
            return Ok(new
            {
                RedeemCode = trackresult1.ResultMessage,
                resultId = 1,
                resultMessage = "Game played successfully.",
                gameId = game.BrandGameID,
                memberId = request.UserId,
                onceIn,
                attemptNumber = attemptNumber > 0 ? (int?)attemptNumber : null,
                isReleased,
                isWinner,
                prizeType = finalPrizeType,
                prizeLabel,
                prizeMessage,
                prizeImage = BuildFullImageUrl(baseUrl, prizeImagePath),
                coinsEarned = game.PointsAwarded.GetValueOrDefault(),

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
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Valid gameId is required."
                });
            }

            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Valid userId is required."
                });
            }

            if (request.SectionId <= 0)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Valid sectionId is required."
                });
            }

            var result = await _spinGameRepository.PlaySpinGameAsync(request);

            if (result.ResultId > 0)
            {
                var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
                var game = await _spinGameRepository.GetSpinGameByIdAsync(request.GameId);
                var section = await _spinGameRepository.GetSectionByIdAsync(request.SectionId);

                if (game != null && game.RewardCoins > 0)
                {
                    await _spinGameRepository.AddSpinGameRewardCoinsAsync(
                        request.UserId,
                        game.RewardCoins,
                        request.GameId);
                }
                
                // Backward compatible response + enriched fields for app/store redemption flows.
                return Ok(new
                {
                    // existing result payload preserved
                    result.ResultId,
                    result.ResultMessage,
                    result.GameResultId,
                    result.GameId,
                    result.SectionId,
                    result.RewardValue,
                    result.RedeemCode,
                    result.Status,
                    result.PlayedAt,

                    // enriched fields
                    gameImage = BuildFullImageUrl(baseUrl, game?.GameImage),
                    offerText = section?.PrizeText ?? result.RewardValue,
                    sectionImage = BuildFullImageUrl(baseUrl, section?.SectionImage),
                    redeemCode = result.RedeemCode,
                    reward = new
                    {
                        gameId = result.GameId,
                        sectionId = result.SectionId,
                        offerText = section?.PrizeText ?? result.RewardValue,
                        redeemCode = result.RedeemCode,
                        gameImage = BuildFullImageUrl(baseUrl, game?.GameImage),
                        sectionImage = BuildFullImageUrl(baseUrl, section?.SectionImage)
                    }
                });
            }

            return BadRequest(result);
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

        [HttpPost("GetBrandGameDetails")]
        public async Task<IActionResult> GetBrandGameDetails([FromBody] GetGameDetails request)
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
                    resultMessage = "Valid memberId is required."
                });
            }

            var game = await _brandGameRepository.GetBrandGameByIdAsync(request.GameId);

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

                gameId = game.BrandGameID,

                gameName = game.BrandGameName,

                gameTitle = game.BrandGameTitle,

                description = game.BrandGameDesc,

                conditionsApply = game.ConditionsApply,

                destinationUrl = game.DestinationUrl,

                onceIn = game.OnceIn,

                isReleased = game.IsReleased,

                panelCount = game.PanelCount,

                panelOpeningLimit = game.PanelOpeningLimit,

                chanceCount = game.ChanceCount,

                pointsAwarded = game.PointsAwarded,

                expiryText = game.ExpiryText,

                permitNumber = game.PermitNumber,

                classNumber = game.ClassNumber,

                formColor = game.FormColor,

                textColor = game.TextColor,

                promotionalCode = game.PromotionalCode,

                startDate = game.DateStart,

                endDate = game.DateEnd,

                gameImage = BuildFullImageUrl(baseUrl, game.BrandGameImage),

                primaryPrizeImage = BuildFullImageUrl(baseUrl, game.PrimaryPrizeImage),

                secondaryPrizeImage = BuildFullImageUrl(baseUrl, game.SecondaryPrizeImage),

                consolationPrizeImage = BuildFullImageUrl(baseUrl, game.ConsolationPrizeImage),

                unsuccessfulImage = BuildFullImageUrl(baseUrl, game.UnSuccessfulImage),

                primaryOfferText = game.PrimaryOfferText,

                secondaryOfferText = game.OfferText,

                primaryWinMessage = game.PrimaryWinMessage,

                secondaryWinMessage = game.SecondaryWinMessage,

                consolationMessage = game.ConsolationMessage,

                prizeBalance = new
                {
                    primary = game.PrimaryPrizeBalCount,
                    secondary = game.SecondaryPrizeBalCount,
                    consolation = game.ConsolationPrizeBalCount
                }
            });
        }


        [HttpPost("SubmitBrandGame")]
        public async Task<IActionResult> SubmitBrandGame([FromBody] PlayGameRequest request)
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
                    resultMessage = "Valid memberId is required."
                });
            }

            var game = await _brandGameRepository.GetBrandGameByIdAsync(request.GameId);

            if (game == null)
            {
                return NotFound(new
                {
                    resultId = 0,
                    resultMessage = "Game not found."
                });
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "").TrimEnd('/');

            var onceIn = game.OnceIn.GetValueOrDefault(1);

            if (onceIn <= 0)
                onceIn = 1;

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

            string finalPrizeType = "ConsolationPrize";
            string prizeLabel = game.OfferText;
            string prizeMessage = game.ConsolationMessage;
            string prizeImage = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage;

            if (isReleased && isWinningAttempt)
            {
                if (primaryBalance > 0)
                {
                    finalPrizeType = "PrimaryPrize";
                    prizeLabel = game.PrimaryOfferText;
                    prizeMessage = game.PrimaryWinMessage;
                    prizeImage = game.PrimaryPrizeImage ?? game.BrandGameImage;
                }
                else if (secondaryBalance > 0)
                {
                    finalPrizeType = "SecondaryPrize";
                    prizeLabel = game.OfferText;
                    prizeMessage = game.SecondaryWinMessage;
                    prizeImage = game.SecondaryPrizeImage ?? game.BrandGameImage;
                }
            }

            var consumeResult = await _brandGameRepository.TryConsumePrizeAsync(
                game.BrandGameID,
                finalPrizeType);

            if (!consumeResult.IsConsumed && finalPrizeType != "ConsolationPrize")
            {
                finalPrizeType = "ConsolationPrize";

                prizeLabel = game.OfferText;
                prizeMessage = game.ConsolationMessage;
                prizeImage = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage;

                consumeResult = await _brandGameRepository.TryConsumePrizeAsync(
                    game.BrandGameID,
                    finalPrizeType);
            }

            if (!consumeResult.IsConsumed)
            {
                await _brandGameRepository.TrackGameplayAsync(
                    game.BrandGameID,
                    request.UserId,
                    "NoPrize",
                    false,
                    attemptNumber > 0 ? attemptNumber : null);

                return Ok(new
                {
                    resultId = 0,
                    resultMessage = "No prize balance available.",

                    gameId = game.BrandGameID,
                    memberId = request.UserId,

                    isWinner = false,

                    prizeType = "NoPrize",

                    prizeLabel = "",

                    prizeMessage = "Prize stock is over.",

                    prizeImage = BuildFullImageUrl(
                        baseUrl,
                        game.UnSuccessfulImage ?? game.BrandGameImage),

                    coinsEarned = 0,

                    prizeBalances = new
                    {
                        primary = consumeResult.PrimaryPrizeBalCount,
                        secondary = consumeResult.SecondaryPrizeBalCount,
                        consolation = consumeResult.ConsolationPrizeBalCount
                    }
                });
            }

            bool isWinner = finalPrizeType != "ConsolationPrize";

            await _brandGameRepository.TrackGameplayAsync(
                game.BrandGameID,
                request.UserId,
                finalPrizeType,
                isWinner,
                attemptNumber > 0 ? attemptNumber : null);

            await _brandGameRepository.AddRewardCoinsAsync(
                request.UserId,
                game.PointsAwarded.GetValueOrDefault(),
                game.BrandGameID);

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Game played successfully.",

                gameId = game.BrandGameID,
                memberId = request.UserId,

                isWinner,

                prizeType = finalPrizeType,

                prizeLabel,

                prizeMessage,

                prizeImage = BuildFullImageUrl(baseUrl, prizeImage),

                coinsEarned = game.PointsAwarded.GetValueOrDefault(),

                prizeBalances = new
                {
                    primary = consumeResult.PrimaryPrizeBalCount,
                    secondary = consumeResult.SecondaryPrizeBalCount,
                    consolation = consumeResult.ConsolationPrizeBalCount
                }
            });
        }


    }
}
