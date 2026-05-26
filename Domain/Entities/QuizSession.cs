using System;

namespace CommUnityApp.Domain.Entities
{
    public class QuizSession
    {
        public int SessionId { get; set; }
        public Guid UserId { get; set; }
        public int QuizId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int StatusId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
