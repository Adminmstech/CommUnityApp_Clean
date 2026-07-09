using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    internal class SmartQuiz
    {
    }
    public class SmartQuizModel
    {
        public int QuizId { get; set; }

        public int OrgId { get; set; }

        public int GroupId { get; set; }

        public string SmartQuizName { get; set; }

        public string SmartQuizCode { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string SmsCode { get; set; }

        public string SmartQuizImage { get; set; }

        public string SmartQuizImagePath { get; set; }

        public string QRCode { get; set; }

        public string QRCodePath { get; set; }

        public string SmartQuizUrl { get; set; }

        public bool IsReferFriend { get; set; }

        public string ShortDescription { get; set; }

        public int Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public long TotalRecords { get; set; }

        public long StartedIn { get; set; }

        public long EndedIn { get; set; }

        public int ResultId { get; set; }

        public bool IsFinished { get; set; }

        public int GameStatus =>
            StartedIn > 0 ? 1 :
            EndedIn > 0 ? 2 : 3;

        public string StartedInText { get; set; }

        public string EndedInText { get; set; }

        public string StartDateString { get; set; }

        public string EndDateString { get; set; }
    }
    public class SmartQuizListModel
    {
        public int QuizId { get; set; }

        public string SmartQuizName { get; set; }

        public string SmartQuizImage { get; set; }

        public string SmartQuizImagePath { get; set; }

        public string ShortDescription { get; set; }

        public int Status { get; set; }
    }
    public class SmartQuizQuestionModel
    {
        public int SmartQuizQuestionId { get; set; }

        public int QuizId { get; set; }

        public int QuestionId { get; set; }

        public int SurveyId { get; set; }

        public string Question { get; set; }

        public int QuestionNum { get; set; }

        public int CorrectAnswerId { get; set; }

        public bool IsActive { get; set; }

        public int CorrectAnswer { get; set; }

        public int IsQuestionAvailable { get; set; }

        public List<SmartQuizAnswerModel> Answers { get; set; } = new();
    }
    public class SmartQuizAnswerModel
    {
        public int SmartQuizAnswerId { get; set; }

        public int SmartQuizQuestionId { get; set; }

        public int QuizId { get; set; }

        public int QuestionNumber { get; set; }

        public int AnswerNumber { get; set; }

        public string AnswerImage { get; set; }

        public string AnswerImagePath { get; set; }
    }

    public class SmartQuizQuestionAndAnswerModel
    {
        public SmartQuizModel SmartQuizDetails { get; set; }

        public List<SmartQuizQuestionModel> Questions { get; set; } = new();
    }
    public class SmartQuizResultModel
    {
        public int SmartQuizResultId { get; set; }

        public Guid UserId { get; set; }

        public int QuizId { get; set; }

        public int AnsweredCount { get; set; }

        public int CorrectAnswerCount { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public long Duration { get; set; }

        public string DurationString { get; set; }

        public string CustomerName { get; set; }

        public string Mobile { get; set; }

        public string Country { get; set; }

        public string QuizName { get; set; }

        public int TotalRecords { get; set; }

        public long Rank { get; set; }

        public bool IsSelf { get; set; }
    }

    public class SubmitSmartQuizRequest
    {
        public int QuizId { get; set; }

        public Guid UserId { get; set; }

        public int Duration { get; set; }

        public int CorrectAnsweredCount { get; set; }

        public int AnsweredCount { get; set; }

        public List<CustomerAnswerModel> Answers { get; set; } = new();
    }
    public class CustomerAnswerModel
    {
        public int QuestionId { get; set; }

        public short QuestionNumber { get; set; }

        public int AnswerId { get; set; }

        public short AnswerNumber { get; set; }

        public bool IsCorrect { get; set; }
    }

    public class SubmitSmartQuizAnswerRequest
    {
        public int QuizId { get; set; }
        public Guid UserId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public bool IsCorrect { get; set; }
        public int Duration { get; set; }
    }


}
