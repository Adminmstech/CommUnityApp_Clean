using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class QuizGameRepository : IQuizGameRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly IDapperWrapper _dapper;

        public QuizGameRepository(Func<IDbConnection> connectionFactory, IDapperWrapper dapper)
        {
            _connectionFactory = connectionFactory;
            _dapper = dapper;
        }

        private IDbConnection Connection => _connectionFactory();

        public async Task<BaseResponse> AddUpdateQuizGameAsync(AddUpdateQuizGameRequest model)
        {
            using var con = Connection;
            con.Open();
            using var transaction = con.BeginTransaction();

            try
            {
                // 1. Add/Update QuizGameConfig
                object configParameters;
                if (model.Config.ConfigId == 0)
                {
                    configParameters = new
                    {
                        MaxAttemptsPerDay = model.Config.MaxAttemptsPerDay,
                        QuestionsPerQuiz = model.Config.QuestionsPerQuiz,
                        PassingScore = model.Config.PassingScore,
                        TimeLimitInSeconds = model.Config.TimeLimitInSeconds,
                        GameStartDate = model.Config.GameStartDate,
                        GameEndDate = model.Config.GameEndDate,
                        IsActive = model.Config.IsActive
                    };
                }
                else
                {
                    configParameters = new
                    {
                        ConfigId = model.Config.ConfigId,
                        MaxAttemptsPerDay = model.Config.MaxAttemptsPerDay,
                        QuestionsPerQuiz = model.Config.QuestionsPerQuiz,
                        PassingScore = model.Config.PassingScore,
                        TimeLimitInSeconds = model.Config.TimeLimitInSeconds,
                        GameStartDate = model.Config.GameStartDate,
                        GameEndDate = model.Config.GameEndDate,
                        IsActive = model.Config.IsActive
                    };
                }

                var configResult = await _dapper.QueryFirstOrDefaultAsync<BaseResponse>(
                    con,
                    model.Config.ConfigId == 0 ? "sp_AddQuizGameConfig" : "sp_UpdateQuizGameConfig",
                    configParameters,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure
                );

                if (configResult == null || configResult.ResultId <= 0)
                {
                    transaction.Rollback();
                    return configResult ?? new BaseResponse { ResultId = 0, ResultMessage = "Failed to add/update Quiz Configuration." };
                }

                int configId = configResult.ResultId;
                model.ConfigId = configId;

                // 2. Add/Update QuizGame
                object quizParameters;
                if (model.QuizId == 0)
                {
                    quizParameters = new
                    {
                        BusinessId = model.BusinessId,
                        GameName = model.GameName,
                        Description = model.Description,
                        QuizImage = model.QuizImage,
                        ConfigId = configId,
                        CreatedByAdminId = model.CreatedByAdminId,
                        IsActive = model.IsActive
                    };
                }
                else
                {
                    quizParameters = new
                    {
                        QuizId = model.QuizId,
                        BusinessId = model.BusinessId,
                        GameName = model.GameName,
                        Description = model.Description,
                        QuizImage = model.QuizImage,
                        ConfigId = configId,
                        CreatedByAdminId = model.CreatedByAdminId,
                        IsActive = model.IsActive
                    };
                }

                var quizResult = await _dapper.QueryFirstOrDefaultAsync<BaseResponse>(
                    con,
                    model.QuizId == 0 ? "sp_AddQuizGame" : "sp_UpdateQuizGame",
                    quizParameters,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure
                );

                if (quizResult == null || quizResult.ResultId <= 0)
                {
                    transaction.Rollback();
                    return quizResult ?? new BaseResponse { ResultId = 0, ResultMessage = "Failed to add/update Quiz Game." };
                }

                int quizId = quizResult.ResultId;

                // 3. Handle Questions and Options
                if (model.Questions != null && model.Questions.Any())
                {
                    // For a clean update, we might want to soft-delete existing questions/options not in the current list
                    // But for simplicity in this SP-based approach, we'll just add/update what's provided.

                    foreach (var question in model.Questions)
                    {
                        question.QuizId = quizId;
                        object questionParams;
                        if (question.QuestionId == 0)
                        {
                            questionParams = new
                            {
                                question.QuizId,
                                question.QuestionText,
                                question.QuestionImage,
                                question.Points,
                                question.IsActive
                            };
                        }
                        else
                        {
                            questionParams = new
                            {
                                question.QuestionId,
                                question.QuizId,
                                question.QuestionText,
                                question.QuestionImage,
                                question.Points,
                                question.IsActive
                            };
                        }

                        var questionResult = await _dapper.QueryFirstOrDefaultAsync<BaseResponse>(
                            con,
                            question.QuestionId == 0 ? "sp_AddQuizQuestion" : "sp_UpdateQuizQuestion",
                            questionParams,
                            transaction: transaction,
                            commandType: CommandType.StoredProcedure
                        );

                        if (questionResult == null || questionResult.ResultId <= 0)
                        {
                            transaction.Rollback();
                            return questionResult ?? new BaseResponse { ResultId = 0, ResultMessage = "Failed to save quiz question." };
                        }

                        int questionId = questionResult.ResultId;

                        if (question.Options != null && question.Options.Any())
                        {
                            foreach (var option in question.Options)
                            {
                                option.QuestionId = questionId;
                                object optionParams;
                                if (option.OptionId == 0)
                                {
                                    optionParams = new
                                    {
                                        option.QuestionId,
                                        option.OptionText,
                                        option.IsCorrect,
                                        option.IsActive
                                    };
                                }
                                else
                                {
                                    optionParams = new
                                    {
                                        option.OptionId,
                                        option.QuestionId,
                                        option.OptionText,
                                        option.IsCorrect,
                                        option.IsActive
                                    };
                                }

                                var optionResult = await _dapper.QueryFirstOrDefaultAsync<BaseResponse>(
                                    con,
                                    option.OptionId == 0 ? "sp_AddQuizOption" : "sp_UpdateQuizOption",
                                    optionParams,
                                    transaction: transaction,
                                    commandType: CommandType.StoredProcedure
                                );

                                if (optionResult == null || optionResult.ResultId <= 0)
                                {
                                    transaction.Rollback();
                                    return optionResult ?? new BaseResponse { ResultId = 0, ResultMessage = "Failed to save quiz option." };
                                }
                            }
                        }
                    }
                }

                transaction.Commit();
                return quizResult;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new BaseResponse { ResultId = 0, ResultMessage = "Error: " + ex.Message };
            }
        }

        public async Task<QuizGameDto?> GetQuizGameByIdAsync(int quizId)
        {
            using var con = Connection;
            return await _dapper.QueryFirstOrDefaultAsync<QuizGameDto>(con, "SELECT * FROM QuizGame WHERE QuizId = @QuizId", new { QuizId = quizId });
        }

        public async Task<IEnumerable<QuizGameDto>> GetAllQuizGamesAsync()
        {
            using var con = Connection;
            return await _dapper.QueryAsync<QuizGameDto>(con, "SELECT * FROM QuizGame WHERE IsActive = 1");
        }

        public async Task<IEnumerable<QuizGameDto>> GetQuizGamesByBusinessAsync(int businessId)
        {
            using var con = Connection;
            var sql = "SELECT QuizId, BusinessId, GameName, Description, QuizImage, ConfigId, IsActive FROM QuizGame WHERE BusinessId = @BusinessId AND IsActive = 1";
            return await _dapper.QueryAsync<QuizGameDto>(con, sql, new { BusinessId = businessId });
        }

        public async Task<BaseResponse> DeleteQuizGameAsync(int quizId)
        {
            using var con = Connection;
            var result = await _dapper.ExecuteAsync(con, "UPDATE QuizGame SET IsActive = 0 WHERE QuizId = @QuizId", new { QuizId = quizId });
            return new BaseResponse { ResultId = result > 0 ? 1 : 0, ResultMessage = result > 0 ? "Quiz soft-deleted." : "Quiz not found." };
        }

        public async Task<QuizGameConfigRequest?> GetConfigByIdAsync(int configId)
        {
            using var con = Connection;
            return await _dapper.QueryFirstOrDefaultAsync<QuizGameConfigRequest>(con, "SELECT * FROM QuizGameConfiguration WHERE ConfigId = @ConfigId", new { ConfigId = configId });
        }

        public async Task<IEnumerable<QuizQuestionRequest>> GetQuestionsByQuizIdAsync(int quizId)
        {
            using var con = Connection;
            return await _dapper.QueryAsync<QuizQuestionRequest>(con, "SELECT * FROM QuizQuestion WHERE QuizId = @QuizId AND IsActive = 1", new { QuizId = quizId });
        }

        public async Task<IEnumerable<QuizOptionRequest>> GetOptionsByQuestionIdAsync(int questionId)
        {
            using var con = Connection;
            return await _dapper.QueryAsync<QuizOptionRequest>(con, "SELECT * FROM QuizOption WHERE QuestionId = @QuestionId AND IsActive = 1", new { QuestionId = questionId });
        }

        public async Task<QuizSessionResponse> StartQuizSessionAsync(Guid userId, int quizId)
        {
            using var con = Connection;
            return await _dapper.QueryFirstOrDefaultAsync<QuizSessionResponse>(con, "sp_StartQuizSession", new { UserId = userId, QuizId = quizId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<QuizSubmitResponse> SubmitQuizResponseAsync(int sessionId, int questionId, int optionId)
        {
            using var con = Connection;
            return await _dapper.QueryFirstOrDefaultAsync<QuizSubmitResponse>(con, "sp_SubmitQuizResponse", new { SessionId = sessionId, QuestionId = questionId, OptionId = optionId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<BaseResponse> CompleteQuizSessionAsync(int sessionId, int statusId)
        {
            using var con = Connection;
            return await _dapper.QueryFirstOrDefaultAsync<BaseResponse>(con, "sp_CompleteQuizSession", new { SessionId = sessionId, StatusId = statusId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<QuizResultDto>> GetQuizResultsAsync(int? quizId, Guid? userId)
        {
            using var con = Connection;
            var sql = @"SELECT qr.*, qg.GameName 
                        FROM QuizResult qr 
                        INNER JOIN QuizGame qg ON qr.QuizId = qg.QuizId 
                        WHERE 1=1";
            if (quizId.HasValue) sql += " AND qr.QuizId = @QuizId";
            if (userId.HasValue) sql += " AND qr.UserId = @UserId";
            sql += " ORDER BY qr.AttemptDate DESC";

            return await _dapper.QueryAsync<QuizResultDto>(con, sql, new { QuizId = quizId, UserId = userId });
        }
    }
}
