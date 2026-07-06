using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmartQuizController : ControllerBase
    {
        private readonly ISmartQuizRepository _smartQuizRepository;

        public SmartQuizController(ISmartQuizRepository smartQuizRepository)
        {
            _smartQuizRepository = smartQuizRepository;
        }




        [HttpGet("GetSmartQuizList")]
        public async Task<IActionResult> GetSmartQuizList()
        {
            try
            {
                var data = await _smartQuizRepository.GetSmartQuizList();

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpGet("GetSmartQuizById")]
        public async Task<IActionResult> GetSmartQuizById(
    int quizId,
    Guid userId)
        {
            try
            {
                var quiz =
                    await _smartQuizRepository.GetSmartQuizById(
                        quizId,
                        userId);

                if (quiz == null || quiz.SmartQuizDetails == null)
                {
                    return Ok(new
                    {
                        ResultId = 0,
                        ResultMessage = "Quiz not found.",
                        Status = false
                    });
                }

                int isFinished =
                    quiz.SmartQuizDetails.GameStatus == 1
                        ? 0
                        : quiz.Questions == null
                            ? 1
                            : (quiz.SmartQuizDetails.GameStatus == 3 &&
                               quiz.Questions.FirstOrDefault()?.QuestionNum == 1)
                                ? 1
                                : 0;

                if (quiz.Questions != null &&
                    quiz.Questions.Any() &&
                    quiz.Questions.First().QuestionNum == 0)
                {
                    isFinished = 1;
                }

                var results =
                    new List<SmartQuizResultModel>();

                string statusMessage =
                    quiz.SmartQuizDetails.GameStatus == 1
                    ? $"Game Starts In : {quiz.SmartQuizDetails.StartedInText}"
                    : "Game in Play Mode.";

                if (isFinished == 1)
                {
                    results =
                        await _smartQuizRepository
                            .GetCustomerSmartQuizResult(
                                quizId,
                                userId);

                    var self =
                        results.FirstOrDefault(x => x.IsSelf);

                    if (self != null)
                    {
                        statusMessage =
                            $"You have finished the quiz. " +
                            $"Score : {self.CorrectAnswerCount}/{self.AnsweredCount}. " +
                            $"Duration : {self.DurationString}";
                    }
                    else
                    {
                        statusMessage =
                            "You missed the quiz. Better luck next time.";
                    }
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = new
                    {
                        GameStatus = quiz.SmartQuizDetails.GameStatus,
                        Duration = quiz.SmartQuizDetails.StartedIn,
                        IsFinished = isFinished,
                        StatusMessage = statusMessage,
                        StartedIn = quiz.SmartQuizDetails.StartedInText,
                        EndedIn = quiz.SmartQuizDetails.EndedInText,
                        Questions = quiz,
                        Results = results
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpGet("GetSmartQuizStatusByCustomer")]
        public async Task<IActionResult> GetSmartQuizStatusByCustomer(
    int quizId,
    Guid userId)
        {
            try
            {
                var quiz =
                    await _smartQuizRepository.GetSmartQuizStatusByCustomer(
                        quizId,
                        userId);

                if (quiz == null || quiz.SmartQuizDetails == null)
                {
                    return Ok(new
                    {
                        ResultId = 0,
                        ResultMessage = "Quiz not found.",
                        Status = false
                    });
                }

                int isFinished = quiz.SmartQuizDetails.IsFinished ? 1 : 0;

                var results = new List<SmartQuizResultModel>();

                string statusMessage =
                    quiz.SmartQuizDetails.GameStatus == 1
                        ? $"Game Starts In : {quiz.SmartQuizDetails.StartedInText}"
                        : quiz.SmartQuizDetails.IsFinished
                            ? "Game Finished."
                            : "Game in Play Mode.";

                if (isFinished == 1)
                {
                    results = await _smartQuizRepository
                        .GetCustomerSmartQuizResult(
                            quizId,
                            userId);

                    var self = results.FirstOrDefault(x => x.IsSelf);

                    if (self != null)
                    {
                        statusMessage =
                            "Thanks for your time, you have completed the quiz.";

                        if (quiz.SmartQuizDetails.GameStatus != 3)
                        {
                            statusMessage +=
                                " Your rank may change while the quiz is still active.";
                        }
                    }
                    else
                    {
                        statusMessage =
                            "You missed the quiz. Better luck next time.";
                    }
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = new
                    {
                        GameStatus = quiz.SmartQuizDetails.GameStatus,
                        StartDuration = quiz.SmartQuizDetails.StartedIn,
                        IsFinished = isFinished,
                        StatusMessage = statusMessage,
                        StartedIn = quiz.SmartQuizDetails.StartedInText,
                        EndedIn = quiz.SmartQuizDetails.EndedInText,
                        Questions = quiz.Questions,
                        Results = results
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpPost("InsertCustomerSmartQuizAnswer")]
        public async Task<IActionResult> InsertCustomerSmartQuizAnswer(
    [FromBody] SubmitSmartQuizAnswerRequest request)
        {
            try
            {
                var quiz =
                    await _smartQuizRepository.GetSmartQuizById(
                        request.QuizId,
                        request.UserId);

                if (quiz == null || quiz.SmartQuizDetails == null)
                {
                    return Ok(new
                    {
                        ResultId = 0,
                        ResultMessage = "Quiz not found.",
                        Status = false
                    });
                }

                SmartQuizQuestionModel nextQuestion = null;

                if (quiz.SmartQuizDetails.GameStatus == 2 ||
                    (quiz.SmartQuizDetails.GameStatus == 3 &&
                     request.QuestionId > 0))
                {
                    nextQuestion =
                        await _smartQuizRepository.InsertCustomerSmartQuizAnswer(request);
                }

                int isFinished =
                    quiz.SmartQuizDetails.GameStatus == 1
                        ? 0
                        : nextQuestion == null
                            ? 1
                            : 0;

                if (nextQuestion != null &&
                    nextQuestion.QuestionNum == 0)
                {
                    isFinished = 1;
                }

                var results =
                    new List<SmartQuizResultModel>();

                string statusMessage =
                    quiz.SmartQuizDetails.GameStatus == 1
                        ? $"Game Starts In : {quiz.SmartQuizDetails.StartedInText}"
                        : nextQuestion == null
                            ? "Quiz Finished"
                            : "Game in Play Mode.";

                if (isFinished == 1)
                {
                    results =
                        await _smartQuizRepository
                            .GetCustomerSmartQuizResult(
                                request.QuizId,
                                request.UserId);

                    var self =
                        results.FirstOrDefault(x => x.IsSelf);

                    if (self != null)
                    {
                        statusMessage =
                            $"You have completed the quiz. " +
                            $"Score : {self.CorrectAnswerCount}/{self.AnsweredCount}. " +
                            $"Duration : {self.DurationString}";
                    }
                    else
                    {
                        statusMessage =
                            "You missed the quiz. Better luck next time.";
                    }
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = new
                    {
                        GameStatus = quiz.SmartQuizDetails.GameStatus,
                        Duration = quiz.SmartQuizDetails.StartedIn,
                        IsFinished = isFinished,
                        StatusMessage = statusMessage,
                        StartedIn = quiz.SmartQuizDetails.StartedInText,
                        EndedIn = quiz.SmartQuizDetails.EndedInText,
                        Question = nextQuestion,
                        Results = results
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpPost("InsertSmartQuizCustomerAllAnswers")]
        public async Task<IActionResult> InsertSmartQuizCustomerAllAnswers(
    [FromBody] SubmitSmartQuizRequest request)
        {
            try
            {
                var submitResult =
                    await _smartQuizRepository
                        .InsertSmartQuizCustomerAllAnswers(request);

                var quiz =
                    await _smartQuizRepository
                        .GetSmartQuizStatusByCustomer(
                            request.QuizId,
                            request.UserId);

                var results =
                    await _smartQuizRepository
                        .GetCustomerSmartQuizResult(
                            request.QuizId,
                            request.UserId);

                string statusMessage =
                    "You missed the quiz. Better luck next time.";

                var self =
                    results.FirstOrDefault(x => x.IsSelf);

                if (self != null)
                {
                    statusMessage =
                        "Thanks for your time, you have completed the quiz.";

                    if (quiz.SmartQuizDetails.GameStatus != 3)
                    {
                        statusMessage +=
                            " Game is still running. Your rank may change.";
                    }

                    // Optional
                    // Send Email
                    // await _emailService.SendQuizCompletionEmail(...);
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Quiz submitted successfully.",
                    Status = true,
                    Data = new
                    {
                        StatusMessage = statusMessage,
                        QuizResult = submitResult,
                        Results = results
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpGet("GetSmartQuizResultsByUserId")]
        public async Task<IActionResult> GetSmartQuizResultsByUserId(
    Guid userId)
        {
            try
            {
                var data =
                    await _smartQuizRepository
                        .GetSmartQuizResultsByUserId(userId);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }
       

    }
}
