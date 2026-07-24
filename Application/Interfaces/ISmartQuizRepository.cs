using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
   public interface ISmartQuizRepository
    {

        Task<List<SmartQuizListModel>> GetSmartQuizList();

        Task<SmartQuizQuestionAndAnswerModel> GetSmartQuizById( int quizId, Guid userId);

        Task<SmartQuizQuestionAndAnswerModel> GetSmartQuizStatusByCustomer(int quizId,Guid userId);

        Task<List<SmartQuizAnswerModel>> GetSmartQuizAnswersByQuestionId( int questionNumber,int quizId);

        Task<SmartQuizQuestionModel> InsertCustomerSmartQuizAnswer(SubmitSmartQuizAnswerRequest request);
        Task<List<SmartQuizResultModel>> GetCustomerSmartQuizResult(int quizId,Guid userId);

        Task<List<SmartQuizResultModel>> GetSmartQuizResultsByUserId( Guid userId);

        Task<SmartQuizResultModel> InsertSmartQuizCustomerAllAnswers(SubmitSmartQuizRequest request);

        Task AddSmartQuizRewardCoinsAsync(Guid userId, int coins, int quizId);

        Task<SaveResult> SaveSmartQuiz(SaveSmartQuizModel model);

        Task<IEnumerable<SmartQuizGameModel>> GetAllSmartQuiz();

        Task<SmartQuizDetailsModel> GetSmartQuizById(long quizId);

        Task<SaveResult> DeleteSmartQuiz(long quizId);
    }
}
