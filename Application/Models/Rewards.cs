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

    public class ReferAndEarnConfigRequest
    {
        public int? BusinessId { get; set; }

        public int ReferralCoins { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class GenerateReferralCodeRequest
    {
        public Guid ReferrerUserId { get; set; }

        public string ReferralType { get; set; } = "App";

        public int? ReferenceId { get; set; }

        public int? BusinessId { get; set; }
    }

    public class ReferralCodeResponse
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; } = string.Empty;

        public long? ReferralCodeId { get; set; }

        public string? ReferralCode { get; set; }

        public string? ReferralType { get; set; }

        public int? ReferenceId { get; set; }

        public int? BusinessId { get; set; }
    }

    public class ApplyReferralCodeRequest
    {
        public string ReferralCode { get; set; } = string.Empty;

        public Guid UsedByUserId { get; set; }
    }

    public class ApplyReferralCodeResponse
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; } = string.Empty;

        public bool Status { get; set; }

        public long? ReferralUseId { get; set; }

        public string? ReferralCode { get; set; }

        public Guid ReferrerUserId { get; set; }

        public Guid UsedByUserId { get; set; }

        public int ReferrerRewardCoins { get; set; }

        public int ReferredUserRewardCoins { get; set; }
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

    public class DailyStreakResponse
    {
        public bool Success { get; set; }
        public bool AlreadyClaimed { get; set; }
        public int CurrentStreak { get; set; }
        public DateTime? LastClaimDate { get; set; }
        public int CoinsEarned { get; set; }
        public int NextRewardCoins { get; set; }
        public int NextStreakDay { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DailyStreakDetailsResponse
    {
        public bool Success { get; set; }
        public bool AlreadyClaimed { get; set; }
        public int CurrentStreak { get; set; }
        public int TotalStreakDays { get; set; }
        public int DailyRewardCoins { get; set; }
        public int MilestoneRewardCoins { get; set; }
        public int DaysToMilestoneReward { get; set; }
        public DateTime? LastClaimDate { get; set; }
        public DateTime Today { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    
}
