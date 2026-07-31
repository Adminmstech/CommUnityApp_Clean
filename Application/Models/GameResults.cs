using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class GameResults
    {
    }

    public class AssignPrizeModel
    {
        public int MemberId { get; set; }
        public string Address { get; set; }
        public string DeliveryType { get; set; }
    }

    public class QuizRankingResult
    {
        public List<QuizRankingWinner> Winners { get; set; } = new();

        public List<QuizRankingPlayer> Players { get; set; } = new();
    }

    public class QuizRankingPlayer
    {
        public string QuizType { get; set; } = string.Empty;

        public int QuizId { get; set; }

        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int CorrectAnswerCount { get; set; }

        public int AnsweredCount { get; set; }

        public long Duration { get; set; }

        public DateTime? EndTime { get; set; }

        public int RankNo { get; set; }
    }

    public class QuizRankingWinner : QuizRankingPlayer
    {
        public int WinnerRank { get; set; }
    }
}
