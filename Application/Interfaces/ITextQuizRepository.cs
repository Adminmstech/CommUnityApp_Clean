using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface ITextQuizRepository
    {
        Task<List<TextQuizEntity>> GetTextQuizList(PagingRequest paging);
        Task<TextQuizGameDetailResponse> GetTextQuizDetailsById(int quizId, Guid userId);
        Task<TextQuizQuestionAndAnswer> GetTextQuizByGameId(int quizId, Guid userId);

        Task<TextQuizResultEntity> InsertUserTextQuizAnswers(UserTextQuizAnswers model);
        Task<List<TextQuizResultEntity>> GetTextQuizResult(PagingRequest paging,int quizId);

        Task<List<TextQuizResultEntity>> GetUserQuizResult(int quizId,Guid userId);
        Task<List<TextQuizAnswer>> GetQuizAnswersByQuestionId(int questionNumber,int quizId);
        Task<List<TextQuizResultEntity>> GetTextQuizResultsByUser(Guid userId);
        Task<TextQuizQuestionAndAnswer> GetTextQuizStatusByUser(int quizId,Guid userId);
        Task<List<TextQuizQuestion>> GetTextQuizQuestionsByGameId(int quizId);

        Task<List<TextQuizAnswer>> GetTextQuizQuestionAnswersByGameId( int questionNumber, int quizId);

        Task<List<GameSponsor>> GetGameSponsors(int gameTypeId,int gameId);

        Task<GameSponsorDetails?> GetGameSponsorDetailsById(int sponsorId);

        Task<SaveResult> SaveTextQuiz(SaveTextQuizModel model);

        Task<IEnumerable<TextQuizGameModel>> GetAllTextQuiz();

        Task<TextQuizDetailsModel> GetTextQuizById(long quizId);

        Task<SaveResult> DeleteTextQuiz(long quizId);
    }
}
