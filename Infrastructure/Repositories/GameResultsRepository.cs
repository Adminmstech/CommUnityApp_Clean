using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.AspNet.SignalR.Infrastructure;
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
    public class GameResultsRepository : IGameResultsRepository
    {
        private readonly IConfiguration _configuration;

        public GameResultsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(IEnumerable<dynamic> Data, int Total)> GetGamePlayMembers(int page, int size, string search)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi = await con.QueryMultipleAsync(  
                "sp_GetBrandGamePlayMembers",
                new
                {
                    PageNumber = page,
                    PageSize = size,
                    Search = string.IsNullOrEmpty(search) ? null : search
                },
                commandType: CommandType.StoredProcedure);

            var data = await multi.ReadAsync();   

            var totalObj = await multi.ReadFirstOrDefaultAsync<dynamic>();  

            int total = totalObj?.TotalCount ?? 0;

            return (data, total);
        }

        public async Task<(IEnumerable<dynamic> Data, int Total)> GetSpinGameResults(int page, int size, string search)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi = await con.QueryMultipleAsync(
                "sp_GetSpinGameResults",
                new
                {
                    PageNumber = page,
                    PageSize = size,
                    Search = string.IsNullOrEmpty(search) ? null : search
                },
                commandType: CommandType.StoredProcedure);

            var data = await multi.ReadAsync();
            var totalObj = await multi.ReadFirstOrDefaultAsync<dynamic>();

            int total = totalObj?.TotalCount ?? 0;

            return (data, total);
        }

        public async Task<bool> AssignPrize(AssignPrizeModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                var result = await con.ExecuteAsync(
                    "sp_AssignPrize",
                    new
                    {
                        model.MemberId,
                        model.Address,
                        model.DeliveryType
                    },
                    commandType: CommandType.StoredProcedure);

                return result > 0;
            }
        }

        public async Task<QuizRankingResult> GetQuizRankings(string? quizType, int? quizId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
;WITH QuizResults AS
(
    SELECT
        'Smart Quiz' AS QuizType,
        S.QuizId,
        S.UserId,
        CONCAT(U.FirstName, ' ', ISNULL(U.LastName, '')) AS FullName,
        U.Email,
        S.CorrectAnswerCount,
        S.AnsweredCount,
        S.Duration,
        S.EndTime,
        S.IsFinished
    FROM SmartQuizResults S
    INNER JOIN Users U ON U.UserId = S.UserId

    UNION ALL

    SELECT
        'Text Quiz' AS QuizType,
        T.QuizId,
        T.UserId,
        CONCAT(U.FirstName, ' ', ISNULL(U.LastName, '')) AS FullName,
        U.Email,
        T.CorrectAnswerCount,
        T.AnsweredCount,
        T.Duration,
        T.EndTime,
        T.IsFinished
    FROM TextQuizResults T
    INNER JOIN Users U ON U.UserId = T.UserId
),
Ranked AS
(
    SELECT *,
           ROW_NUMBER() OVER
           (
               PARTITION BY QuizType, QuizId
               ORDER BY CorrectAnswerCount DESC,
                        Duration ASC,
                        EndTime ASC
           ) AS RankNo
    FROM QuizResults
    WHERE IsFinished = 1
)
SELECT *
INTO #Ranked
FROM Ranked;

SELECT
    QuizType,
    QuizId,
    UserId,
    FullName,
    Email,
    CorrectAnswerCount,
    AnsweredCount,
    Duration,
    EndTime,
    RankNo,
    RankNo AS WinnerRank
FROM #Ranked
WHERE RankNo = 1
  AND (@QuizType IS NULL OR QuizType = @QuizType)
  AND (@QuizId IS NULL OR QuizId = @QuizId)
ORDER BY QuizType, QuizId;

SELECT
    QuizType,
    QuizId,
    UserId,
    FullName,
    Email,
    CorrectAnswerCount,
    AnsweredCount,
    Duration,
    EndTime,
    RankNo
FROM #Ranked
WHERE (@QuizType IS NULL OR QuizType = @QuizType)
  AND (@QuizId IS NULL OR QuizId = @QuizId)
ORDER BY QuizType, QuizId, RankNo;

DROP TABLE #Ranked;";

            using var multi = await con.QueryMultipleAsync(
                sql,
                new
                {
                    QuizType = string.IsNullOrWhiteSpace(quizType) ? null : quizType,
                    QuizId = quizId
                });

            return new QuizRankingResult
            {
                Winners = (await multi.ReadAsync<QuizRankingWinner>()).ToList(),
                Players = (await multi.ReadAsync<QuizRankingPlayer>()).ToList()
            };
        }

        public async Task<IEnumerable<UserGameHistoryModel>> GetUserGameHistory(Guid userId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync<UserGameHistoryModel>(
                "SP_GetUserGameHistory",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
        }
    }
}
