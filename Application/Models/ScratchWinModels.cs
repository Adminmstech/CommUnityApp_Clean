using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace CommUnityApp.ApplicationCore.Models
{
    public class ScratchWinRewardDto
    {
        public int RewardId { get; set; }
        public int GameId { get; set; }
        public int RewardOrder { get; set; }
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

    public class ScratchWinGameDto
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
        public DateTime CreatedDate { get; set; }
        public List<ScratchWinRewardDto> Rewards { get; set; } = new();
    }

    public class AddUpdateScratchWinRewardRequest
    {
        public int RewardId { get; set; } = 0;
        public int RewardOrder { get; set; }
        public string RewardType { get; set; } = string.Empty;
        public string RewardName { get; set; } = string.Empty;
        public int CoinValue { get; set; } = 0;
        public decimal ProbabilityPercentage { get; set; }
        public int? TotalStock { get; set; }
        public int? AvailableStock { get; set; }
        public string RewardImage { get; set; } = string.Empty;
        public IFormFile? RewardImageFile { get; set; }
        public string? Description { get; set; }
        public string? WinMessage { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AddUpdateScratchWinGameRequest
    {
        public int GameId { get; set; } = 0;
        public string GameName { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public string? GameDescription { get; set; }
        public string? TermsAndConditions { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(6);
        public int ChanceCount { get; set; } = 1;
        public int OnceIn { get; set; } = 1;
        public int Status { get; set; } = 1;
        public int? BusinessId { get; set; } = 0;
        public string? BusinessLocation { get; set; }

        public List<AddUpdateScratchWinRewardRequest> Rewards { get; set; } = new();
    }

    public class PlayScratchGameRequest
    {
        public int GameId { get; set; }
        public Guid UserId { get; set; }
    }

    public class RedeemScratchPrizeRequest
    {
        public int GameId { get; set; }
        public Guid UserId { get; set; }
        public int? RewardId { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public string VerificationToken { get; set; } = string.Empty;
    }

    public class ScratchWinPlayHistoryDto
    {
        public int HistoryId { get; set; }
        public int GameId { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public int? RewardId { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty;
        public int CoinsEarned { get; set; }
        public bool IsWinner { get; set; }
        public int AttemptNumber { get; set; }
        public string? RedeemCode { get; set; }
        public string? QRCodePath { get; set; }
        public string? RedeemLocation { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
