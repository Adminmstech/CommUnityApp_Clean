using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    internal class TextQuiz
    {


    }

    public class TextQuizEntity
    {
        public int QuizId { get; set; }

        public int OrgId { get; set; }

        public int GroupId { get; set; }

        public string QuizName { get; set; } = string.Empty;

        public string QuizCode { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string SmsCode { get; set; } = string.Empty;

        public string QuizImage { get; set; } = string.Empty;

        public string QuizImagePath { get; set; } = string.Empty;

        public string QRCode { get; set; } = string.Empty;

        public string QRCodePath { get; set; } = string.Empty;

        public bool IsReferFriend { get; set; }

        public string ShortDescription { get; set; } = string.Empty;

        public int Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public long StartedIn { get; set; }

        public long EndedIn { get; set; }

        public int IsFinished { get; set; }

        public int GameStatus
        {
            get
            {
                if (StartedIn > 0)
                    return 1;

                if (EndedIn > 0)
                    return 2;

                return 3;
            }
        }
    }

    public class TextQuizGameDetailResponse
    {
        public int QuizId { get; set; }

        public string QuizName { get; set; } = string.Empty;

        public string QuizImage { get; set; } = string.Empty;

        public string QuizImagePath { get; set; } = string.Empty;

        public int Status { get; set; }

        public string ShortDescription { get; set; } = string.Empty;
    }

    public class PagingRequest
    {
        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public string? SearchText { get; set; } = string.Empty;

        public int SortBy { get; set; }

        public string? FromDate { get; set; }

        public string? ToDate { get; set; }
    }


    public class TextQuizQuestion
    {
        public int QuizQuestionId { get; set; }

        public int QuizId { get; set; }

        public string Question { get; set; } = string.Empty;

        public int QuestionNum { get; set; }

        public int CorrectAnswerId { get; set; }

        public int IsActive { get; set; }

        public List<TextQuizAnswer> Answers { get; set; } = new();
    }

    public class TextQuizAnswer
    {
        public int QuizAnswerId { get; set; }

        public int QuestionNumber { get; set; }

        public int QuizId { get; set; }

        public int AnswerNumber { get; set; }

        public string Answer { get; set; } = string.Empty;
    }

    public class TextQuizResultEntity
    {
        public int QuizResultId { get; set; }

        public Guid UserId { get; set; }

        public int QuizId { get; set; }

        public int AnsweredCount { get; set; }

        public int CorrectAnswerCount { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public long Duration { get; set; }

        public string QuizName { get; set; } = string.Empty;

        public int Rank { get; set; }

        public bool IsSelf { get; set; }

        public bool IsFinished { get; set; }

        public int CoinsEarned { get; set; }

        public int WalletBalance { get; set; }
    }

    public class UserTextQuizAnswers
    {
        public Guid UserId { get; set; }

        public int QuizId { get; set; }

        public int Duration { get; set; }

        public int CorrectAnsweredCount { get; set; }

        public int AnsweredCount { get; set; }

        public List<UserAnswer> Answers { get; set; } = new();
    }

    public class UserAnswer
    {
        public int QuestionId { get; set; }

        public short QuestionNumber { get; set; }

        public int AnswerId { get; set; }

        public short AnswerNumber { get; set; }

        public int IsCorrect { get; set; }
    }

    public class TextQuizQuestionAndAnswer
    {
        public TextQuizEntity? TextQuizDetails { get; set; }

        public List<TextQuizQuestion> Questions { get; set; } = new();

        public List<TextQuizResultEntity> Results { get; set; } = new();

        public bool IsFinished { get; set; }

        public string StatusMessage { get; set; } = string.Empty;
    }

    public class GameSponsor
    {
        public int SponsorId { get; set; }

        public int GameTypeId { get; set; }

        public int GameId { get; set; }

        public string SponsorName { get; set; }

        public string SponsorDescription { get; set; }

        public string SponsorImage { get; set; }

        public string SponsorImagePath { get; set; } = string.Empty;


        public string SponsorWebsite { get; set; }
        public string VideoUrl { get; set; }

        public int DisplayOrder { get; set; }
    }

    public class GameSponsorDetails
    {
        public int SponsorId { get; set; }

        public int GameTypeId { get; set; }

        public int GameId { get; set; }

        public string SponsorName { get; set; } = string.Empty;

        public string SponsorDescription { get; set; } = string.Empty;

        public string SponsorImage { get; set; } = string.Empty;

        public string SponsorImagePath { get; set; } = string.Empty;

        public string VideoLink { get; set; } = string.Empty;

        public string SponsorWebsite { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
