using System;

namespace CommUnityApp.Domain.Entities
{
    public class ScratchWinGame
    {
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public string? GameDescription { get; set; }
        public string? TermsAndConditions { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ChanceCount { get; set; } = 1;
        public int OnceIn { get; set; } = 1;
        public int TotalEntries { get; set; } = 0;
        public int Status { get; set; } = 1; // 1 = Active, 0 = Inactive
        public int? BusinessId { get; set; } = 0;
        public string? BusinessLocation { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    public class ScratchWinReward
    {
        public int RewardId { get; set; }
        public int GameId { get; set; }
        public int RewardOrder { get; set; } // 1 to 8
        public string RewardType { get; set; } = string.Empty; // 'Prize', 'Coins', 'Consolation'
        public string RewardName { get; set; } = string.Empty;
        public int CoinValue { get; set; } = 0;
        public decimal ProbabilityPercentage { get; set; }
        public int? TotalStock { get; set; }
        public int? AvailableStock { get; set; }
        public string RewardImage { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? WinMessage { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ScratchWinPlayHistory
    {
        public int HistoryId { get; set; }
        public int GameId { get; set; }
        public Guid UserId { get; set; }
        public int? RewardId { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty;
        public int CoinsEarned { get; set; } = 0;
        public bool IsWinner { get; set; } = false;
        public int AttemptNumber { get; set; }
        public string? RedeemCode { get; set; }
        public string? QRCodePath { get; set; }
        public string? RedeemLocation { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
