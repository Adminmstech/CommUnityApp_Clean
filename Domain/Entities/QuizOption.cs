using System;

namespace CommUnityApp.Domain.Entities
{
    public class QuizOption
    {
        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
