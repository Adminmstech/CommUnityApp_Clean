using System;

namespace CommUnityApp.Domain.Entities
{
    public class GameSpin
    {
        public int SpinId { get; set; }
        public Guid UserId { get; set; }
        public DateTime SpinDate { get; set; }
        public int SelectedSectionId { get; set; }
        public int? PointsAwarded { get; set; }
        public int? PromotionId { get; set; }
    }
}

