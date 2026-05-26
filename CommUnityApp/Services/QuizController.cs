using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public QuizController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        [HttpGet("GetActiveQuizzes")]
        public async Task<IActionResult> GetActiveQuizzes(int businessId = 0)
        {
            IEnumerable<QuizGameDto> quizzes;
            if (businessId > 0)
            {
                quizzes = await _unitOfWork.QuizGames.GetQuizGamesByBusinessAsync(businessId);
            }
            else
            {
                quizzes = await _unitOfWork.QuizGames.GetAllQuizGamesAsync();
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
            var result = quizzes.Select(q => new
            {
                q.QuizId,
                q.BusinessId,
                q.GameName,
                q.Description,
                QuizImage = BuildFullImageUrl(baseUrl, q.QuizImage),
                q.IsActive
            });

            return Ok(new { resultId = 1, resultMessage = $"{result.Count()} quiz(zes) found.", quizzes = result });
        }

        [HttpGet("GetQuizDetails")]
        public async Task<IActionResult> GetQuizDetails(int quizId)
        {
            if (quizId <= 0) return BadRequest(new { resultId = 0, resultMessage = "Valid quizId is required." });

            var quiz = await _unitOfWork.QuizGames.GetQuizGameByIdAsync(quizId);
            if (quiz == null) return NotFound(new { resultId = 0, resultMessage = "Quiz not found." });

            var config = await _unitOfWork.QuizGames.GetConfigByIdAsync(quiz.ConfigId ?? 0);
            var questions = await _unitOfWork.QuizGames.GetQuestionsByQuizIdAsync(quizId);
            
            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');

            var detailedQuestions = new List<object>();
            foreach (var q in questions)
            {
                var options = await _unitOfWork.QuizGames.GetOptionsByQuestionIdAsync(q.QuestionId);
                detailedQuestions.Add(new
                {
                    q.QuestionId,
                    q.QuestionText,
                    QuestionImage = BuildFullImageUrl(baseUrl, q.QuestionImage),
                    q.Points,
                    Options = options.Select(o => new
                    {
                        o.OptionId,
                        o.OptionText
                        // We don't send IsCorrect to the client for obvious reasons!
                    })
                });
            }

            return Ok(new
            {
                resultId = 1,
                resultMessage = "Quiz details found.",
                quiz = new
                {
                    quiz.QuizId,
                    quiz.GameName,
                    quiz.Description,
                    QuizImage = BuildFullImageUrl(baseUrl, quiz.QuizImage)
                },
                config,
                questions = detailedQuestions
            });
        }

        [HttpPost("StartSession")]
        public async Task<IActionResult> StartSession([FromBody] StartQuizRequest request)
        {
            if (request == null || request.QuizId <= 0 || request.UserId == Guid.Empty)
            {
                return BadRequest(new { resultId = 0, resultMessage = "Valid quizId and userId are required." });
            }

            var response = await _unitOfWork.QuizGames.StartQuizSessionAsync(request.UserId, request.QuizId);
            if (response.SessionId > 0)
            {
                return Ok(response);
            }
            return BadRequest(new { resultId = 0, resultMessage = response.ResultMessage });
        }

        [HttpPost("SubmitAnswer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitQuizAnswerRequest request)
        {
            if (request == null || request.SessionId <= 0 || request.QuestionId <= 0 || request.OptionId <= 0)
            {
                return BadRequest(new { resultId = 0, resultMessage = "Valid sessionId, questionId, and optionId are required." });
            }

            var response = await _unitOfWork.QuizGames.SubmitQuizResponseAsync(request.SessionId, request.QuestionId, request.OptionId);
            return Ok(response);
        }

        [HttpPost("CompleteSession")]
        public async Task<IActionResult> CompleteSession([FromBody] CompleteQuizRequest request)
        {
            if (request == null || request.SessionId <= 0)
            {
                return BadRequest(new { resultId = 0, resultMessage = "Valid sessionId is required." });
            }

            var response = await _unitOfWork.QuizGames.CompleteQuizSessionAsync(request.SessionId, request.StatusId);
            if (response.ResultId > 0)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("GetMyResults")]
        public async Task<IActionResult> GetMyResults(Guid userId, int? quizId = null)
        {
            if (userId == Guid.Empty) return BadRequest(new { resultId = 0, resultMessage = "Valid userId is required." });

            var results = await _unitOfWork.QuizGames.GetQuizResultsAsync(quizId, userId);
            return Ok(new { resultId = 1, resultMessage = $"{results.Count()} result(s) found.", results });
        }

        private static string BuildFullImageUrl(string baseUrl, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return imagePath;
            if (Uri.TryCreate(imagePath, UriKind.Absolute, out _)) return imagePath;
            if (string.IsNullOrWhiteSpace(baseUrl)) return imagePath;

            var normalizedImagePath = imagePath.TrimStart('/');
            return $"{baseUrl}/{normalizedImagePath}";
        }
    }
}
