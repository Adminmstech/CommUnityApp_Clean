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
    public class SmartQuizRepository : ISmartQuizRepository
    {
        private readonly IConfiguration _configuration;

        public SmartQuizRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<List<SmartQuizListModel>> GetSmartQuizList()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = (await connection.QueryAsync<SmartQuizListModel>(
                "GetSmartQuizList",
                commandType: CommandType.StoredProcedure))
                .ToList();

            foreach (var item in result)
            {
                if (!string.IsNullOrEmpty(item.SmartQuizImage))
                {
                    item.SmartQuizImagePath =
     $"{_configuration["ImageBaseUrl"]}/Uploads/SmartQuiz/{item.QuizId}/{item.SmartQuizImage}";
                }
            }

            return result;
        }

        public async Task<SmartQuizQuestionAndAnswerModel> GetSmartQuizById(
     int quizId,
     Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using var multi = await connection.QueryMultipleAsync(
                "GetSmartQandABySmartQuizId",
                new
                {
                    QuizId = quizId,
                    UserId = userId
                },
                commandType: CommandType.StoredProcedure);

            var response = new SmartQuizQuestionAndAnswerModel();

            response.SmartQuizDetails =
                await multi.ReadFirstOrDefaultAsync<SmartQuizModel>();

            response.Questions =
                (await multi.ReadAsync<SmartQuizQuestionModel>()).ToList();

            var answers =
                (await multi.ReadAsync<SmartQuizAnswerModel>()).ToList();

            foreach (var answer in answers)
            {
                if (!string.IsNullOrWhiteSpace(answer.AnswerImage))
                {
                    answer.AnswerImagePath =
                        $"{_configuration["ImageBaseUrl"]}/Uploads/SmartQuizAnswers/{answer.QuizId}/{answer.QuestionNumber}/{answer.AnswerImage}";
                }
            }

            foreach (var question in response.Questions)
            {
                question.Answers = answers
                    .Where(x => x.QuestionNumber == question.QuestionNum)
                    .ToList();
            }

            if (response.SmartQuizDetails != null)
            {
                response.SmartQuizDetails.SmartQuizImagePath =
                    $"{_configuration["ImageBaseUrl"]}/Uploads/SmartQuiz/{response.SmartQuizDetails.QuizId}/{response.SmartQuizDetails.SmartQuizImage}";

                response.SmartQuizDetails.QRCodePath =
                    $"{_configuration["ImageBaseUrl"]}/Uploads/SmartQuiz/{response.SmartQuizDetails.QuizId}/{response.SmartQuizDetails.QRCode}";
            }

            return response;
        }

        public async Task<SmartQuizQuestionAndAnswerModel>GetSmartQuizStatusByCustomer(int quizId,Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using var multi =
                await connection.QueryMultipleAsync(
                    "GetSmartQuizStatusByCustomer",
                    new
                    {
                        QuizId = quizId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

            var response = new SmartQuizQuestionAndAnswerModel();

            response.SmartQuizDetails =
                await multi.ReadFirstOrDefaultAsync<SmartQuizModel>();

            response.Questions =
                (await multi.ReadAsync<SmartQuizQuestionModel>())
                .ToList();

            var answers =
                (await multi.ReadAsync<SmartQuizAnswerModel>())
                .ToList();

            foreach (var question in response.Questions)
            {
                question.Answers =
                    answers
                    .Where(x => x.QuestionNumber == question.QuestionNum)
                    .ToList();
            }

            return response;
        }
        public async Task<List<SmartQuizAnswerModel>> GetSmartQuizAnswersByQuestionId(
     int questionNumber,
     int quizId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = (await connection.QueryAsync<SmartQuizAnswerModel>(
                "GetSmartQuizAnswersByQuestionId",
                new
                {
                    QuizId = quizId,
                    QuestionNumber = questionNumber
                },
                commandType: CommandType.StoredProcedure))
                .ToList();

            foreach (var answer in result)
            {
                if (!string.IsNullOrWhiteSpace(answer.AnswerImage))
                {
                    answer.AnswerImagePath =
                        $"{_configuration["ImageBaseUrl"]}/Uploads/SmartQuizAnswers/{quizId}/{questionNumber}/{answer.AnswerImage}";
                }
            }

            return result;
        }

        public async Task<SmartQuizQuestionModel> InsertCustomerSmartQuizAnswer( SubmitSmartQuizAnswerRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result =
                await connection.QueryFirstOrDefaultAsync<SmartQuizQuestionModel>(
                    "InsertMemberSmartQuizAnswer",
                    new
                    {
                        QuizId = request.QuizId,
                        UserId = request.UserId,
                        QuestionId = request.QuestionId,
                        AnswerId = request.AnswerId,
                        IsCorrect = request.IsCorrect,
                        Duration = request.Duration
                    },
                    commandType: CommandType.StoredProcedure);

            return result;
        }
        public async Task<List<SmartQuizResultModel>> GetCustomerSmartQuizResult(int quizId,Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result =
                (await connection.QueryAsync<SmartQuizResultModel>(
                    "GetMemberSmartQuizResults",
                    new
                    {
                        QuizId = quizId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure))
                .ToList();

            return result;
        }

        public async Task<List<SmartQuizResultModel>>GetSmartQuizResultsByUserId(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result =
                (await connection.QueryAsync<SmartQuizResultModel>(
                    "GetMemberSmartQuizResultsByMemId",
                    new
                    {
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure))
                .ToList();

            return result;
        }

        public async Task<SmartQuizResultModel> InsertSmartQuizCustomerAllAnswers(SubmitSmartQuizRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var table = CreateAnswerDataTable(request.Answers);

            var parameters = new DynamicParameters();

            parameters.Add(
                "@Answers",
                table.AsTableValuedParameter("dbo.CustAnswersDatatbl"));

            parameters.Add("@QuizId", request.QuizId);
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@Duration", request.Duration);
            parameters.Add("@CorrectAnsweredCount", request.CorrectAnsweredCount);
            parameters.Add("@AnsweredCount", request.AnsweredCount);

            return await connection.QueryFirstOrDefaultAsync<SmartQuizResultModel>(
                "InsertMemberSmartQuizAllAnswers",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        private DataTable CreateAnswerDataTable(
    List<CustomerAnswerModel> answers)
        {
            var table = new DataTable();

            table.Columns.Add("QuestionId", typeof(int));
            table.Columns.Add("QuestionNumber", typeof(short));
            table.Columns.Add("AnswerId", typeof(int));
            table.Columns.Add("AnswerNumber", typeof(short));
            table.Columns.Add("IsCorrect", typeof(bool));

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


        public async Task AddSmartQuizRewardCoinsAsync(Guid userId, int coins, int quizId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "SP_AddSmartQuizRewardCoins",
                new
                {
                    UserId = userId,
                    Coins = coins,
                    QuizId = quizId
                },
                commandType: CommandType.StoredProcedure);
        }

    }


}
