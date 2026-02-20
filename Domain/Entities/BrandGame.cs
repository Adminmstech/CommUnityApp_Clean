using System;

namespace CommUnityApp.Domain.Entities
{
    public class BrandGame
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
        public int? PrimaryWinImageId { get; set; }
        public int? SecondaryWinImageId { get; set; }
        public int? ConsolationImageId { get; set; }
        public int? ScratchCoverImageId { get; set; }
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
        public long? PrimaryPrizePromotionId { get; set; }
        public long? SecondaryPrizePromotionId { get; set; }
        public long? ConsolationPrizePromotionId { get; set; }
        public int? QRScanCount { get; set; }
        public int? SMSCount { get; set; }
        public int? IsPayment { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string UnSuccessfulImage { get; set; }
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
}
