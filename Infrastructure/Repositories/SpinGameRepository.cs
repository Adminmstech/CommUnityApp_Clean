using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class SpinGameRepository : ISpinGameRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly IDapperWrapper _dapper; // New dependency
        private readonly IConfiguration? _configuration;


        public SpinGameRepository(Func<IDbConnection> connectionFactory, IDapperWrapper dapper, IConfiguration? configuration = null) // Modified constructor
        {
            _connectionFactory = connectionFactory;
            _dapper = dapper; // Initialize new dependency
            _configuration = configuration;
        }

        private IDbConnection Connection => _connectionFactory();

        public async Task<BaseResponse> AddUpdateSpinGameAsync(AddUpdateSpinGameRequest model)
        {
            using var con = Connection;
            con.Open();
            using var transaction = con.BeginTransaction();

            try
            {
                // 1. Add/Update SpinGame
                object gameParameters;
                if (model.GameId == 0)
                {
                    gameParameters = new
                    {
                        model.BusinessId,
                        model.GameName,
                        model.Description,
                        model.GameImage,
                        model.CreatedByAdminId,
                        model.IsActive,
                        model.BusinessLocation
                    };
                }
                else
                {
                    gameParameters = new
                    {
                        model.GameId,
                        model.BusinessId,
                        model.GameName,
                        model.Description,
                        model.GameImage,
                        ConfigId = model.ConfigId, // Use existing ConfigId during update
                        model.CreatedByAdminId,
                        model.BusinessLocation,
                        model.IsActive
                    };
                }

                var gameResult = await _dapper.QueryFirstOrDefaultAsync<BaseResponse>(
                    con,
                    model.GameId == 0 ? "sp_AddSpinGame" : "sp_UpdateSpinGame",
                    gameParameters,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure
                );

                if (gameResult == null || gameResult.ResultId <= 0)
                {
                    transaction.Rollback();
                    return gameResult ?? new BaseResponse { ResultId = 0, ResultMessage = "Failed to add/update SpinGame." };
                }

                int gameId = gameResult.ResultId; // This is the new or existing GameId

                // 2. Handle SpinGameConfiguration
                if (model.Configs != null && model.Configs.Any())
                {
                    var configModel = model.Configs.First(); // Assuming one config per game
                    configModel.ConfigId = model.ConfigId; // Use existing ConfigId if updating

                    object configParameters;
                    if (configModel.ConfigId == 0)
                    {
                        configParameters = new
                        {
                            configModel.MaxSpinsPerDay,
                            configModel.NumberOfSections,
                            configModel.GameStartDate,
                            configModel.GameEndDate,
                            configModel.IsActive
                        };
                    }
                    else
                    {
                        configParameters = new
                        {
                            configModel.ConfigId,
                            configModel.MaxSpinsPerDay,
                            configModel.NumberOfSections,
                            configModel.GameStartDate,
                            configModel.GameEndDate,
                            configModel.IsActive
                        };
                    }

                    var configResult = await _dapper.QueryFirstOrDefaultAsync<BaseResponse>(
                        con,
                        configModel.ConfigId == 0 ? "sp_AddSpinGameConfig" : "sp_UpdateSpinGameConfig",
                        configParameters,
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure
                    );

                    if (configResult == null || configResult.ResultId <= 0)
                    {
                        transaction.Rollback();
                        return configResult ?? new BaseResponse { ResultId = 0, ResultMessage = "Failed to add/update SpinGame Configuration." };
                    }

                    int configId = configResult.ResultId;

                    // Update the SpinGame with the correct ConfigId if it was a new game or config
                    if (model.GameId == 0 || model.ConfigId == 0)
                    {
                        var updateGameConfigParameters = new
                        {
                            GameId = gameId,
                            ConfigId = configId
                        };
                        await _dapper.ExecuteAsync(
                            con,
                            "UPDATE SpinGame SET ConfigId = @ConfigId WHERE GameId = @GameId",
                            updateGameConfigParameters,
                            transaction: transaction
                        );
                    }
                }

                // 3. Handle SpinSections
                if (model.Sections != null && model.Sections.Any())
                {
                    // For updates, we might need to delete existing sections first or compare
                    // For simplicity, assuming add/update for now.
                    // A more robust solution would involve comparing existing sections and deleting removed ones.

                    foreach (var sectionModel in model.Sections)
                    {
                        sectionModel.GameId = gameId; // Link section to the current game

                        object sectionParameters;
                        if (sectionModel.SectionId == 0)
                        {
                            sectionParameters = new
                            {
                                sectionModel.GameId,
                                sectionModel.SectionNumber,
                                sectionModel.SectionImage,
                                sectionModel.PrizeText,
                                sectionModel.Color,
                                sectionModel.Probability,
                                sectionModel.WinRangeMin,
                                sectionModel.WinRangeMax,
                                sectionModel.TotalStock,
                                AvailableStock = sectionModel.AvailableStock ?? sectionModel.TotalStock
                                //IsActive = true
                            };
                        }
                        else
                        {
                            sectionParameters = new
                            {
                                sectionModel.SectionId,
                                sectionModel.GameId,
                                sectionModel.SectionNumber,
                                sectionModel.SectionImage,
                                sectionModel.PrizeText,
                                sectionModel.Color,
                                sectionModel.Probability,
                                sectionModel.WinRangeMin,
                                sectionModel.WinRangeMax,
                                sectionModel.TotalStock,
                                AvailableStock = sectionModel.AvailableStock ?? sectionModel.TotalStock,
                                IsActive = true
                            };
                        }

                        var sectionResult = await _dapper.QueryFirstOrDefaultAsync<BaseResponse>(
                            con,
                            sectionModel.SectionId == 0 ? "sp_AddSpinSection" : "sp_UpdateSpinSection",
                            sectionParameters,
                            transaction: transaction,
                            commandType: CommandType.StoredProcedure
                        );

                        if (sectionResult == null || sectionResult.ResultId <= 0)
                        {
                            transaction.Rollback();
                            return sectionResult ?? new BaseResponse { ResultId = 0, ResultMessage = $"Failed to add/update SpinSection {sectionModel.SectionNumber}." };
                        }
                    }
                }

                transaction.Commit();
                return gameResult; // Return the result of the main game operation
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // Log the exception (not implemented here for brevity)
                return new BaseResponse { ResultId = 0, ResultMessage = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<SpinGameConfigRequest?> GetConfigByIdAsync(int configId)
        {
            using var con = Connection;
            return await _dapper.QueryFirstOrDefaultAsync<SpinGameConfigRequest>(
                con,
                "SELECT ConfigId, MaxSpinsPerDay, NumberOfSections, GameStartDate, GameEndDate, IsActive FROM SpinGameConfiguration WHERE ConfigId = @ConfigId",
                new { ConfigId = configId }
            );
        }

        public async Task<IEnumerable<SpinSectionRequest>> GetSectionsByGameIdAsync(int gameId)
        {
            using var con = Connection;
            return await _dapper.QueryAsync<SpinSectionRequest>(
                con,
                @"SELECT SectionId, GameId, SectionNumber, Points, PromotionId, PrizeText, Color, SectionImage, Probability, WinRangeMin, WinRangeMax, TotalStock, AvailableStock 
                  FROM SpinSection WHERE GameId = @GameId ORDER BY SectionNumber",
                new { GameId = gameId }
            );
        }

        public async Task<SpinSectionRequest?> GetSectionByIdAsync(int sectionId)
        {
            using var con = Connection;
            return await _dapper.QueryFirstOrDefaultAsync<SpinSectionRequest>(
                con,
                @"SELECT SectionId, GameId, SectionNumber, Points, PromotionId, PrizeText, Color, SectionImage, Probability, WinRangeMin, WinRangeMax, TotalStock, AvailableStock
                  FROM SpinSection WHERE SectionId = @SectionId",
                new { SectionId = sectionId }
            );
        }

        public async Task<BaseResponse> AddUpdateConfigAsync(SpinGameConfigRequest model)
        {
            using var con = Connection;
            object parameters;

            if (model.ConfigId == 0)
            {
                parameters = new
                {
                    model.MaxSpinsPerDay,
                    model.NumberOfSections,
                    model.GameStartDate,
                    model.GameEndDate,
                    model.IsActive
                };
            }
            else
            {
                parameters = new
                {
                    model.ConfigId,
                    model.MaxSpinsPerDay,
                    model.NumberOfSections,
                    model.GameStartDate,
                    model.GameEndDate,
                    model.IsActive
                };
            }

            var result = await con.QueryFirstOrDefaultAsync<BaseResponse>(
                model.ConfigId == 0 ? "sp_AddSpinGameConfig" : "sp_UpdateSpinGameConfig",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new BaseResponse { ResultId = 0, ResultMessage = "Operation failed." };
        }

        public async Task<BaseResponse> AddUpdateSectionAsync(SpinSectionRequest model)
        {
            using var con = Connection;
            object parameters;
            if (model.SectionId == 0)
            {
                parameters = new
                {
                    model.GameId,
                    model.SectionNumber,
                    model.SectionImage,
                    model.PrizeText,
                    model.Color,
                    model.Probability,
                    model.WinRangeMin,
                    model.WinRangeMax,
                    model.TotalStock,
                    AvailableStock = model.AvailableStock ?? model.TotalStock
                };
            }
            else
            {
                parameters = new
                {
                    model.SectionId,
                    model.GameId,
                    model.SectionNumber,
                    model.SectionImage,
                    model.PrizeText,
                    model.Color,
                    IsActive = true,
                    model.Probability,
                    model.WinRangeMin,
                    model.WinRangeMax,
                    model.TotalStock,
                    AvailableStock = model.AvailableStock ?? model.TotalStock
                };
            }

            var result = await con.QueryFirstOrDefaultAsync<BaseResponse>(
                model.SectionId == 0 ? "sp_AddSpinSection" : "sp_UpdateSpinSection",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new BaseResponse { ResultId = 0, ResultMessage = "Operation failed." };
        }

        public async Task<BaseResponse> DeleteSectionAsync(int sectionId)
        {
            using var con = Connection;
            var result = await _dapper.ExecuteAsync(
                con,
                "UPDATE SpinSection SET IsActive = 0 WHERE SectionId = @SectionId", // Soft delete
                new { SectionId = sectionId }
            );
            return new BaseResponse
            {
                ResultId = result > 0 ? 1 : 0,
                ResultMessage = result > 0 ? "Section deleted." : "Not found."
            };
        }

        public async Task<SpinGameDto> GetSpinGameByIdAsync(int gameId)
        {
            using var con = Connection;
            return await _dapper.QueryFirstOrDefaultAsync<SpinGameDto>(
                con,
                "SELECT * FROM SpinGame WHERE GameId = @GameId",
                new { GameId = gameId }
            );
        }

        public async Task<IEnumerable<SpinGameDto>> GetAllSpinGamesAsync()
        {
            using var con = Connection;
            return await _dapper.QueryAsync<SpinGameDto>(con, "SELECT * FROM SpinGame WHERE IsActive = 1");
        }

        public async Task<IEnumerable<SpinGameDto>> GetSpinGamesByBusinessAsync(int businessId)
        {
            using var con = Connection;
            return await _dapper.QueryAsync<SpinGameDto>(
                con,
                "SELECT * FROM SpinGame WHERE BusinessId = @BusinessId AND IsActive = 1",
                new { BusinessId = businessId }
            );
        }

        public async Task<BaseResponse> DeleteSpinGameAsync(int gameId)
        {
            using var con = Connection;
            var result = await _dapper.ExecuteAsync(
                con,
                "UPDATE SpinGame SET IsActive = 0 WHERE GameId = @GameId",
                new { GameId = gameId }
            );
            return new BaseResponse
            {
                ResultId = result > 0 ? 1 : 0,
                ResultMessage = result > 0 ? "Spin game soft-deleted." : "Not found."
            };
        }
        public async Task<PlaySpinResponse> PlaySpinGameAsync(PlaySpinRequest request,string redeemCode,string qrCodePath)
        {
            using var con = Connection;

            // Validate game
            var game = await GetSpinGameByIdAsync(request.GameId);
            if (game == null || !game.IsActive)
                return new PlaySpinResponse { ResultId = 0, ResultMessage = "Game not found or inactive." };

            // Optional: validate config (IsActive, date range)
            var config = await GetConfigByIdAsync(game.ConfigId);
            if (config == null || !config.IsActive || config.GameStartDate > DateTime.Now || config.GameEndDate < DateTime.Now)
            {
                return new PlaySpinResponse { ResultId = 0, ResultMessage = "Game configuration is invalid or expired." };
            }

            // Fetch sections
            var sectionsList = (await GetSectionsByGameIdAsync(request.GameId)).ToList();
            if (sectionsList == null || !sectionsList.Any())
                return new PlaySpinResponse { ResultId = 0, ResultMessage = "No sections configured for this game." };

            SpinSectionRequest? selectedSection = null;

            // 1. If client provided a valid SectionId (the section the wheel animation landed on), strictly honor it!
            if (request.SectionId > 0)
            {
                selectedSection = sectionsList.FirstOrDefault(s => s.SectionId == request.SectionId);
            }

            // 2. If no valid section provided by client, calculate based on configured probability ranges
            if (selectedSection == null)
            {
                var totalProbability = sectionsList.Sum(s => s.Probability);
                var hasConfiguredRanges = sectionsList.All(s => s.WinRangeMin.HasValue && s.WinRangeMax.HasValue);
                if (totalProbability == 100 && hasConfiguredRanges)
                {
                    int roll = Random.Shared.Next(1, 101); // 1 to 100
                    selectedSection = sectionsList.FirstOrDefault(s => roll >= s.WinRangeMin.Value && roll <= s.WinRangeMax.Value);
                }
            }

            // 3. Fallback to first section if still null
            if (selectedSection == null)
            {
                selectedSection = sectionsList.FirstOrDefault();
            }

            if (selectedSection == null) 
            {
                return new PlaySpinResponse { ResultId = 0, ResultMessage = "Invalid section or section does not belong to this game." };
            }

            // Check if selected section is out of stock (AvailableStock has value and <= 0)
            if (selectedSection.AvailableStock.HasValue && selectedSection.AvailableStock.Value <= 0)
            {
                // Fallback to a section with available stock or unlimited stock (try again, almost there, coins, etc.)
                var fallbackSection = sectionsList.FirstOrDefault(s =>
                    (s.AvailableStock == null || s.AvailableStock > 0) &&
                    (s.PrizeText?.Contains("try again", StringComparison.OrdinalIgnoreCase) == true ||
                     s.PrizeText?.Contains("almost there", StringComparison.OrdinalIgnoreCase) == true ||
                     s.PrizeText?.Contains("spin again", StringComparison.OrdinalIgnoreCase) == true ||
                     (s.Points.HasValue && s.Points.Value > 0)))
                    ?? sectionsList.FirstOrDefault(s => s.AvailableStock == null || s.AvailableStock > 0)
                    ?? selectedSection;

                selectedSection = fallbackSection;
            }

            // Decrement stock atomically if section has a stock limit
            if (selectedSection.AvailableStock.HasValue && selectedSection.AvailableStock.Value > 0)
            {
                const string updateStockSql = @"
                    UPDATE SpinSection 
                    SET AvailableStock = AvailableStock - 1 
                    WHERE SectionId = @SectionId AND AvailableStock > 0;";
                var affected = await _dapper.ExecuteAsync(con, updateStockSql, new { SectionId = selectedSection.SectionId });
                if (affected > 0)
                {
                    selectedSection.AvailableStock -= 1;
                }
                else
                {
                    // Fall back if stock just became 0 in race condition
                    var fallback = sectionsList.FirstOrDefault(s =>
                        (s.AvailableStock == null || s.AvailableStock > 0) &&
                        s.SectionId != selectedSection.SectionId);
                    if (fallback != null)
                    {
                        selectedSection = fallback;
                        if (selectedSection.AvailableStock.HasValue && selectedSection.AvailableStock.Value > 0)
                        {
                            await _dapper.ExecuteAsync(con, updateStockSql, new { SectionId = selectedSection.SectionId });
                            selectedSection.AvailableStock -= 1;
                        }
                    }
                }
            }

            string text = selectedSection.PrizeText ?? "";

            // 1. Check if losing / try again / spin again (using contains to handle emojis like 🔄, 😮, etc.)
            bool isLosingOrSpinAgain = text.Contains("spin again", StringComparison.OrdinalIgnoreCase) ||
                                       text.Contains("try again", StringComparison.OrdinalIgnoreCase) ||
                                       text.Contains("better luck", StringComparison.OrdinalIgnoreCase) ||
                                       text.Contains("almost there", StringComparison.OrdinalIgnoreCase) ||
                                       text.Contains("no prize", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(text.Trim(), "none", StringComparison.OrdinalIgnoreCase);

            // 2. Check if coin reward
            // A section is coins if Points > 0, or if text specifically mentions coins/IC and is NOT a % off / discount promotion
            bool hasDiscount = text.Contains("%") || text.Contains("off", StringComparison.OrdinalIgnoreCase) || text.Contains("discount", StringComparison.OrdinalIgnoreCase);
            bool hasCoinKeyword = System.Text.RegularExpressions.Regex.IsMatch(text, @"\b(ic|indocoin|indocoins|coin|coins|points?)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            int coins = 0;
            if (selectedSection.Points.HasValue && selectedSection.Points.Value > 0)
            {
                coins = selectedSection.Points.Value;
            }
            else if (!hasDiscount && hasCoinKeyword)
            {
                var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int parsed))
                {
                    coins = parsed;
                }
            }

            bool isCoinReward = coins > 0 || (!hasDiscount && hasCoinKeyword);
            if (isCoinReward)
            {
                selectedSection.Points = coins > 0 ? coins : 0;
            }
            else
            {
                selectedSection.Points = null;
            }

            // 3. Redeemable ONLY for real physical/voucher prizes (NOT coins, NOT spin-again/losing)
            bool isRedeemable = !isLosingOrSpinAgain && !isCoinReward &&
                                (selectedSection.PromotionId.GetValueOrDefault() > 0 ||
                                 (!string.IsNullOrWhiteSpace(selectedSection.PrizeText) && !selectedSection.PrizeText.Equals("None", StringComparison.OrdinalIgnoreCase)));

            if (!isRedeemable)
            {
                redeemCode = null;
                qrCodePath = null; 
            }

            // Insert into GameSpins (using the entity name as table name, common in this project schema e.g., SpinGame, SpinSection)
            var gameSpin = new CommUnityApp.Domain.Entities.GameSpin
            {
                UserId = request.UserId,
                SpinDate = DateTime.Now,
                SelectedSectionId = selectedSection.SectionId,
                PointsAwarded = selectedSection.Points,
                PromotionId = selectedSection.PromotionId,
                redeemCode = redeemCode,
                QRCodePath = qrCodePath,
                IsRedeemed = false
            };

            var insertQuery = @"
                INSERT INTO GameSpins (UserId, SpinDate, SelectedSectionId, PointsAwarded, PromotionId, RedeemCode, QRCodePath, IsRedeemed)
                VALUES (@UserId, @SpinDate, @SelectedSectionId, @PointsAwarded, @PromotionId, @redeemCode, @QRCodePath, 0);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            try
            {
                var spinId = await _dapper.QueryFirstOrDefaultAsync<int>(con, insertQuery, gameSpin);
                gameSpin.SpinId = spinId;

                return new PlaySpinResponse 
                {
                    ResultId = 1,
                    ResultMessage = "Spin played successfully.",
                    SelectedSection = selectedSection,
                    CoinsEarned = game.RewardCoins,
                    GameResultId = spinId, 
                    GameId = request.GameId,
                    SectionId = selectedSection.SectionId,
                    RewardValue = selectedSection.PrizeText,
                    RedeemCode = redeemCode,
                    QRCodePath = qrCodePath,
                    BusinessLocation = isRedeemable ? game.BusinessLocation : null,
                    Status = "Success",
                    PlayedAt = gameSpin.SpinDate,
                    TotalStock = selectedSection.TotalStock,
                    AvailableStock = selectedSection.AvailableStock,
                    IsRedeemed = false
                };
            }
            catch (Exception ex)
            {
                return new PlaySpinResponse { ResultId = 0, ResultMessage = "Failed to save game spin: " + ex.Message };
            }
        }

        public async Task<IEnumerable<GameSpinResultDto>> GetGameSpinResultsAsync(int? gameId, Guid? userId)
        {
            using var con = Connection;
            var queryBuilder = new System.Text.StringBuilder(@"
                SELECT 
                    gs.SpinId, 
                    gs.UserId, 
                    gs.SpinDate, 
                    gs.SelectedSectionId, 
                    gs.PointsAwarded, 
                    gs.PromotionId,
                    gs.IsRedeemed,
                    gs.RedeemedDate,

                    CASE 
                        WHEN ISNULL(gs.PointsAwarded, 0) > 0 
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%ic%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%coin%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%point%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%spin again%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%try again%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%better luck%'
                        THEN NULL
                        ELSE gs.RedeemCode
                    END AS RedeemCode,

                    CASE 
                        WHEN ISNULL(gs.PointsAwarded, 0) > 0 
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%ic%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%coin%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%point%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%spin again%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%try again%'
                             OR LOWER(ISNULL(ss.PrizeText, '')) LIKE '%better luck%'
                        THEN NULL
                        ELSE gs.QRCodePath
                    END AS QRCodePath,

                    CASE 
                        WHEN gs.RedeemCode IS NOT NULL 
                             AND LTRIM(RTRIM(gs.RedeemCode)) <> ''
                             AND ISNULL(gs.PointsAwarded, 0) = 0
                             AND LOWER(ISNULL(ss.PrizeText, '')) NOT LIKE '%ic%'
                             AND LOWER(ISNULL(ss.PrizeText, '')) NOT LIKE '%coin%'
                             AND LOWER(ISNULL(ss.PrizeText, '')) NOT LIKE '%point%'
                             AND LOWER(ISNULL(ss.PrizeText, '')) NOT LIKE '%spin again%'
                             AND LOWER(ISNULL(ss.PrizeText, '')) NOT LIKE '%try again%'
                             AND LOWER(ISNULL(ss.PrizeText, '')) NOT LIKE '%better luck%'
                        THEN sg.BusinessLocation 
                        ELSE NULL 
                    END AS BusinessLocation,
                    sg.GameId,
                    sg.GameName,
                    ss.PrizeText
                FROM GameSpins gs
                INNER JOIN SpinSection ss ON gs.SelectedSectionId = ss.SectionId
                INNER JOIN SpinGame sg ON ss.GameId = sg.GameId
                WHERE 1 = 1
            ");

            if (gameId.HasValue && gameId.Value > 0)
            {
                queryBuilder.Append(" AND sg.GameId = @GameId");
            }

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                queryBuilder.Append(" AND gs.UserId = @UserId");
            }

            queryBuilder.Append(" ORDER BY gs.SpinDate DESC");

            return await _dapper.QueryAsync<GameSpinResultDto>(con, queryBuilder.ToString(), new { GameId = gameId, UserId = userId });
        }

        public async Task AddSpinGameRewardCoinsAsync(Guid userId, int coins, int gameId, string? notes = null)
        {
            using var connection = new SqlConnection(
                _configuration?.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection is not configured."));

            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "SP_AddSpinGameRewardCoins",
                new
                {
                    UserId = userId,
                    Coins = coins,
                    GameId = gameId,
                    Notes = notes
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> TryConsumeSectionStockAsync(int sectionId)
        {
            using var con = Connection;
            const string sql = @"
                UPDATE SpinSection 
                SET AvailableStock = AvailableStock - 1 
                WHERE SectionId = @SectionId 
                  AND AvailableStock IS NOT NULL 
                  AND AvailableStock > 0;";
            var affected = await _dapper.ExecuteAsync(con, sql, new { SectionId = sectionId });
            return affected > 0;
        }

        public async Task<RedeemSpinPrizeResponse> RedeemSpinPrizeAsync(RedeemSpinPrizeRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RedeemCode))
            {
                return new RedeemSpinPrizeResponse
                {
                    ResultId = 0,
                    ResultMessage = "Redeem code is required.",
                    Status = false
                };
            }

            using var con = Connection;
            var trimmedCode = request.RedeemCode.Trim();

            const string findQuery = @"
                SELECT 
                    gs.SpinId, 
                    gs.UserId, 
                    gs.SpinDate, 
                    gs.SelectedSectionId, 
                    gs.PointsAwarded, 
                    gs.PromotionId, 
                    gs.RedeemCode, 
                    gs.QRCodePath, 
                    gs.IsRedeemed, 
                    gs.RedeemedDate,
                    gs.RedeemedBy,
                    ss.PrizeText,
                    ss.AvailableStock,
                    sg.GameId,
                    sg.GameName,
                    sg.BusinessLocation
                FROM GameSpins gs
                INNER JOIN SpinSection ss ON gs.SelectedSectionId = ss.SectionId
                INNER JOIN SpinGame sg ON ss.GameId = sg.GameId
                WHERE gs.RedeemCode = @RedeemCode;";

            var record = await _dapper.QueryFirstOrDefaultAsync<SpinRedemptionRecord>(con, findQuery, new { RedeemCode = trimmedCode });
            if (record == null)
            {
                return new RedeemSpinPrizeResponse
                {
                    ResultId = 0,
                    ResultMessage = "Invalid or non-existent redeem code.",
                    Status = false
                };
            }

            if (record.IsRedeemed)
            {
                DateTime? dt = record.RedeemedDate;
                string dateStr = dt.HasValue ? dt.Value.ToString("dd-MMM-yyyy hh:mm tt") : "previously";
                return new RedeemSpinPrizeResponse
                {
                    ResultId = 0,
                    ResultMessage = $"This prize has already been redeemed on {dateStr}.",
                    Status = false,
                    SpinId = record.SpinId,
                    GameId = record.GameId,
                    GameName = record.GameName,
                    PrizeText = record.PrizeText,
                    RedeemCode = trimmedCode,
                    RedeemedDate = dt
                };
            }

            // Mark as redeemed
            const string redeemSql = @"
                UPDATE GameSpins 
                SET IsRedeemed = 1, 
                    RedeemedDate = GETDATE(), 
                    RedeemedBy = @RedeemedBy 
                WHERE SpinId = @SpinId AND IsRedeemed = 0;";

            var affected = await _dapper.ExecuteAsync(con, redeemSql, new 
            { 
                SpinId = record.SpinId, 
                RedeemedBy = request.RedeemedBy ?? request.RedeemedByUserId?.ToString() 
            });

            if (affected <= 0)
            {
                return new RedeemSpinPrizeResponse
                {
                    ResultId = 0,
                    ResultMessage = "Could not redeem prize. It may have already been redeemed.",
                    Status = false
                };
            }

            return new RedeemSpinPrizeResponse
            {
                ResultId = 1,
                ResultMessage = "Prize redeemed successfully!",
                Status = true,
                SpinId = record.SpinId,
                GameId = record.GameId,
                GameName = record.GameName,
                PrizeText = record.PrizeText,
                BusinessLocation = record.BusinessLocation,
                RedeemCode = trimmedCode,
                RedeemedDate = DateTime.Now,
                RemainingStock = record.AvailableStock
            };
        }
        

    }
}

