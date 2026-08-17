using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
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

        public async Task<DailyStreakResponse?> ClaimDailyStreak(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.Guid);

            var result = await connection.QueryFirstOrDefaultAsync<DailyStreakResponse>(
                "dbo.Claim_UserDailyStreak",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<DailyStreakDetailsResponse?> GetDailyStreakDetails(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            const string sql = @"
DECLARE @Today DATE =
    CONVERT
    (
        DATE,
        SYSUTCDATETIME()
            AT TIME ZONE 'UTC'
            AT TIME ZONE 'India Standard Time'
    );

DECLARE @TotalStreakDays INT = 15;
DECLARE @DailyRewardCoins INT = 5;
DECLARE @MilestoneRewardCoins INT = 30;

SELECT
    CAST(1 AS BIT) AS Success,
    CAST(CASE WHEN uds.LastClaimDate = @Today THEN 1 ELSE 0 END AS BIT) AS AlreadyClaimed,
    CASE
        WHEN uds.UserId IS NULL THEN 0
        WHEN uds.LastClaimDate IS NULL THEN 0
        WHEN uds.LastClaimDate = @Today THEN ISNULL(uds.CurrentStreak, 0)
        WHEN DATEDIFF(DAY, uds.LastClaimDate, @Today) = 1 THEN ISNULL(uds.CurrentStreak, 0)
        ELSE 0
    END AS CurrentStreak,
    @TotalStreakDays AS TotalStreakDays,
    @DailyRewardCoins AS DailyRewardCoins,
    @MilestoneRewardCoins AS MilestoneRewardCoins,
    @TotalStreakDays - 
        CASE
            WHEN uds.UserId IS NULL THEN 0
            WHEN uds.LastClaimDate IS NULL THEN 0
            WHEN uds.LastClaimDate = @Today THEN ISNULL(uds.CurrentStreak, 0)
            WHEN DATEDIFF(DAY, uds.LastClaimDate, @Today) = 1 THEN ISNULL(uds.CurrentStreak, 0)
            ELSE 0
        END AS DaysToMilestoneReward,
    uds.LastClaimDate,
    @Today AS Today,
    CASE
        WHEN uds.LastClaimDate = @Today
            THEN CONCAT('You earned ', @DailyRewardCoins, ' IC for today!')
        ELSE CONCAT('Earn ', @DailyRewardCoins, ' IC for today!')
    END AS Message
FROM (SELECT @UserId AS UserId) u
LEFT JOIN dbo.UserDailyStreak uds ON uds.UserId = u.UserId;";

            return await connection.QueryFirstOrDefaultAsync<DailyStreakDetailsResponse>(
                sql,
                new
                {
                    UserId = userId
                }
            );
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

        public async Task<BaseResponse> SaveReferAndEarnConfig(ReferAndEarnConfigRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>(
                "Save_ReferAndEarnConfig",
                new
                {
                    request.BusinessId,
                    request.ReferralCoins,
                    request.IsActive
                },
                commandType: CommandType.StoredProcedure
            );

            return result ?? new BaseResponse
            {
                ResultId = 0,
                ResultMessage = "Configuration was not saved."
            };
        }

        public async Task<ShareRewardConfigModel?> GetReferAndEarnConfig(int? businessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<ShareRewardConfigModel>(
                "Get_ReferAndEarnConfig",
                new
                {
                    BusinessId = businessId
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<ReferralCodeResponse?> GenerateReferralCode(GenerateReferralCodeRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<ReferralCodeResponse>(
                "Generate_ReferralCode",
                new
                {
                    request.ReferrerUserId,
                    request.ReferralType,
                    request.ReferenceId,
                    request.BusinessId
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<ApplyReferralCodeResponse?> ApplyReferralCode(ApplyReferralCodeRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<ApplyReferralCodeResponse>(
                "Apply_ReferralCode",
                new
                {
                    request.ReferralCode,
                    request.UsedByUserId
                },
                commandType: CommandType.StoredProcedure
            );
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
