using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class RewardsRepository:IRewardsRepository
    {
        private readonly IConfiguration _configuration;

        public RewardsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<Rewards?> GetCoins(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var result = await connection.QueryFirstOrDefaultAsync<Rewards>(
                "Get_UserEarnedCoins",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<BaseResponse> SaveShareRewardConfig(SaveShareRewardConfigRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@BusinessId", request.BusinessId);
            parameters.Add("@PromotionShareCoins", request.PromotionShareCoins);
            parameters.Add("@ProductShareCoins", request.ProductShareCoins);
            parameters.Add("@ReferralCoins", request.ReferralCoins);

            var result = await connection.QueryAsync<BaseResponse>(
                "Save_ShareRewardConfig",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<ShareRewardConfigModel> GetShareRewardConfig(int businessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@BusinessId", businessId);

            var result =
                await connection.QueryFirstOrDefaultAsync<ShareRewardConfigModel>(
                    "Get_ShareRewardConfig",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

            return result;
        }

        public async Task<BaseResponse> RewardShare(ShareRewardRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@BusinessId", request.BusinessId);
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@ShareType", request.ShareType);
            parameters.Add("@ReferenceId", request.ReferenceId);
            parameters.Add("@SharePlatform", request.SharePlatform);

            var result = await connection.QueryAsync<BaseResponse>(
                "Reward_Share",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<List<ShareRewardHistoryModel>> GetShareRewards(int businessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@BusinessId", businessId);

            var result = await connection.QueryAsync<ShareRewardHistoryModel>(
                "Get_ShareRewards",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }


        public async Task<List<ShareRewardModel>> GetUserShareRewards(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);

            var result = await connection.QueryAsync<ShareRewardModel>(
                "Get_UserShareRewards",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }
}
