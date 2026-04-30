using System;

namespace CommUnityApp.Domain.Entities
{
    public class QuizResult
    {
        public int ResultId { get; set; }
        public Guid UserId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public bool IsPassed { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}
