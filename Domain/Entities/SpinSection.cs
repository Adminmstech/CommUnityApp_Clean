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
    }
}

