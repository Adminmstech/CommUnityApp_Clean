using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IQuizGameRepository
    {
        Task<BaseResponse> AddUpdateQuizGameAsync(AddUpdateQuizGameRequest model);
        Task<QuizGameDto?> GetQuizGameByIdAsync(int quizId);
        Task<IEnumerable<QuizGameDto>> GetAllQuizGamesAsync();
        Task<IEnumerable<QuizGameDto>> GetQuizGamesByBusinessAsync(int businessId);
        Task<BaseResponse> DeleteQuizGameAsync(int quizId);
        
        Task<QuizGameConfigRequest?> GetConfigByIdAsync(int configId);
        Task<IEnumerable<QuizQuestionRequest>> GetQuestionsByQuizIdAsync(int quizId);
        Task<IEnumerable<QuizOptionRequest>> GetOptionsByQuestionIdAsync(int questionId);

        Task<QuizSessionResponse> StartQuizSessionAsync(Guid userId, int quizId);
        Task<QuizSubmitResponse> SubmitQuizResponseAsync(int sessionId, int questionId, int optionId);
        Task<BaseResponse> CompleteQuizSessionAsync(int sessionId, int statusId);
        Task<IEnumerable<QuizResultDto>> GetQuizResultsAsync(int? quizId, Guid? userId);
    }
}
