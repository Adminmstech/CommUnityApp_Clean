using CommUnityApp.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using CommUnityApp.ApplicationCore.Models;
using QRCoder;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IBrandGameRepository _brandGameRepository;
        private readonly ISpinGameRepository _spinGameRepository; // Added
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;


        public GameController(IWebHostEnvironment environment, IBrandGameRepository brandGameRepository, ISpinGameRepository spinGameRepository, IConfiguration configuration) // Modified
        {
            _environment = environment;
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

        //[HttpPost("PlayGame")]
        //public async Task<IActionResult> PlayGame([FromBody] PlayGameRequest request)
        //{
        //    if (request == null || request.GameId <= 0)
        //    {
        //        return BadRequest(new { resultId = 0, resultMessage = "Valid gameId is required." });
        //    }

        //    if (request.UserId == Guid.Empty)
        //    {
        //        return BadRequest(new { resultId = 0, resultMessage = "Valid memberId is required." });
        //    }

        //    var game = await _brandGameRepository.GetBrandGameByIdAsync(request.GameId);
        //    if (game == null)
        //    {
        //        return NotFound(new { resultId = 0, resultMessage = "Game not found." });
        //    }

        //    var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
        //    var onceIn = game.OnceIn.GetValueOrDefault(1);
        //    if (onceIn <= 0)
        //    {
        //        onceIn = 1;
        //    }

        //    var isReleased = game.IsReleased.GetValueOrDefault(0) == 1;
        //    var attemptNumber = request.AttemptNumber.GetValueOrDefault(0);
        //    var isWinningAttempt = attemptNumber > 0
        //        ? attemptNumber % onceIn == 0
        //        : Random.Shared.Next(1, onceIn + 1) == 1;

        //    var primaryBalance = game.PrimaryPrizeBalCount.GetValueOrDefault() > 0
        //        ? game.PrimaryPrizeBalCount.GetValueOrDefault()
        //        : game.PrimaryPrizeCount.GetValueOrDefault();

        //    var secondaryBalance = game.SecondaryPrizeBalCount.GetValueOrDefault() > 0
        //        ? game.SecondaryPrizeBalCount.GetValueOrDefault()
        //        : game.SecondaryPrizeCount.GetValueOrDefault();

        //    var desiredPrizeType = "ConsolationPrize";
        //    var prizeMessage = game.ConsolationMessage;
        //    var prizeLabel = game.OfferText;
        //    var prizeImagePath = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage;

        //    if (isReleased && isWinningAttempt)
        //    {
        //        if (primaryBalance > 0)
        //        {
        //            desiredPrizeType = "PrimaryPrize";
        //            prizeMessage = game.PrimaryWinMessage;
        //            prizeLabel = game.PrimaryOfferText;
        //            prizeImagePath = game.PrimaryPrizeImage ?? game.BrandGameImage;
        //        }
        //        else if (secondaryBalance > 0)
        //        {
        //            desiredPrizeType = "SecondaryPrize";
        //            prizeMessage = game.SecondaryWinMessage;
        //            prizeLabel = game.OfferText;
        //            prizeImagePath = game.SecondaryPrizeImage ?? game.BrandGameImage;
        //        }
        //    }

        //    var consumeResult = await _brandGameRepository.TryConsumePrizeAsync(game.BrandGameID, desiredPrizeType);
        //    var finalPrizeType = desiredPrizeType;

        //    if (!consumeResult.IsConsumed && desiredPrizeType != "ConsolationPrize")
        //    {
        //        finalPrizeType = "ConsolationPrize";
        //        prizeMessage = game.ConsolationMessage;
        //        prizeLabel = game.OfferText;
        //        prizeImagePath = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage;
        //        consumeResult = await _brandGameRepository.TryConsumePrizeAsync(game.BrandGameID, finalPrizeType);
        //    }

        //    if (!consumeResult.IsConsumed)
        //    {
        //        var trackresult=await _brandGameRepository.TrackGameplayAsync(
        //            game.BrandGameID,
        //            request.UserId,
        //            "NoPrize",
        //            false,
        //            attemptNumber > 0 ? attemptNumber : null
        //        );

        //        return Ok(new
        //        {
        //            RedeemCode= trackresult.ResultMessage,
        //            resultId = 0,
        //            resultMessage = "No prize balance available.",
        //            gameId = game.BrandGameID,
        //            memberId = request.UserId,
        //            onceIn,
        //            attemptNumber = attemptNumber > 0 ? (int?)attemptNumber : null,
        //            isReleased,
        //            isWinner = false,
        //            prizeType = "NoPrize",
        //            prizeLabel = string.Empty,
        //            prizeMessage = "Prize stock is over.",
        //            prizeImage = BuildFullImageUrl(baseUrl, game.UnSuccessfulImage ?? game.BrandGameImage),
        //            prizeBalances = new
        //            {
        //                primary = consumeResult.PrimaryPrizeBalCount,
        //                secondary = consumeResult.SecondaryPrizeBalCount,
        //                consolation = consumeResult.ConsolationPrizeBalCount
        //            }
        //        });
        //    }

        //    var isWinner = finalPrizeType != "ConsolationPrize";
        //    var trackresult1 = await _brandGameRepository.TrackGameplayAsync(
        //        game.BrandGameID,
        //        request.UserId,
        //        finalPrizeType, 
        //        isWinner,
        //        attemptNumber > 0 ? attemptNumber : null
        //    );
        //    await _brandGameRepository.AddRewardCoinsAsync(
        //    request.UserId,
        //    game.PointsAwarded.GetValueOrDefault(),
        //    game.BrandGameID);
        //    return Ok(new
        //    {
        //        RedeemCode = trackresult1.ResultMessage,
        //        resultId = 1,
        //        resultMessage = "Game played successfully.",
        //        gameId = game.BrandGameID,
        //        memberId = request.UserId,
        //        onceIn,
        //        attemptNumber = attemptNumber > 0 ? (int?)attemptNumber : null,
        //        isReleased,
        //        isWinner,
        //        prizeType = finalPrizeType,
        //        prizeLabel,
        //        prizeMessage,
        //        prizeImage = BuildFullImageUrl(baseUrl, prizeImagePath),
        //        coinsEarned = game.PointsAwarded.GetValueOrDefault(),

        //        prizeBalances = new
        //        {
        //            primary = consumeResult.PrimaryPrizeBalCount,
        //            secondary = consumeResult.SecondaryPrizeBalCount,
        //            consolation = consumeResult.ConsolationPrizeBalCount
        //        }
        //    });
        //}

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
            var redeemCode = GenerateRedeemCode();

            var qrCodePath = GenerateSpinGameQRCode(redeemCode);

            var result = await _spinGameRepository.PlaySpinGameAsync(
                request,
                redeemCode,
                qrCodePath);
            if (result.ResultId > 0)
            {
                var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
                var game = await _spinGameRepository.GetSpinGameByIdAsync(request.GameId);
                var section = await _spinGameRepository.GetSectionByIdAsync(result.SectionId);

                int totalCoinsAwarded = 0;
                if (game != null && game.RewardCoins > 0)
                {
                    totalCoinsAwarded += game.RewardCoins;
                }
                if (section != null && section.Points.GetValueOrDefault() > 0)
                {
                    totalCoinsAwarded += section.Points.Value;
                }

                if (totalCoinsAwarded > 0)
                {
                    await _spinGameRepository.AddSpinGameRewardCoinsAsync(
                        request.UserId,
                        totalCoinsAwarded,
                        request.GameId);
                }

                // Backward compatible response + enriched fields for app/store redemption flows.
                return Ok(new
                {
                    result.ResultId,
                    result.ResultMessage,
                    result.GameResultId,
                    result.GameId,
                    result.SectionId,
                    result.RewardValue,
                    result.RedeemCode,
                    result.Status,
                    result.PlayedAt,
                  
                    gameImage = BuildFullImageUrl(baseUrl, game?.GameImage),
                    offerText = section?.PrizeText ?? result.RewardValue,
                    sectionImage = BuildFullImageUrl(baseUrl, section?.SectionImage),

                    spinRedeemCode = result.RedeemCode,

                    spinRedeemQrCode = BuildFullImageUrl(baseUrl, result.QRCodePath),

                    businessLocation = result.BusinessLocation,

                    reward = new
                    {
                        gameId = result.GameId,
                        sectionId = result.SectionId,
                        offerText = section?.PrizeText ?? result.RewardValue,
                        redeemCode = result.RedeemCode,
                        redeemQrCode = BuildFullImageUrl(baseUrl, result.QRCodePath),
                        businessLocation = result.BusinessLocation,
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

        //[HttpPost("GetBrandGameDetails")]
        //public async Task<IActionResult> GetBrandGameDetails([FromBody] GetGameDetails request)
        //{
        //    if (request == null || request.GameId <= 0)
        //    {
        //        return BadRequest(new
        //        {
        //            resultId = 0,
        //            resultMessage = "Valid gameId is required."
        //        });
        //    }

        //    if (request.UserId == Guid.Empty)
        //    {
        //        return BadRequest(new
        //        {
        //            resultId = 0,
        //            resultMessage = "Valid memberId is required."
        //        });
        //    }

        //    var game = await _brandGameRepository.GetBrandGameByIdAsync(request.GameId);

        //    if (game == null)
        //    {
        //        return NotFound(new
        //        {
        //            resultId = 0,
        //            resultMessage = "Game not found."
        //        });
        //    }

        //    var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "").TrimEnd('/');

        //    return Ok(new
        //    {
        //        resultId = 1,
        //        resultMessage = "Game details retrieved successfully.",

        //        gameId = game.BrandGameID,

        //        gameName = game.BrandGameName,

        //        gameTitle = game.BrandGameTitle,

        //        description = game.BrandGameDesc,

        //        conditionsApply = game.ConditionsApply,

        //        destinationUrl = game.DestinationUrl,

        //        onceIn = game.OnceIn,

        //        isReleased = game.IsReleased,

        //        panelCount = game.PanelCount,

        //        panelOpeningLimit = game.PanelOpeningLimit,

        //        chanceCount = game.ChanceCount,

        //        pointsAwarded = game.PointsAwarded,

        //        expiryText = game.ExpiryText,

        //        permitNumber = game.PermitNumber,

        //        classNumber = game.ClassNumber,

        //        formColor = game.FormColor,

        //        textColor = game.TextColor,

        //        promotionalCode = game.PromotionalCode,

        //        startDate = game.DateStart,

        //        endDate = game.DateEnd,

        //        gameImage = BuildFullImageUrl(baseUrl, game.BrandGameImage),

        //        primaryPrizeImage = BuildFullImageUrl(baseUrl, game.PrimaryPrizeImage),

        //        secondaryPrizeImage = BuildFullImageUrl(baseUrl, game.SecondaryPrizeImage),

        //        consolationPrizeImage = BuildFullImageUrl(baseUrl, game.ConsolationPrizeImage),

        //        unsuccessfulImage = BuildFullImageUrl(baseUrl, game.UnSuccessfulImage),

        //        primaryOfferText = game.PrimaryOfferText,

        //        secondaryOfferText = game.OfferText,

        //        primaryWinMessage = game.PrimaryWinMessage,

        //        secondaryWinMessage = game.SecondaryWinMessage,

        //        consolationMessage = game.ConsolationMessage,

        //        prizeBalance = new
        //        {
        //            primary = game.PrimaryPrizeBalCount,
        //            secondary = game.SecondaryPrizeBalCount,
        //            consolation = game.ConsolationPrizeBalCount
        //        }
        //    });
        //}


        [HttpPost("GetBrandGameDetails")]
        public async Task<IActionResult> GetBrandGameDetails(
    [FromBody] GetGameDetails request)
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

            var game = await _brandGameRepository
                .GetBrandGameByIdAsync(request.GameId);

            if (game == null)
            {
                return NotFound(new
                {
                    resultId = 0,
                    resultMessage = "Game not found."
                });
            }

            var baseUrl =
                (_configuration["ApiSettings:BaseUrl"] ?? "")
                .TrimEnd('/');

            // Only used for displaying the scratch card.
            // This DOES NOT assign a prize.
            var prizeImages = new List<string>();

            if (!string.IsNullOrWhiteSpace(game.PrimaryPrizeImage))
                prizeImages.Add(game.PrimaryPrizeImage);

            if (!string.IsNullOrWhiteSpace(game.SecondaryPrizeImage))
                prizeImages.Add(game.SecondaryPrizeImage);

            if (!string.IsNullOrWhiteSpace(game.ConsolationPrizeImage))
                prizeImages.Add(game.ConsolationPrizeImage);

            string? scratchImage = null;

            if (prizeImages.Count > 0)
            {
                scratchImage =
                    prizeImages[Random.Shared.Next(prizeImages.Count)];
            }

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Game details retrieved successfully.",

                gameId = game.BrandGameID,
                gameName = game.BrandGameName,
                gameTitle = game.BrandGameTitle,
                description = game.BrandGameDesc,
                conditionsApply = game.ConditionsApply,

                onceIn = game.OnceIn,
                isReleased = game.IsReleased,

                panelCount = game.PanelCount,
                panelOpeningLimit = game.PanelOpeningLimit,
                chanceCount = game.ChanceCount,

                pointsAwarded = game.PointsAwarded,

                expiryText = game.ExpiryText,

                startDate = game.DateStart,
                endDate = game.DateEnd,

                gameImage = BuildFullImageUrl(
                    baseUrl,
                    game.BrandGameImage),

                scratchImage = BuildFullImageUrl(
                    baseUrl,
                    scratchImage)
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

            // Monitor and update isWinningAttempt every gameplay by counting existing plays
            int totalPlays = await _brandGameRepository.GetTotalPlaysCountAsync(game.BrandGameID);
            var attemptNumber = totalPlays + 1;

            var isWinningAttempt = attemptNumber % onceIn == 0;

            // Retrieve balance counts to check availability of 1st, 2nd, 3rd prizes
            var primaryBalance = game.PrimaryPrizeBalCount.GetValueOrDefault() > 0
                ? game.PrimaryPrizeBalCount.GetValueOrDefault()
                : game.PrimaryPrizeCount.GetValueOrDefault();

            var secondaryBalance = game.SecondaryPrizeBalCount.GetValueOrDefault() > 0
                ? game.SecondaryPrizeBalCount.GetValueOrDefault()
                : game.SecondaryPrizeCount.GetValueOrDefault();

            var consolationBalance = game.ConsolationPrizeBalCount.GetValueOrDefault() > 0
                ? game.ConsolationPrizeBalCount.GetValueOrDefault()
                : game.ConsolationPrizeCount.GetValueOrDefault();

            // Default fallback is "Almost There" (Consolation, 5 IC)
            string swFinalPrizeType = "AlmostThere";
            string swPrizeLabel = "😮 Almost There";
            string swPrizeMessage = "Almost There! Earned 5 IC";
            string swPrizeImage = "Images/brandgames/rewards/almost_there.png";

            // Roll a number 1-100 to determine reward based on probability
            int roll = Random.Shared.Next(1, 101); // 1 to 100

            // Probability brackets:
            // 1-15 (15%): 1st Prize (PrimaryPrize)
            // 16-23 (8%): 2nd Prize (SecondaryPrize)
            // 24-28 (5%): 3rd Prize (ConsolationPrize)
            // 29 (1%): 200 IndoCoins (Jackpot)
            // 30-32 (3%): 100 IndoCoins (Rare)
            // 33-40 (8%): 50 IndoCoins (Medium)
            // 41-60 (20%): 25 IndoCoins (Frequent)
            // 61-100 (40%): Almost There (Consolation, 5 IC)

            if (roll <= 15)
            {
                // 1st Prize
                if (isReleased && isWinningAttempt && primaryBalance > 0)
                {
                    swFinalPrizeType = "PrimaryPrize";
                    swPrizeLabel = game.PrimaryOfferText ?? "1st Prize";
                    swPrizeMessage = game.PrimaryWinMessage ?? "You won the 1st Prize!";
                    swPrizeImage = game.PrimaryPrizeImage ?? game.BrandGameImage;
                }
            }
            else if (roll <= 23)
            {
                // 2nd Prize
                if (isReleased && isWinningAttempt && secondaryBalance > 0)
                {
                    swFinalPrizeType = "SecondaryPrize";
                    swPrizeLabel = game.OfferText ?? "2nd Prize";
                    swPrizeMessage = game.SecondaryWinMessage ?? "You won the 2nd Prize!";
                    swPrizeImage = game.SecondaryPrizeImage ?? game.BrandGameImage;
                }
            }
            else if (roll <= 28)
            {
                // 3rd Prize
                if (isReleased && isWinningAttempt && consolationBalance > 0)
                {
                    swFinalPrizeType = "ConsolationPrize";
                    swPrizeLabel = game.OfferText ?? "3rd Prize";
                    swPrizeMessage = game.ConsolationMessage ?? "You won the 3rd Prize!";
                    swPrizeImage = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage;
                }
            }
            else if (roll <= 29)
            {
                swFinalPrizeType = "200IndoCoins";
                swPrizeLabel = "🪙 200 IndoCoins";
                swPrizeMessage = "Jackpot! You won 200 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/200_indocoins.png";
            }
            else if (roll <= 32)
            {
                swFinalPrizeType = "100IndoCoins";
                swPrizeLabel = "🪙 100 IndoCoins";
                swPrizeMessage = "Awesome! You won 100 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/100_indocoins.png";
            }
            else if (roll <= 40)
            {
                swFinalPrizeType = "50IndoCoins";
                swPrizeLabel = "🪙 50 IndoCoins";
                swPrizeMessage = "Great! You won 50 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/50_indocoins.png";
            }
            else if (roll <= 60)
            {
                swFinalPrizeType = "25IndoCoins";
                swPrizeLabel = "🪙 25 IndoCoins";
                swPrizeMessage = "Nice! You won 25 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/25_indocoins.png";
            }

            var verificationToken = GenerateVerificationToken(game.BrandGameID, request.UserId, swFinalPrizeType, attemptNumber);

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Prize revealed successfully.",
                gameId = game.BrandGameID,
                memberId = request.UserId,
                prizeType = swFinalPrizeType,
                prizeLabel = swPrizeLabel,
                prizeMessage = swPrizeMessage,
                prizeImage = BuildFullImageUrl(baseUrl, swPrizeImage),
                attemptNumber = attemptNumber,
                verificationToken = verificationToken
            });
        }

        [HttpPost("RedeemPrize")]
        public async Task<IActionResult> RedeemPrize([FromBody] RedeemPrizeRequest request)
        {
            if (request == null || request.GameId <= 0 || request.UserId == Guid.Empty || string.IsNullOrEmpty(request.PrizeType) || string.IsNullOrEmpty(request.VerificationToken))
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Valid GameId, UserId, PrizeType, and VerificationToken are required."
                });
            }

            // Verify verification token signature
            var expectedToken = GenerateVerificationToken(request.GameId, request.UserId, request.PrizeType, request.AttemptNumber);
            if (request.VerificationToken != expectedToken)
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Invalid verification token."
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

            string swFinalPrizeType = request.PrizeType;
            string swPrizeLabel = "";
            string swPrizeMessage = "";
            string swPrizeImage = "";
            int coinsEarned = 0;
            bool swIsWinner = false;

            string? swRedeemCode = null;
            string? swQrCodePath = null;

            if (swFinalPrizeType == "PrimaryPrize")
            {
                var consumeResult = await _brandGameRepository.TryConsumePrizeAsync(game.BrandGameID, "PrimaryPrize");
                if (consumeResult.IsConsumed)
                {
                    swPrizeLabel = game.PrimaryOfferText ?? "1st Prize";
                    swPrizeMessage = game.PrimaryWinMessage ?? "You won the 1st Prize!";
                    swPrizeImage = game.PrimaryPrizeImage ?? game.BrandGameImage;
                    swIsWinner = true;
                    // Generate redeem code and QR code only for physical prizes
                    swRedeemCode = GenerateRedeemCode();
                    swQrCodePath = GenerateQRCode(swRedeemCode);
                }
                else
                {
                    // Fallback to Almost There (Consolation) if out of stock
                    swFinalPrizeType = "AlmostThere";
                }
            }
            else if (swFinalPrizeType == "SecondaryPrize")
            {
                var consumeResult = await _brandGameRepository.TryConsumePrizeAsync(game.BrandGameID, "SecondaryPrize");
                if (consumeResult.IsConsumed)
                {
                    swPrizeLabel = game.OfferText ?? "2nd Prize";
                    swPrizeMessage = game.SecondaryWinMessage ?? "You won the 2nd Prize!";
                    swPrizeImage = game.SecondaryPrizeImage ?? game.BrandGameImage;
                    swIsWinner = true;
                    // Generate redeem code and QR code only for physical prizes
                    swRedeemCode = GenerateRedeemCode();
                    swQrCodePath = GenerateQRCode(swRedeemCode);
                }
                else
                {
                    swFinalPrizeType = "AlmostThere";
                }
            }
            else if (swFinalPrizeType == "ConsolationPrize")
            {
                var consumeResult = await _brandGameRepository.TryConsumePrizeAsync(game.BrandGameID, "ConsolationPrize");
                if (consumeResult.IsConsumed)
                {
                    swPrizeLabel = game.OfferText ?? "3rd Prize";
                    swPrizeMessage = game.ConsolationMessage ?? "You won the 3rd Prize!";
                    swPrizeImage = game.ConsolationPrizeImage ?? game.BrandGameImage;
                    swIsWinner = true;
                    // Generate redeem code and QR code only for physical prizes
                    swRedeemCode = GenerateRedeemCode();
                    swQrCodePath = GenerateQRCode(swRedeemCode);
                }
                else
                {
                    swFinalPrizeType = "AlmostThere";
                }
            }

            // If we fell back to or rolled Almost There/Coins
            if (swFinalPrizeType == "AlmostThere")
            {
                swPrizeLabel = "😮 Almost There";
                swPrizeMessage = "Almost There! Earned 5 IC";
                swPrizeImage = "Images/brandgames/rewards/almost_there.png";
                coinsEarned = 5;
                swIsWinner = false;

                // Track play in general consolation count
                await _brandGameRepository.TryConsumePrizeAsync(game.BrandGameID, "ConsolationPrize");
            }
            else if (swFinalPrizeType == "200IndoCoins")
            {
                swPrizeLabel = "🪙 200 IndoCoins";
                swPrizeMessage = "Jackpot! You won 200 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/200_indocoins.png";
                coinsEarned = 200;
                swIsWinner = true;
            }
            else if (swFinalPrizeType == "100IndoCoins")
            {
                swPrizeLabel = "🪙 100 IndoCoins";
                swPrizeMessage = "Awesome! You won 100 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/100_indocoins.png";
                coinsEarned = 100;
                swIsWinner = true;
            }
            else if (swFinalPrizeType == "50IndoCoins")
            {
                swPrizeLabel = "🪙 50 IndoCoins";
                swPrizeMessage = "Great! You won 50 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/50_indocoins.png";
                coinsEarned = 50;
                swIsWinner = true;
            }
            else if (swFinalPrizeType == "25IndoCoins")
            {
                swPrizeLabel = "🪙 25 IndoCoins";
                swPrizeMessage = "Nice! You won 25 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/25_indocoins.png";
                coinsEarned = 25;
                swIsWinner = true;
            }

            // Save permanent gameplay to history
            await _brandGameRepository.TrackGameplayAsync(
                game.BrandGameID,
                request.UserId,
                swFinalPrizeType,
                swIsWinner,
                request.AttemptNumber,
                swRedeemCode,
                swQrCodePath);

            // Add coins to wallet if won
            if (coinsEarned > 0)
            {
                await _brandGameRepository.AddRewardCoinsAsync(
                    request.UserId,
                    coinsEarned,
                    game.BrandGameID);
            }

            // Fetch fresh balance counts
            var freshGame = await _brandGameRepository.GetBrandGameByIdAsync(game.BrandGameID);

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Prize redeemed successfully.",
                gameId = game.BrandGameID,
                memberId = request.UserId,
                isWinner = swIsWinner,
                prizeType = swFinalPrizeType,
                prizeLabel = swPrizeLabel,
                prizeMessage = swPrizeMessage,
                prizeImage = BuildFullImageUrl(baseUrl, swPrizeImage),
                coinsEarned = coinsEarned,
                redeemCode = swRedeemCode,
                redeemQrCode = BuildFullImageUrl(baseUrl, swQrCodePath),
                businessLocation = game.BusinessLocation,
                prizeBalances = new
                {
                    primary = freshGame?.PrimaryPrizeBalCount.GetValueOrDefault() ?? 0,
                    secondary = freshGame?.SecondaryPrizeBalCount.GetValueOrDefault() ?? 0,
                    consolation = freshGame?.ConsolationPrizeBalCount.GetValueOrDefault() ?? 0
                }
            });
        }

        private string GenerateVerificationToken(int gameId, Guid userId, string prizeType, int attemptNumber)
        {
            var salt = "CommUnityApp_ScratchWin_Salt_2026";
            var raw = $"{gameId}:{userId}:{prizeType}:{attemptNumber}:{salt}";
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
                "brandgames",
                "QR");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = $"{redeemCode}.png";

            string fullPath = Path.Combine(folder, fileName);


            string qrContent = redeemCode;

            using var generator = new QRCodeGenerator();

            using var data = generator.CreateQrCode(
                qrContent,
                QRCodeGenerator.ECCLevel.Q);

            var qr = new PngByteQRCode(data);

            byte[] bytes = qr.GetGraphic(20);

            System.IO.File.WriteAllBytes(fullPath, bytes);

            return $"Images/brandgames/QR/{fileName}";
        }


        private string GenerateSpinGameQRCode(string redeemCode)
        {
            string folder = Path.Combine(
                _environment.WebRootPath,
                "Images",
                "spingames",
                "QR");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = $"{redeemCode}.png";

            string fullPath = Path.Combine(folder, fileName);

            using var generator = new QRCodeGenerator();

            using var data = generator.CreateQrCode(
                redeemCode,
                QRCodeGenerator.ECCLevel.Q);

            var qr = new PngByteQRCode(data);

            byte[] bytes = qr.GetGraphic(20);

            System.IO.File.WriteAllBytes(fullPath, bytes);

            return $"Images/spingames/QR/{fileName}";
        }
    }
}
