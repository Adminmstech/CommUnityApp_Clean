using System;

namespace CommUnityApp.Domain.Entities
{
    public class QuizGameConfiguration
    {
        public int ConfigId { get; set; }
        public int MaxAttemptsPerDay { get; set; }
        public int QuestionsPerQuiz { get; set; }
        public int PassingScore { get; set; }
        public int? TimeLimitInSeconds { get; set; }
        public DateTime GameStartDate { get; set; }
        public DateTime GameEndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
