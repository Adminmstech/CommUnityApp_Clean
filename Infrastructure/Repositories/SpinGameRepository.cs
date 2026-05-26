using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
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
    public class SpinGameRepository : ISpinGameRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly IDapperWrapper _dapper; // New dependency

        public SpinGameRepository(Func<IDbConnection> connectionFactory, IDapperWrapper dapper) // Modified constructor
        {
            _connectionFactory = connectionFactory;
            _dapper = dapper; // Initialize new dependency
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
                        model.IsActive
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
                                sectionModel.Points,
                                sectionModel.PromotionId,
                                sectionModel.PrizeText,
                                sectionModel.Color
                            };
                        }
                        else
                        {
                            sectionParameters = new
                            {
                                sectionModel.SectionId,
                                sectionModel.GameId,
                                sectionModel.SectionNumber,
                                sectionModel.Points,
                                sectionModel.PromotionId,
                                sectionModel.PrizeText,
                                sectionModel.Color,
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
                @"SELECT SectionId, GameId, SectionNumber, Points, PromotionId, PrizeText, Color 
                  FROM SpinSection WHERE GameId = @GameId ORDER BY SectionNumber",
                new { GameId = gameId }
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
                    model.Points,
                    model.PromotionId,
                    model.PrizeText,
                    model.Color
                };
            }
            else
            {
                parameters = new
                {
                    model.SectionId,
                    model.GameId,
                    model.SectionNumber,
                    model.Points,
                    model.PromotionId,
                    model.PrizeText,
                    model.Color,
                    IsActive = true
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
        public async Task<PlaySpinResponse> PlaySpinGameAsync(PlaySpinRequest request)
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
            var sections = await GetSectionsByGameIdAsync(request.GameId);
            if (sections == null || !sections.Any())
                 return new PlaySpinResponse { ResultId = 0, ResultMessage = "No sections configured for this game." };

            // Validate and select the section provided in the request
            var selectedSection = sections.FirstOrDefault(s => s.SectionId == request.SectionId);
            if (selectedSection == null)
            {
                 return new PlaySpinResponse { ResultId = 0, ResultMessage = "Invalid section or section does not belong to this game." };
            }

            // Insert into GameSpin (using the entity name as table name, common in this project schema e.g., SpinGame, SpinSection)
            var gameSpin = new CommUnityApp.Domain.Entities.GameSpin
            {
                UserId = request.UserId,
                SpinDate = DateTime.Now,
                SelectedSectionId = selectedSection.SectionId,
                PointsAwarded = selectedSection.Points,
                PromotionId = selectedSection.PromotionId
            };

            var insertQuery = @"
                INSERT INTO GameSpins (UserId, SpinDate, SelectedSectionId, PointsAwarded, PromotionId)
                VALUES (@UserId, @SpinDate, @SelectedSectionId, @PointsAwarded, @PromotionId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            try
            {
                var spinId = await _dapper.QueryFirstOrDefaultAsync<int>(con, insertQuery, gameSpin);
                gameSpin.SpinId = spinId;

                return new PlaySpinResponse
                {
                    ResultId = 1,
                    ResultMessage = "Spin played successfully.",
                    SelectedSection = selectedSection
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
    }
}

