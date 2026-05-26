using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace CommUnityApp.ApplicationCore.Models
{
    public class QuizGameDto
    {
        public int QuizId { get; set; }
        public int BusinessId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? QuizImage { get; set; }
        public int? ConfigId { get; set; }
        public bool IsActive { get; set; }
    }

    public class QuizGameConfigRequest
    {
        public int ConfigId { get; set; } = 0;
        public int MaxAttemptsPerDay { get; set; } = 1;
        public int QuestionsPerQuiz { get; set; } = 10;
        public int PassingScore { get; set; } = 50;
        public int? TimeLimitInSeconds { get; set; }
        public DateTime GameStartDate { get; set; } = DateTime.Now;
        public DateTime GameEndDate { get; set; } = DateTime.Now.AddDays(30);
        public bool IsActive { get; set; } = true;
    }

    public class QuizOptionRequest
    {
        public int OptionId { get; set; } = 0;
        public int QuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class QuizQuestionRequest
    {
        public int QuestionId { get; set; } = 0;
        public int QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? QuestionImage { get; set; }
        public IFormFile? QuestionImageFile { get; set; }
        public int Points { get; set; } = 10;
        public bool IsActive { get; set; } = true;
        public List<QuizOptionRequest> Options { get; set; } = new();
    }

    public class AddUpdateQuizGameRequest
    {
        public int QuizId { get; set; } = 0;
        public int BusinessId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? QuizImage { get; set; }
        public IFormFile? QuizImageFile { get; set; }
        public int? ConfigId { get; set; }
        public int CreatedByAdminId { get; set; }
        public bool IsActive { get; set; } = true;

        public QuizGameConfigRequest Config { get; set; } = new();
        public List<QuizQuestionRequest> Questions { get; set; } = new();
    }

    public class QuizSessionResponse
    {
        public int SessionId { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
    }

    public class QuizSubmitResponse
    {
        public bool IsCorrect { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
    }

    public class QuizResultDto
    {
        public int ResultId { get; set; }
        public Guid UserId { get; set; }
        public int QuizId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public bool IsPassed { get; set; }
        public DateTime AttemptDate { get; set; }
    }

    public class StartQuizRequest
    {
        public Guid UserId { get; set; }
        public int QuizId { get; set; }
    }

    public class SubmitQuizAnswerRequest
    {
        public int SessionId { get; set; }
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
    }

    public class CompleteQuizRequest
    {
        public int SessionId { get; set; }
        public int StatusId { get; set; } = 2; // Default to 2 (Completed)
    }
}
