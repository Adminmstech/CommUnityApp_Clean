using System;

namespace CommUnityApp.Domain.Entities
{
    public class QuizQuestion
    {
        public int QuestionId { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? QuestionImage { get; set; }
        public int Points { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
