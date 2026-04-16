using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace CommUnityApp.ApplicationCore.Models
{
    public class BrandGameDto
    {
        public int BrandGameID { get; set; }
        public string BrandGameName { get; set; }
        public string BrandGameTitle { get; set; }
        public string BrandGameImage { get; set; }
        public int? UserGroupId { get; set; }
        public int? BusinessId { get; set; }
        public string BrandGameDesc { get; set; }
        public string ConditionsApply { get; set; }
        public int? GameClassificationID { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public int PanelCount { get; set; }
        public int PanelOpeningLimit { get; set; }
        public int ChanceCount { get; set; }
        public DateTime? DateEntered { get; set; }
        public string DestinationUrl { get; set; }
        public int Status { get; set; }
        public string QRImagePath { get; set; }
        public int? LimitCount { get; set; }
        public string PrimaryOfferText { get; set; }
        public string OfferText { get; set; }
        public string PrimaryWinMessage { get; set; }
        public string SecondaryWinMessage { get; set; }
        public string ConsolationMessage { get; set; }
        public int? PointsAwarded { get; set; }
        public string PermitNumber { get; set; }
        public string ClassNumber { get; set; }
        public string FormColor { get; set; }
        public string TextColor { get; set; }
        public string PromotionalCode { get; set; }
        public int? PrimaryPrizeCount { get; set; }
        public int? SecondaryPrizeCount { get; set; }
        public int? ConsolationPrizeCount { get; set; }
        public int? TotalEntries { get; set; }
        public int? PrimaryPrizeBalCount { get; set; }
        public int? SecondaryPrizeBalCount { get; set; }
        public int? ConsolationPrizeBalCount { get; set; }
        public int? TotalBalCount { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string UnSuccessfulImage { get; set; }
        public string PrimaryPrizeImage { get; set; }
        public string SecondaryPrizeImage { get; set; }
        public string ConsolationPrizeImage { get; set; }
        public string ExpiryText { get; set; }
        public int? OnceIn { get; set; }
        public int? IsReleased { get; set; }
        public int? IsPrizeClosed { get; set; }
        public int? ReferaFriend { get; set; }
        public string CurrentInterval { get; set; }
        public int? IntervalId { get; set; }
        public string CustomTagIds { get; set; }
        public long? GroupId { get; set; }
        public long? QRlinkedId { get; set; }
        public int? IsArchive { get; set; }
    }
    public class PlayGameRequest
    {
        public int GameId { get; set; }
        public long MemberId { get; set; }
        public int? AttemptNumber { get; set; }
    }
    public class AddUpdateBrandGameRequest
    {
        public int BrandGameID { get; set; }

        [Display(Name = "Game Name")]
        public string? BrandGameName { get; set; }

        [Display(Name = "Game Subtitle")]
        public string? BrandGameTitle { get; set; }

        public IFormFile? BrandGameImageFile { get; set; }

        public int? UserGroupId { get; set; }
        public int? BusinessId { get; set; }

        [Display(Name = "Description")]
        public string? BrandGameDesc { get; set; }

        public string? ConditionsApply { get; set; }
        public int? GameClassificationID { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? DateStart { get; set; }

        [Display(Name = "End Date")]
        public DateTime? DateEnd { get; set; }

        public int PanelCount { get; set; } = 1;

        public int PanelOpeningLimit { get; set; } = 1;

        public int ChanceCount { get; set; } = 1;

        [Display(Name = "Destination URL")]
        public string? DestinationUrl { get; set; }

        public int Status { get; set; } = 1;
        public int? PrimaryWinImageId { get; set; }
        public int? SecondaryWinImageId { get; set; }
        public int? ConsolationImageId { get; set; }
        public int? ScratchCoverImageId { get; set; }
        public string? QRImagePath { get; set; }
        public int? LimitCount { get; set; }

        [Display(Name = "Primary Offer Label")]
        public string? PrimaryOfferText { get; set; }

        [Display(Name = "Secondary Offer Label")]
        public string? OfferText { get; set; }

        [Display(Name = "Primary Success Message")]
        public string? PrimaryWinMessage { get; set; }

        [Display(Name = "Secondary Success Message")]
        public string? SecondaryWinMessage { get; set; }

        [Display(Name = "Consolation Message")]
        public string? ConsolationMessage { get; set; }

        [Display(Name = "Points Awarded")]
        public int? PointsAwarded { get; set; } = 0;

        public string? PermitNumber { get; set; }
        public string? ClassNumber { get; set; }
        public string? FormColor { get; set; }
        public string? TextColor { get; set; }
        public string? PromotionalCode { get; set; }

        [Display(Name = "Primary Prize Count")]
        public int? PrimaryPrizeCount { get; set; } = 0;

        [Display(Name = "Secondary Prize Count")]
        public int? SecondaryPrizeCount { get; set; } = 0;

        [Display(Name = "Consolation Prize Count")]
        public int? ConsolationPrizeCount { get; set; } = 0;

        public int? TotalEntries { get; set; } = 0;
        public long? PrimaryPrizePromotionId { get; set; } = 0;
        public long? SecondaryPrizePromotionId { get; set; } = 0;
        public long? ConsolationPrizePromotionId { get; set; } = 0;
        public IFormFile? PrimaryPrizeImageFile { get; set; }
        public IFormFile? SecondaryPrizeImageFile { get; set; }
        public IFormFile? ConsolationPrizeImageFile { get; set; }
        public int? IsPayment { get; set; } = 0;
        public decimal? PaymentAmount { get; set; } = 0;
        public IFormFile? UnSuccessfulImageFile { get; set; }
        public string? ExpiryText { get; set; }

        [Display(Name = "Prize Frequency (1 in X)")]
        public int? OnceIn { get; set; } = 1;

        [Display(Name = "Is Game Released")]
        public int? IsReleased { get; set; } = 0;

        [Display(Name = "Is Prize Distribution Closed")]
        public int? IsPrizeClosed { get; set; } = 0;

        public int? ReferaFriend { get; set; } = 0;
        public string? CurrentInterval { get; set; }
        public int? IntervalId { get; set; }
        public string? CustomTagIds { get; set; }
        public long? GroupId { get; set; }
        public long? QRlinkedId { get; set; }
        public int? IsArchive { get; set; } = 0;
    }

    public class PrizeConsumeResult
    {
        public bool IsConsumed { get; set; }
        public int PrimaryPrizeBalCount { get; set; }
        public int SecondaryPrizeBalCount { get; set; }
        public int ConsolationPrizeBalCount { get; set; }
        public int TotalEntries { get; set; }
    }

    public class PrizeDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string GameType { get; set; } // e.g., "ScratchAndWin", "SpinAndWin"
        public int GameId { get; set; }
        public string PrizeType { get; set; } // e.g., "Primary", "Secondary", "Consolation", "Section"
    }
}
