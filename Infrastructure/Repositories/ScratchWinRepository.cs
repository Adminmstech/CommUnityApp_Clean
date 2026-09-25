using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class ScratchWinRepository : IScratchWinRepository
    {
        private readonly IConfiguration _configuration;

        public ScratchWinRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<BaseResponse> AddUpdateScratchWinGameAsync(AddUpdateScratchWinGameRequest model)
        {
            using var con = Connection;
            con.Open();
            using var tx = con.BeginTransaction();

            try
            {
                int gameId = model.GameId;

                if (gameId == 0)
                {
                    const string insertGameSql = @"
                        INSERT INTO ScratchWinGame (
                            GameName,
                            GameTitle,
                            GameDescription,
                            TermsAndConditions,
                            StartDate,
                            EndDate,
                            ChanceCount,
                            OnceIn,
                            TotalEntries,
                            Status,
                            BusinessId,
                            BusinessLocation,
                            CreatedDate,
                            IsDeleted
                        )
                        VALUES (
                            @GameName,
                            @GameTitle,
                            @GameDescription,
                            @TermsAndConditions,
                            @StartDate,
                            @EndDate,
                            @ChanceCount,
                            @OnceIn,
                            0,
                            @Status,
                            @BusinessId,
                            @BusinessLocation,
                            GETDATE(),
                            0
                        );
                        SELECT CAST(SCOPE_IDENTITY() as int);";

                    gameId = await con.ExecuteScalarAsync<int>(insertGameSql, new
                    {
                        model.GameName,
                        model.GameTitle,
                        model.GameDescription,
                        model.TermsAndConditions,
                        model.StartDate,
                        model.EndDate,
                        model.ChanceCount,
                        model.OnceIn,
                        model.Status,
                        BusinessId = model.BusinessId ?? 0,
                        BusinessLocation = model.BusinessLocation ?? "Main Store"
                    }, tx);

                    if (model.Rewards != null && model.Rewards.Any())
                    {
                        const string insertRewardSql = @"
                            INSERT INTO ScratchWinReward (
                                GameId,
                                RewardOrder,
                                RewardType,
                                RewardName,
                                CoinValue,
                                ProbabilityPercentage,
                                TotalStock,
                                AvailableStock,
                                RewardImage,
                                Description,
                                WinMessage,
                                IsActive
                            )
                            VALUES (
                                @GameId,
                                @RewardOrder,
                                @RewardType,
                                @RewardName,
                                @CoinValue,
                                @ProbabilityPercentage,
                                @TotalStock,
                                @AvailableStock,
                                @RewardImage,
                                @Description,
                                @WinMessage,
                                @IsActive
                            );";

                        foreach (var reward in model.Rewards)
                        {
                            await con.ExecuteAsync(insertRewardSql, new
                            {
                                GameId = gameId,
                                reward.RewardOrder,
                                reward.RewardType,
                                reward.RewardName,
                                reward.CoinValue,
                                reward.ProbabilityPercentage,
                                reward.TotalStock,
                                AvailableStock = reward.TotalStock,
                                reward.RewardImage,
                                reward.Description,
                                reward.WinMessage,
                                reward.IsActive
                            }, tx);
                        }
                    }
                }
                else
                {
                    const string updateGameSql = @"
                        UPDATE ScratchWinGame
                        SET GameName = @GameName,
                            GameTitle = @GameTitle,
                            GameDescription = @GameDescription,
                            TermsAndConditions = @TermsAndConditions,
                            StartDate = @StartDate,
                            EndDate = @EndDate,
                            ChanceCount = @ChanceCount,
                            OnceIn = @OnceIn,
                            Status = @Status,
                            BusinessId = @BusinessId,
                            BusinessLocation = @BusinessLocation,
                            UpdatedDate = GETDATE()
                        WHERE GameId = @GameId AND IsDeleted = 0;";

                    await con.ExecuteAsync(updateGameSql, new
                    {
                        model.GameId,
                        model.GameName,
                        model.GameTitle,
                        model.GameDescription,
                        model.TermsAndConditions,
                        model.StartDate,
                        model.EndDate,
                        model.ChanceCount,
                        model.OnceIn,
                        model.Status,
                        BusinessId = model.BusinessId ?? 0,
                        BusinessLocation = model.BusinessLocation ?? "Main Store"
                    }, tx);

                    if (model.Rewards != null && model.Rewards.Any())
                    {
                        foreach (var reward in model.Rewards)
                        {
                            if (reward.RewardId > 0)
                            {
                                const string updateRewardSql = @"
                                    UPDATE ScratchWinReward
                                    SET RewardOrder = @RewardOrder,
                                        RewardType = @RewardType,
                                        RewardName = @RewardName,
                                        CoinValue = @CoinValue,
                                        ProbabilityPercentage = @ProbabilityPercentage,
                                        TotalStock = @TotalStock,
                                        AvailableStock = CASE WHEN @TotalStock IS NULL THEN NULL ELSE ISNULL(AvailableStock, @TotalStock) END,
                                        RewardImage = @RewardImage,
                                        Description = @Description,
                                        WinMessage = @WinMessage,
                                        IsActive = @IsActive
                                    WHERE RewardId = @RewardId AND GameId = @GameId;";

                                await con.ExecuteAsync(updateRewardSql, new
                                {
                                    reward.RewardId,
                                    GameId = gameId,
                                    reward.RewardOrder,
                                    reward.RewardType,
                                    reward.RewardName,
                                    reward.CoinValue,
                                    reward.ProbabilityPercentage,
                                    reward.TotalStock,
                                    reward.RewardImage,
                                    reward.Description,
                                    reward.WinMessage,
                                    reward.IsActive
                                }, tx);
                            }
                            else
                            {
                                const string checkExistSql = @"SELECT RewardId FROM ScratchWinReward WHERE GameId = @GameId AND RewardOrder = @RewardOrder;";
                                var existingRewardId = await con.ExecuteScalarAsync<int?>(checkExistSql, new { GameId = gameId, reward.RewardOrder }, tx);

                                if (existingRewardId.HasValue && existingRewardId.Value > 0)
                                {
                                    const string updateByOrderSql = @"
                                        UPDATE ScratchWinReward
                                        SET RewardType = @RewardType,
                                            RewardName = @RewardName,
                                            CoinValue = @CoinValue,
                                            ProbabilityPercentage = @ProbabilityPercentage,
                                            TotalStock = @TotalStock,
                                            AvailableStock = CASE WHEN @TotalStock IS NULL THEN NULL ELSE ISNULL(AvailableStock, @TotalStock) END,
                                            RewardImage = @RewardImage,
                                            Description = @Description,
                                            WinMessage = @WinMessage,
                                            IsActive = @IsActive
                                        WHERE RewardId = @RewardId AND GameId = @GameId;";

                                    await con.ExecuteAsync(updateByOrderSql, new
                                    {
                                        RewardId = existingRewardId.Value,
                                        GameId = gameId,
                                        reward.RewardType,
                                        reward.RewardName,
                                        reward.CoinValue,
                                        reward.ProbabilityPercentage,
                                        reward.TotalStock,
                                        reward.RewardImage,
                                        reward.Description,
                                        reward.WinMessage,
                                        reward.IsActive
                                    }, tx);
                                }
                                else
                                {
                                    const string insertRewardSql = @"
                                        INSERT INTO ScratchWinReward (
                                            GameId,
                                            RewardOrder,
                                            RewardType,
                                            RewardName,
                                            CoinValue,
                                            ProbabilityPercentage,
                                            TotalStock,
                                            AvailableStock,
                                            RewardImage,
                                            Description,
                                            WinMessage,
                                            IsActive
                                        )
                                        VALUES (
                                            @GameId,
                                            @RewardOrder,
                                            @RewardType,
                                            @RewardName,
                                            @CoinValue,
                                            @ProbabilityPercentage,
                                            @TotalStock,
                                            @AvailableStock,
                                            @RewardImage,
                                            @Description,
                                            @WinMessage,
                                            @IsActive
                                        );";

                                    await con.ExecuteAsync(insertRewardSql, new
                                    {
                                        GameId = gameId,
                                        reward.RewardOrder,
                                        reward.RewardType,
                                        reward.RewardName,
                                        reward.CoinValue,
                                        reward.ProbabilityPercentage,
                                        reward.TotalStock,
                                        AvailableStock = reward.TotalStock,
                                        reward.RewardImage,
                                        reward.Description,
                                        reward.WinMessage,
                                        reward.IsActive
                                    }, tx);
                                }
                            }
                        }
                    }
                }

                tx.Commit();
                return new BaseResponse
                {
                    ResultId = 1,
                    ResultMessage = "Scratch & Win game saved successfully."
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = $"Error saving Scratch & Win game: {ex.Message}"
                };
            }
        }

        public async Task<ScratchWinGameDto?> GetScratchWinGameByIdAsync(int gameId)
        {
            using var con = Connection;
            const string sql = @"
                SELECT * FROM ScratchWinGame WHERE GameId = @GameId AND IsDeleted = 0;
                SELECT * FROM ScratchWinReward WHERE GameId = @GameId ORDER BY RewardOrder ASC;";

            using var multi = await con.QueryMultipleAsync(sql, new { GameId = gameId });
            var game = await multi.ReadFirstOrDefaultAsync<ScratchWinGameDto>();
            if (game != null)
            {
                var rewards = (await multi.ReadAsync<ScratchWinRewardDto>()).ToList();
                game.Rewards = rewards;
            }
            return game;
        }

        public async Task<ScratchWinGameDto?> GetActiveScratchWinGameAsync()
        {
            using var con = Connection;
            const string sql = @"
                SELECT TOP 1 * 
                FROM ScratchWinGame 
                WHERE Status = 1 AND IsDeleted = 0 
                ORDER BY GameId DESC;
                
                SELECT * FROM ScratchWinReward 
                WHERE GameId = (SELECT TOP 1 GameId FROM ScratchWinGame WHERE Status = 1 AND IsDeleted = 0 ORDER BY GameId DESC) 
                  AND IsActive = 1
                ORDER BY RewardOrder ASC;";

            using var multi = await con.QueryMultipleAsync(sql);
            var game = await multi.ReadFirstOrDefaultAsync<ScratchWinGameDto>();
            if (game != null)
            {
                var rewards = (await multi.ReadAsync<ScratchWinRewardDto>()).ToList();
                game.Rewards = rewards;
            }
            return game;
        }

        public async Task<IEnumerable<ScratchWinGameDto>> GetAllScratchWinGamesAsync()
        {
            using var con = Connection;
            const string sql = @"
                SELECT g.*, 
                       (SELECT COUNT(*) FROM ScratchWinPlayHistory h WHERE h.GameId = g.GameId) AS TotalEntries
                FROM ScratchWinGame g
                WHERE g.IsDeleted = 0
                ORDER BY g.GameId DESC;";

            var games = (await con.QueryAsync<ScratchWinGameDto>(sql)).ToList();

            if (games.Any())
            {
                var gameIds = games.Select(x => x.GameId).ToArray();
                const string rewardsSql = "SELECT * FROM ScratchWinReward WHERE GameId IN @GameIds ORDER BY RewardOrder ASC;";
                var rewards = (await con.QueryAsync<ScratchWinRewardDto>(rewardsSql, new { GameIds = gameIds })).ToList();

                foreach (var game in games)
                {
                    game.Rewards = rewards.Where(r => r.GameId == game.GameId).ToList();
                }
            }

            return games;
        }

        public async Task<BaseResponse> DeleteScratchWinGameAsync(int gameId)
        {
            using var con = Connection;
            const string sql = "UPDATE ScratchWinGame SET IsDeleted = 1 WHERE GameId = @GameId;";
            var affected = await con.ExecuteAsync(sql, new { GameId = gameId });
            return new BaseResponse
            {
                ResultId = affected > 0 ? 1 : 0,
                ResultMessage = affected > 0 ? "Game deleted successfully." : "Game not found."
            };
        }

        public async Task<BaseResponse> ToggleGameStatusAsync(int gameId)
        {
            using var con = Connection;
            const string sql = @"
                UPDATE ScratchWinGame 
                SET Status = CASE WHEN Status = 1 THEN 0 ELSE 1 END,
                    UpdatedDate = GETDATE()
                WHERE GameId = @GameId AND IsDeleted = 0;";
            var affected = await con.ExecuteAsync(sql, new { GameId = gameId });
            return new BaseResponse
            {
                ResultId = affected > 0 ? 1 : 0,
                ResultMessage = affected > 0 ? "Game status updated successfully." : "Game not found."
            };
        }

        public async Task<int> GetTotalPlaysCountAsync(int gameId)
        {
            using var con = Connection;
            const string sql = "SELECT COUNT(*) FROM ScratchWinPlayHistory WHERE GameId = @GameId;";
            return await con.ExecuteScalarAsync<int>(sql, new { GameId = gameId });
        }

        public async Task<bool> TryConsumeRewardStockAsync(int rewardId)
        {
            using var con = Connection;
            const string sql = @"
                UPDATE ScratchWinReward 
                SET AvailableStock = AvailableStock - 1 
                WHERE RewardId = @RewardId 
                  AND AvailableStock IS NOT NULL 
                  AND AvailableStock > 0;";
            var affected = await con.ExecuteAsync(sql, new { RewardId = rewardId });
            return affected > 0;
        }

        public async Task<int> TrackGameplayAsync(int gameId, Guid userId, int? rewardId, string rewardName, string rewardType, int coins, bool isWinner, int attemptNumber, string? redeemCode, string? qrCodePath, string? redeemLocation = null)
        {
            using var con = Connection;
            const string sql = @"
                INSERT INTO ScratchWinPlayHistory (
                    GameId,
                    UserId,
                    RewardId,
                    RewardName,
                    RewardType,
                    CoinsEarned,
                    IsWinner,
                    AttemptNumber,
                    RedeemCode,
                    QRCodePath,
                    RedeemLocation,
                    CreatedDate
                )
                VALUES (
                    @GameId,
                    @UserId,
                    @RewardId,
                    @RewardName,
                    @RewardType,
                    @CoinsEarned,
                    @IsWinner,
                    @AttemptNumber,
                    @RedeemCode,
                    @QRCodePath,
                    @RedeemLocation,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await con.ExecuteScalarAsync<int>(sql, new
            {
                GameId = gameId,
                UserId = userId,
                RewardId = rewardId,
                RewardName = rewardName,
                RewardType = rewardType,
                CoinsEarned = coins,
                IsWinner = isWinner,
                AttemptNumber = attemptNumber,
                RedeemCode = redeemCode,
                QRCodePath = qrCodePath,
                RedeemLocation = redeemLocation
            });
        }

        public async Task AddRewardCoinsAsync(Guid userId, int coins, int gameId)
        {
            using var con = Connection;
            await con.ExecuteAsync(
                "SP_AddBrandGameRewardCoins",
                new
                {
                    UserId = userId,
                    Coins = coins,
                    GameId = gameId,
                    Notes = "Scratch & Win reward coins"
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ScratchWinPlayHistoryDto>> GetGameplayHistoryAsync(int? gameId, int pageNumber = 1, int pageSize = 50)
        {
            using var con = Connection;
            var offset = (pageNumber - 1) * pageSize;
            const string sql = @"
                SELECT h.*
                FROM ScratchWinPlayHistory h
                WHERE (@GameId IS NULL OR h.GameId = @GameId)
                ORDER BY h.HistoryId DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            return await con.QueryAsync<ScratchWinPlayHistoryDto>(sql, new
            {
                GameId = gameId,
                Offset = offset,
                PageSize = pageSize
            });
        }
    }
}
