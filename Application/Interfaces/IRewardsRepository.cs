using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IRewardsRepository
    {
        Task<Rewards?> GetCoins(Guid userId);
        Task<DailyStreakResponse?> ClaimDailyStreak(Guid userId);
        Task<DailyStreakDetailsResponse?> GetDailyStreakDetails(Guid userId);
        Task<BaseResponse> RewardShare(ShareRewardRequest request);

        Task<BaseResponse> SaveShareRewardConfig(SaveShareRewardConfigRequest request);

        Task<ShareRewardConfigModel> GetShareRewardConfig(int businessId);

        Task<BaseResponse> SaveReferAndEarnConfig(ReferAndEarnConfigRequest request);

        Task<ShareRewardConfigModel?> GetReferAndEarnConfig(int? businessId);

        Task<ReferralCodeResponse?> GenerateReferralCode(GenerateReferralCodeRequest request);

        Task<ApplyReferralCodeResponse?> ApplyReferralCode(ApplyReferralCodeRequest request);

        Task<List<ShareRewardHistoryModel>> GetShareRewards(int businessId);
        Task<List<ShareRewardModel>> GetUserShareRewards(Guid userId);
    }
}
