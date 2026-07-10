using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class TextQuizRepository : ITextQuizRepository
    {
        private readonly IConfiguration _configuration;

        public TextQuizRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection Connection =>
            new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

        public async Task<List<TextQuizEntity>> GetTextQuizList(PagingRequest paging)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();

            parameters.Add("@PageSize", paging.PageSize);
            parameters.Add("@PageIndex", paging.PageIndex);
            parameters.Add("@Searchstr", paging.SearchText);
            parameters.Add("@SortBy", paging.SortBy);

            var result = await connection.QueryAsync<TextQuizEntity>(
                "GetTextQuizList",
                parameters,
                commandType: CommandType.StoredProcedure);

            var quizzes = result.ToList();

            foreach (var item in quizzes)
            {
                item.QuizImagePath = string.IsNullOrEmpty(item.QuizImage)
                    ? string.Empty
                    : $"{_configuration["ImageBaseUrl"]}/TextQuiz/{item.QuizId}/{item.QuizImage}";

                item.QRCodePath = string.IsNullOrEmpty(item.QRCode)
                    ? string.Empty
                    : $"{_configuration["ImageBaseUrl"]}/TextQuiz/{item.QuizId}/{item.QRCode}";
            }

            return quizzes;
        }

        public async Task<TextQuizGameDetailResponse> GetTextQuizDetailsById(int quizId,Guid userId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();
            parameters.Add("@QuizId", quizId);
            parameters.Add("@UserId", userId);

            var result = await connection.QueryFirstOrDefaultAsync<TextQuizGameDetailResponse>(
                "GetTextQuizDetailsById",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result != null)
            {
                result.QuizImagePath = string.IsNullOrEmpty(result.QuizImage)
                    ? string.Empty
                    : $"{_configuration["ImageBaseUrl"]}/TextQuiz/{result.QuizId}/{result.QuizImage}";
            }

            return result ?? new TextQuizGameDetailResponse();
        }

        public async Task<TextQuizQuestionAndAnswer> GetTextQuizByGameId(int quizId,Guid userId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();
            parameters.Add("@QuizId", quizId);
            parameters.Add("@UserId", userId);

            using var multi = await connection.QueryMultipleAsync(
                "GetTextQuizQandAByQuizId",
                parameters,
                commandType: CommandType.StoredProcedure);

            var quizDetails = await multi.ReadFirstOrDefaultAsync<TextQuizEntity>();

            var questions = (await multi.ReadAsync<TextQuizQuestion>()).ToList();

            var answers = (await multi.ReadAsync<TextQuizAnswer>()).ToList();

            if (quizDetails != null)
            {
                quizDetails.QuizImagePath = string.IsNullOrEmpty(quizDetails.QuizImage)
                    ? string.Empty
                    : $"{_configuration["ImageBaseUrl"]}/TextQuiz/{quizDetails.QuizId}/{quizDetails.QuizImage}";
            }

            foreach (var question in questions)
            {
                question.Answers = answers
                    .Where(a => a.QuestionNumber == question.QuestionNum)
                    .ToList();
            }

            return new TextQuizQuestionAndAnswer
            {
                TextQuizDetails = quizDetails,
                Questions = questions
            };
        }

        public async Task<TextQuizResultEntity> InsertUserTextQuizAnswers(UserTextQuizAnswers model)
        {
            using var connection = Connection;

            var table = GetAnswerDataTable(model.Answers);

            var parameters = new DynamicParameters();

            parameters.Add("@Answers",
                table.AsTableValuedParameter("dbo.CustAnswersDataTbl"));

            parameters.Add("@QuizId", model.QuizId);
            parameters.Add("@UserId", model.UserId);
            parameters.Add("@Duration", model.Duration);
            parameters.Add("@CorrectAnsweredCount", model.CorrectAnsweredCount);
            parameters.Add("@AnsweredCount", model.AnsweredCount);

            var result = await connection.QueryFirstOrDefaultAsync<TextQuizResultEntity>(
                "InsertUserTextQuizAllAnswers",
                parameters,
                commandType: CommandType.StoredProcedure);
            if (result != null)
            {
                var wallet = await connection.QueryFirstOrDefaultAsync<TextQuizResultEntity>(
                    "SP_AddTextQuizCoins",
                    new
                    {
                        model.UserId,
                        model.QuizId,
                        result.QuizResultId,
                        result.CorrectAnswerCount
                    },
                    commandType: CommandType.StoredProcedure);

                if (wallet != null)
                {
                    result.CoinsEarned = wallet.CoinsEarned;
                    result.WalletBalance = wallet.WalletBalance;
                }
            }
            return result ?? new TextQuizResultEntity();
        }

        private DataTable GetAnswerDataTable(List<UserAnswer> answers)
        {
            DataTable table = new DataTable();

            table.Columns.Add("QuestionId", typeof(int));
            table.Columns.Add("QuestionNumber", typeof(short));
            table.Columns.Add("AnswerId", typeof(int));
            table.Columns.Add("AnswerNumber", typeof(short));
            table.Columns.Add("IsCorrect", typeof(int));

            foreach (var item in answers)
            {
                table.Rows.Add(
                    item.QuestionId,
                    item.QuestionNumber,
                    item.AnswerId,
                    item.AnswerNumber,
                    item.IsCorrect);
            }

            return table;
        }


        public async Task<List<TextQuizResultEntity>> GetTextQuizResult(PagingRequest paging,int quizId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();

            parameters.Add("@PageSize", paging.PageSize);
            parameters.Add("@PageIndex", paging.PageIndex);
            parameters.Add("@Searchstr", paging.SearchText);
            parameters.Add("@SortBy", paging.SortBy);
            parameters.Add("@QuizId", quizId);

            var result = await connection.QueryAsync<TextQuizResultEntity>(
                "GetTextQuizResult",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<TextQuizResultEntity>> GetUserQuizResult(int quizId,Guid userId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();

            parameters.Add("@QuizId", quizId);
            parameters.Add("@UserId", userId);

            var result = await connection.QueryAsync<TextQuizResultEntity>(
                "GetTextQuizResultsByUserId",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
        public async Task<List<TextQuizAnswer>> GetQuizAnswersByQuestionId(int questionNumber,int quizId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();

            parameters.Add("@QuizId", quizId);
            parameters.Add("@QuestionNumber", questionNumber);

            var result = await connection.QueryAsync<TextQuizAnswer>(
                "GetTextQuizAnswersByQuestionId",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<TextQuizResultEntity>> GetTextQuizResultsByUser(Guid userId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);

            var result = await connection.QueryAsync<TextQuizResultEntity>(
                "GetTextQuizResultsByUserId",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<TextQuizQuestionAndAnswer> GetTextQuizStatusByUser( int quizId, Guid userId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();

            parameters.Add("@QuizId", quizId);
            parameters.Add("@UserId", userId);

            using var multi = await connection.QueryMultipleAsync(
                "GetTextQuizStatusByUser",
                parameters,
                commandType: CommandType.StoredProcedure);

            var quiz = await multi.ReadFirstOrDefaultAsync<TextQuizEntity>();

            var questions = (await multi.ReadAsync<TextQuizQuestion>()).ToList();

            var answers = (await multi.ReadAsync<TextQuizAnswer>()).ToList();

            if (quiz != null)
            {
                quiz.QuizImagePath = string.IsNullOrWhiteSpace(quiz.QuizImage)
                    ? string.Empty
                    : $"{_configuration["ImageBaseUrl"]}/TextQuiz/{quiz.QuizId}/{quiz.QuizImage}";
            }

            foreach (var question in questions)
            {
                question.Answers = answers
                    .Where(a => a.QuestionNumber == question.QuestionNum)
                    .ToList();
            }

            return new TextQuizQuestionAndAnswer
            {
                TextQuizDetails = quiz,
                Questions = questions
            };
        }

        public async Task<List<TextQuizQuestion>> GetTextQuizQuestionsByGameId(int quizId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();
            parameters.Add("@QuizId", quizId);

            var result = await connection.QueryAsync<TextQuizQuestion>(
                "GetTextQuizQuestionsByGameId",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<TextQuizAnswer>> GetTextQuizQuestionAnswersByGameId( int questionNumber, int quizId)
        {
            using var connection = Connection;

            var parameters = new DynamicParameters();

            parameters.Add("@QuizId", quizId);
            parameters.Add("@QuestionNum", questionNumber);

            var result = await connection.QueryAsync<TextQuizAnswer>(
                "GetTextQuizQuestionAnsByGameId",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
    }

}
