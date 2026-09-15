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

            SpinSectionRequest? requestedSection = null;
            if (request.SectionId > 0)
            {
                requestedSection = await _spinGameRepository.GetSectionByIdAsync(request.SectionId);
            }

            int preCoins = requestedSection?.Points.GetValueOrDefault() ?? 0;
            if (preCoins <= 0 && !string.IsNullOrWhiteSpace(requestedSection?.PrizeText))
            {
                var match = System.Text.RegularExpressions.Regex.Match(requestedSection.PrizeText, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int parsed))
                {
                    preCoins = parsed;
                }
            }

            bool isCoinReward = preCoins > 0 ||
                                (requestedSection != null && !string.IsNullOrWhiteSpace(requestedSection.PrizeText) &&
                                 (requestedSection.PrizeText.Contains("point", StringComparison.OrdinalIgnoreCase) ||
                                  requestedSection.PrizeText.Contains("coin", StringComparison.OrdinalIgnoreCase) ||
                                  requestedSection.PrizeText.Contains("ic", StringComparison.OrdinalIgnoreCase) ||
                                  requestedSection.PrizeText.Contains("indocoin", StringComparison.OrdinalIgnoreCase)));

            bool isLosingSection = requestedSection != null &&
                                   (string.Equals(requestedSection.PrizeText, "Better Luck Next Time", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(requestedSection.PrizeText, "Try Again :(", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(requestedSection.PrizeText, "Spin Again", StringComparison.OrdinalIgnoreCase));

            bool isRedeemable = (requestedSection == null) || (!isCoinReward && !isLosingSection &&
                                (requestedSection.PromotionId.GetValueOrDefault() > 0 ||
                                 (!string.IsNullOrWhiteSpace(requestedSection.PrizeText) && !requestedSection.PrizeText.Equals("None", StringComparison.OrdinalIgnoreCase))));

            string? redeemCode = null;
            string? qrCodePath = null;

            if (isRedeemable)
            {
                redeemCode = GenerateRedeemCode();
                qrCodePath = GenerateSpinGameQRCode(redeemCode);
            }

            var result = await _spinGameRepository.PlaySpinGameAsync(
                request,
                redeemCode,
                qrCodePath);

            if (result.ResultId > 0)
            {
                var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
                var game = await _spinGameRepository.GetSpinGameByIdAsync(request.GameId);
                var section = await _spinGameRepository.GetSectionByIdAsync(result.SectionId) ?? requestedSection;

                // Robust coin parsing from section.Points or section.PrizeText
                int coinsFromSection = section?.Points.GetValueOrDefault() ?? 0;
                if (coinsFromSection <= 0 && !string.IsNullOrWhiteSpace(section?.PrizeText))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(section.PrizeText, @"\d+");
                    if (match.Success && int.TryParse(match.Value, out int parsedCoins))
                    {
                        coinsFromSection = parsedCoins;
                    }
                }

                isCoinReward = coinsFromSection > 0 ||
                               (!string.IsNullOrWhiteSpace(section?.PrizeText) &&
                                (section.PrizeText.Contains("point", StringComparison.OrdinalIgnoreCase) ||
                                 section.PrizeText.Contains("coin", StringComparison.OrdinalIgnoreCase) ||
                                 section.PrizeText.Contains("ic", StringComparison.OrdinalIgnoreCase) ||
                                 section.PrizeText.Contains("indocoin", StringComparison.OrdinalIgnoreCase)));

                isLosingSection = section != null &&
                                  (string.Equals(section.PrizeText, "Better Luck Next Time", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(section.PrizeText, "Try Again :(", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(section.PrizeText, "Spin Again", StringComparison.OrdinalIgnoreCase));

                isRedeemable = !isCoinReward && !isLosingSection &&
                               (section != null && (section.PromotionId.GetValueOrDefault() > 0 ||
                                (!string.IsNullOrWhiteSpace(section.PrizeText) && !section.PrizeText.Equals("None", StringComparison.OrdinalIgnoreCase))));

                int totalCoinsAwarded = 0;
                if (game != null && game.RewardCoins > 0)
                {
                    totalCoinsAwarded += game.RewardCoins;
                }
                totalCoinsAwarded += coinsFromSection;

                if (totalCoinsAwarded > 0)
                {
                    string spinNotes = $"Coins earned from Spin the Wheel - {section?.PrizeText ?? $"{totalCoinsAwarded} Coins"}";
                    await _spinGameRepository.AddSpinGameRewardCoinsAsync(
                        request.UserId,
                        totalCoinsAwarded,
                        request.GameId,
                        spinNotes);
                }

                // Resolve relevant image for section (coins, losing, or voucher)
                string? resolvedSectionImage = section?.SectionImage;
                if (string.IsNullOrWhiteSpace(resolvedSectionImage))
                {
                    int pts = coinsFromSection > 0 ? coinsFromSection : (section?.Points.GetValueOrDefault() ?? 0);
                    if (pts >= 200 || (section?.PrizeText?.Contains("200") ?? false))
                        resolvedSectionImage = "Images/brandgames/rewards/200_indocoins.png";
                    else if (pts >= 100 || (section?.PrizeText?.Contains("100") ?? false))
                        resolvedSectionImage = "Images/brandgames/rewards/100_indocoins.png";
                    else if (pts >= 50 || (section?.PrizeText?.Contains("50") ?? false))
                        resolvedSectionImage = "Images/brandgames/rewards/50_indocoins.png";
                    else if (pts >= 25 || (section?.PrizeText?.Contains("25") ?? false))
                        resolvedSectionImage = "Images/brandgames/rewards/25_indocoins.png";
                    else if (isCoinReward)
                        resolvedSectionImage = "Images/brandgames/rewards/50_indocoins.png";
                    else if (isLosingSection)
                        resolvedSectionImage = "Images/brandgames/rewards/almost_there.png";
                    else
                        resolvedSectionImage = game?.GameImage ?? "Images/brandgames/rewards/mystery_gift.png";
                }

                string? effectiveRedeemCode = isRedeemable ? result.RedeemCode : null;
                string? effectiveQrCodePath = isRedeemable ? result.QRCodePath : null;

                // Backward compatible response + enriched fields for app/store redemption flows.
                return Ok(new
                {
                    result.ResultId,
                    result.ResultMessage,
                    result.GameResultId,
                    result.GameId,
                    result.SectionId,
                    sectionNumber = section?.SectionNumber ?? 0,
                    sectionIndex = (section != null) ? (section.SectionNumber - 1) : -1,
                    result.RewardValue,
                    redeemCode = effectiveRedeemCode,
                    result.Status,
                    result.PlayedAt,
                    isCoinReward = isCoinReward,
                    isRedeemable = isRedeemable,
                    coinsEarned = totalCoinsAwarded,
                  
                    gameImage = BuildFullImageUrl(baseUrl, game?.GameImage ?? resolvedSectionImage),
                    offerText = section?.PrizeText ?? result.RewardValue,
                    sectionImage = BuildFullImageUrl(baseUrl, resolvedSectionImage),

                    spinRedeemCode = effectiveRedeemCode,

                    spinRedeemQrCode = isRedeemable ? BuildFullImageUrl(baseUrl, effectiveQrCodePath) : null,

                    businessLocation = result.BusinessLocation,

                    reward = new
                    {
                        gameId = result.GameId,
                        sectionId = result.SectionId,
                        sectionNumber = section?.SectionNumber ?? 0,
                        sectionIndex = (section != null) ? (section.SectionNumber - 1) : -1,
                        offerText = section?.PrizeText ?? result.RewardValue,
                        redeemCode = effectiveRedeemCode,
                        redeemQrCode = isRedeemable ? BuildFullImageUrl(baseUrl, effectiveQrCodePath) : null,
                        businessLocation = result.BusinessLocation,
                        gameImage = BuildFullImageUrl(baseUrl, game?.GameImage ?? resolvedSectionImage),
                        sectionImage = BuildFullImageUrl(baseUrl, resolvedSectionImage),
                        isCoinReward = isCoinReward,
                        isRedeemable = isRedeemable,
                        coinsEarned = totalCoinsAwarded
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

            // Provide fallback cascade for all prize images so they are never null or broken
            const string defaultMysteryGift = "Images/brandgames/rewards/mystery_gift.png";
            const string defaultCoinsImage = "Images/brandgames/rewards/50_indocoins.png";
            const string defaultConsolation = "Images/brandgames/rewards/almost_there.png";

            string primaryImage = !string.IsNullOrWhiteSpace(game.PrimaryPrizeImage)
                ? game.PrimaryPrizeImage
                : (!string.IsNullOrWhiteSpace(game.BrandGameImage) ? game.BrandGameImage : defaultMysteryGift);

            string secondaryImage = !string.IsNullOrWhiteSpace(game.SecondaryPrizeImage)
                ? game.SecondaryPrizeImage
                : (!string.IsNullOrWhiteSpace(game.BrandGameImage) ? game.BrandGameImage : defaultCoinsImage);

            string consolationImage = !string.IsNullOrWhiteSpace(game.ConsolationPrizeImage)
                ? game.ConsolationPrizeImage
                : (!string.IsNullOrWhiteSpace(game.UnSuccessfulImage) ? game.UnSuccessfulImage : (!string.IsNullOrWhiteSpace(game.BrandGameImage) ? game.BrandGameImage : defaultConsolation));

            // Only used for displaying the scratch card inside image underneath the foil.
            var prizeImages = new List<string>();
            if (!string.IsNullOrWhiteSpace(game.PrimaryPrizeImage)) prizeImages.Add(game.PrimaryPrizeImage);
            if (!string.IsNullOrWhiteSpace(game.SecondaryPrizeImage)) prizeImages.Add(game.SecondaryPrizeImage);
            if (!string.IsNullOrWhiteSpace(game.ConsolationPrizeImage)) prizeImages.Add(game.ConsolationPrizeImage);
            if (!string.IsNullOrWhiteSpace(game.BrandGameImage)) prizeImages.Add(game.BrandGameImage);

            string scratchImage = prizeImages.Count > 0
                ? prizeImages[Random.Shared.Next(prizeImages.Count)]
                : primaryImage;

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

                startDate = game.DateStart,
                endDate = game.DateEnd,

                // Comprehensive image mappings ensuring inside scratch image is always displayed
                gameImage = BuildFullImageUrl(baseUrl, game.BrandGameImage ?? primaryImage),
                scratchImage = BuildFullImageUrl(baseUrl, scratchImage),
                insideImage = BuildFullImageUrl(baseUrl, scratchImage),
                prizeImage = BuildFullImageUrl(baseUrl, primaryImage),

                primaryPrizeImage = BuildFullImageUrl(baseUrl, primaryImage),
                secondaryPrizeImage = BuildFullImageUrl(baseUrl, secondaryImage),
                consolationPrizeImage = BuildFullImageUrl(baseUrl, consolationImage),
                unsuccessfulImage = BuildFullImageUrl(baseUrl, consolationImage),

                primaryOfferText = game.PrimaryOfferText ?? "1st Prize",
                secondaryOfferText = game.OfferText ?? "2nd Prize",
                primaryWinMessage = game.PrimaryWinMessage,
                secondaryWinMessage = game.SecondaryWinMessage,
                consolationMessage = game.ConsolationMessage,
                businessLocation = game.BusinessLocation,
                prizeBalance = new
                {
                    primary = game.PrimaryPrizeBalCount.GetValueOrDefault(),
                    secondary = game.SecondaryPrizeBalCount.GetValueOrDefault(),
                    consolation = game.ConsolationPrizeBalCount.GetValueOrDefault()
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

            // Check user's last prize type to prevent repeating the same reward image consecutively
            string? lastPrizeType = await _brandGameRepository.GetLastUserPrizeTypeAsync(game.BrandGameID, request.UserId);

            // Default fallback is 25 IndoCoins instead of repeating AlmostThere
            string swFinalPrizeType = "25IndoCoins";
            string swPrizeLabel = "🪙 25 IndoCoins";
            string swPrizeMessage = "Nice! You won 25 IndoCoins!";
            string swPrizeImage = "Images/brandgames/rewards/25_indocoins.png";

            // Roll a number 1-100 to determine reward based on probability
            int roll = Random.Shared.Next(1, 101); // 1 to 100

            if (roll <= 15 && isReleased && isWinningAttempt && primaryBalance > 0)
            {
                swFinalPrizeType = "PrimaryPrize";
                swPrizeLabel = game.PrimaryOfferText ?? "1st Prize";
                swPrizeMessage = game.PrimaryWinMessage ?? "You won the 1st Prize!";
                swPrizeImage = game.PrimaryPrizeImage ?? game.BrandGameImage ?? "Images/brandgames/rewards/mystery_gift.png";
            }
            else if (roll <= 23 && isReleased && isWinningAttempt && secondaryBalance > 0)
            {
                swFinalPrizeType = "SecondaryPrize";
                swPrizeLabel = game.OfferText ?? "2nd Prize";
                swPrizeMessage = game.SecondaryWinMessage ?? "You won the 2nd Prize!";
                swPrizeImage = game.SecondaryPrizeImage ?? game.BrandGameImage ?? "Images/brandgames/rewards/50_indocoins.png";
            }
            else if (roll <= 28 && isReleased && isWinningAttempt && consolationBalance > 0)
            {
                swFinalPrizeType = "ConsolationPrize";
                swPrizeLabel = game.OfferText ?? "3rd Prize";
                swPrizeMessage = game.ConsolationMessage ?? "You won the 3rd Prize!";
                swPrizeImage = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage ?? "Images/brandgames/rewards/almost_there.png";
            }
            else
            {
                // Diverse non-major rewards pool
                var coinPool = new List<(string Type, string Label, string Message, string Image)>
                {
                    ("25IndoCoins", "🪙 25 IndoCoins", "Nice! You won 25 IndoCoins!", "Images/brandgames/rewards/25_indocoins.png"),
                    ("50IndoCoins", "🪙 50 IndoCoins", "Great! You won 50 IndoCoins!", "Images/brandgames/rewards/50_indocoins.png"),
                    ("100IndoCoins", "🪙 100 IndoCoins", "Awesome! You won 100 IndoCoins!", "Images/brandgames/rewards/100_indocoins.png"),
                    ("10IndoCoins", "🪙 10 IndoCoins", "Good luck! You won 10 IndoCoins!", "Images/brandgames/rewards/mystery_gift.png"),
                    ("200IndoCoins", "🪙 200 IndoCoins", "Jackpot! You won 200 IndoCoins!", "Images/brandgames/rewards/200_indocoins.png"),
                    ("AlmostThere", "😮 Almost There", "Almost There! Earned 5 IC", "Images/brandgames/rewards/almost_there.png")
                };

                // Filter out previous attempt's prize type to guarantee NO consecutive identical reward images
                var eligiblePool = (!string.IsNullOrEmpty(lastPrizeType))
                    ? coinPool.Where(p => p.Type != lastPrizeType).ToList()
                    : coinPool;

                if (!eligiblePool.Any())
                {
                    eligiblePool = coinPool;
                }

                // Weighted selection from eligible pool
                int coinRoll = Random.Shared.Next(1, 101);
                var selectedReward = coinRoll switch
                {
                    <= 35 => eligiblePool.FirstOrDefault(p => p.Type == "25IndoCoins"),
                    <= 60 => eligiblePool.FirstOrDefault(p => p.Type == "50IndoCoins"),
                    <= 75 => eligiblePool.FirstOrDefault(p => p.Type == "10IndoCoins"),
                    <= 87 => eligiblePool.FirstOrDefault(p => p.Type == "100IndoCoins"),
                    <= 95 => eligiblePool.FirstOrDefault(p => p.Type == "200IndoCoins"),
                    _     => eligiblePool.FirstOrDefault(p => p.Type == "AlmostThere")
                };

                if (string.IsNullOrEmpty(selectedReward.Type))
                {
                    selectedReward = eligiblePool[Random.Shared.Next(eligiblePool.Count)];
                }

                swFinalPrizeType = selectedReward.Type;
                swPrizeLabel = selectedReward.Label;
                swPrizeMessage = selectedReward.Message;
                swPrizeImage = selectedReward.Image;
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
                    swPrizeImage = game.PrimaryPrizeImage ?? game.BrandGameImage ?? "Images/brandgames/rewards/mystery_gift.png";
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
                    swPrizeImage = game.SecondaryPrizeImage ?? game.BrandGameImage ?? "Images/brandgames/rewards/50_indocoins.png";
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
                    swPrizeImage = game.ConsolationPrizeImage ?? game.UnSuccessfulImage ?? game.BrandGameImage ?? "Images/brandgames/rewards/almost_there.png";
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
            else if (swFinalPrizeType == "10IndoCoins")
            {
                swPrizeLabel = "🪙 10 IndoCoins";
                swPrizeMessage = "Good luck! You won 10 IndoCoins!";
                swPrizeImage = "Images/brandgames/rewards/mystery_gift.png";
                coinsEarned = 10;
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
                string brandGameNote = $"Coins earned from Scratch & Win - {swPrizeLabel}";
                await _brandGameRepository.AddRewardCoinsAsync(
                    request.UserId,
                    coinsEarned,
                    game.BrandGameID,
                    brandGameNote);
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
