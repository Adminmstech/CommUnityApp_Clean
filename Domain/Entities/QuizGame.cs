using System;

namespace CommUnityApp.Domain.Entities
{
    public class QuizGame
    {
        public int QuizId { get; set; }
        public int BusinessId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? QuizImage { get; set; }
        public int? ConfigId { get; set; }
        public int CreatedByAdminId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
