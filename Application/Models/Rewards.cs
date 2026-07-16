using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Rewards
    {
        public Guid UserId { get; set; }

        public int TotalCoinsEarned { get; set; }

        public int TotalCoinsUsed { get; set; }

        public int? BalanceCoins { get; set; }

        public decimal? MoneyValue { get; set; }
    }

    public class ShareRewardRequest
    {
        public int BusinessId { get; set; }

        public Guid UserId { get; set; }

        public string ShareType { get; set; }

        public int ReferenceId { get; set; }

        public string SharePlatform { get; set; }
    }

    public class ShareRewardConfigModel
    {
        public int Id { get; set; }

        public int BusinessId { get; set; }

        public int PromotionShareCoins { get; set; }

        public int ProductShareCoins { get; set; }

        public int ReferralCoins { get; set; }

        public bool IsActive { get; set; }
    }

    public class SaveShareRewardConfigRequest
    {
        public int BusinessId { get; set; }

        public int PromotionShareCoins { get; set; }

        public int ProductShareCoins { get; set; }

        public int ReferralCoins { get; set; }
    }

    public class ShareRewardModel
    {
        public long ShareRewardId { get; set; }

        public int BusinessId { get; set; }

        public Guid UserId { get; set; }

        public string ShareType { get; set; }

        public int ReferenceId { get; set; }

        public string SharePlatform { get; set; }

        public int RewardCoins { get; set; }

        public bool RewardGiven { get; set; }

        public DateTime SharedAt { get; set; }
    }

    
}
