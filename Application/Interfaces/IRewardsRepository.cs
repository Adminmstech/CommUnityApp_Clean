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
        Task<BaseResponse> RewardShare(ShareRewardRequest request);

        Task<BaseResponse> SaveShareRewardConfig(SaveShareRewardConfigRequest request);

        Task<ShareRewardConfigModel> GetShareRewardConfig(int businessId);

        Task<List<ShareRewardHistoryModel>> GetShareRewards(int businessId);
        Task<List<ShareRewardModel>> GetUserShareRewards(Guid userId);
    }
}
