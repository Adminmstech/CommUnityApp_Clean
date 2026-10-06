using System;

namespace CommUnityApp.Domain.Entities
{
    public class SpinSection
    {
        public int SectionId { get; set; }
        public int GameId { get; set; }
        public int SectionNumber { get; set; }
        public int? Points { get; set; }
        public int? PromotionId { get; set; }
        public string? PrizeText { get; set; }
        public string? Color { get; set; }
        public string? SectionImage { get; set; }
        public int? Probability { get; set; }
        public int? WinRangeMin { get; set; }
        public int? WinRangeMax { get; set; }
        public int? TotalStock { get; set; }
        public int? AvailableStock { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
