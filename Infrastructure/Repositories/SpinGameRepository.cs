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
        private readonly IConfiguration _configuration;

        public SpinGameRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<BaseResponse> AddUpdateSpinGameAsync(AddUpdateSpinGameRequest model)
        {
            using var con = Connection;
            var parameters = new
            {
                model.GameId,
                model.BusinessId,
                model.GameName,
                model.Description,
                model.ConfigId,
                model.CreatedByAdminId,
                model.IsActive
            };

            var result = await con.QueryFirstOrDefaultAsync<BaseResponse>(
                model.GameId == 0 ? "sp_AddSpinGame" : "sp_UpdateSpinGame", // Assume SPs exist or create them
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new BaseResponse { ResultId = 0, ResultMessage = "Operation failed." };
        }

        public async Task<SpinGameConfigRequest?> GetConfigByIdAsync(int configId)
        {
            using var con = Connection;
            return await con.QueryFirstOrDefaultAsync<SpinGameConfigRequest>(
                "SELECT ConfigId, MaxSpinsPerDay, NumberOfSections, GameStartDate, GameEndDate, IsActive FROM SpinGameConfiguration WHERE ConfigId = @ConfigId",
                new { ConfigId = configId }
            );
        }

        public async Task<IEnumerable<SpinSectionRequest>> GetSectionsByGameIdAsync(int gameId)
        {
            using var con = Connection;
            return await con.QueryAsync<SpinSectionRequest>(
                @"SELECT SectionId, GameId, SectionNumber, Points, PromotionId 
                  FROM SpinSection WHERE GameId = @GameId ORDER BY SectionNumber",
                new { GameId = gameId }
            );
        }

        public async Task<BaseResponse> AddUpdateConfigAsync(SpinGameConfigRequest model)
        {
            using var con = Connection;
            var parameters = new
            {
                model.ConfigId,
                model.MaxSpinsPerDay,
                model.NumberOfSections,
                model.GameStartDate,
                model.GameEndDate,
                model.IsActive
            };

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
            var parameters = new
            {
                model.SectionId,
                model.GameId,
                model.SectionNumber,
                model.Points,
                model.PromotionId
            };

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
            var result = await con.ExecuteAsync(
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
            return await con.QueryFirstOrDefaultAsync<SpinGameDto>(
                "SELECT * FROM SpinGame WHERE GameId = @GameId",
                new { GameId = gameId }
            );
        }

        public async Task<IEnumerable<SpinGameDto>> GetAllSpinGamesAsync()
        {
            using var con = Connection;
            return await con.QueryAsync<SpinGameDto>("SELECT * FROM SpinGame WHERE IsActive = 1");
        }

        public async Task<IEnumerable<SpinGameDto>> GetSpinGamesByBusinessAsync(int businessId)
        {
            using var con = Connection;
            return await con.QueryAsync<SpinGameDto>(
                "SELECT * FROM SpinGame WHERE BusinessId = @BusinessId AND IsActive = 1",
                new { BusinessId = businessId }
            );
        }

        public async Task<BaseResponse> DeleteSpinGameAsync(int gameId)
        {
            using var con = Connection;
            var result = await con.ExecuteAsync(
                "UPDATE SpinGame SET IsActive = 0 WHERE GameId = @GameId",
                new { GameId = gameId }
            );
            return new BaseResponse
            {
                ResultId = result > 0 ? 1 : 0,
                ResultMessage = result > 0 ? "Spin game soft-deleted." : "Not found."
            };
        }
    }
}

