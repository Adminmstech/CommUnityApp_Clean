using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class TextQuizController : ControllerBase
    {
        private readonly ITextQuizRepository _textQuizRepository;

        public TextQuizController(ITextQuizRepository textQuizRepository)
        {
            _textQuizRepository = textQuizRepository;
        }

        [HttpGet("GetTextQuizList")]
        public async Task<IActionResult> GetTextQuizList([FromQuery] PagingRequest paging)
        {
            var data = await _textQuizRepository.GetTextQuizList(
                paging);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetTextQuizDetailsById")]
        public async Task<IActionResult> GetTextQuizDetailsById(int quizId,Guid userId)
        {
            var data = await _textQuizRepository.GetTextQuizDetailsById(
                quizId,
                userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }


        [HttpGet("GetTextQuizByGameId")]
        public async Task<IActionResult> GetTextQuizByGameId(int quizId,Guid userId)
        {
            var data = await _textQuizRepository.GetTextQuizByGameId(
                quizId,
                userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpPost("SubmitTextQuiz")]
        public async Task<IActionResult> SubmitTextQuiz([FromBody] UserTextQuizAnswers model)
        {
            var result = await _textQuizRepository.InsertUserTextQuizAnswers(model);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Quiz submitted successfully.",
                Status = true,
                Data = result
            });
        }

        [HttpGet("GetTextQuizResult")]
        public async Task<IActionResult> GetTextQuizResult( [FromQuery] PagingRequest paging,int quizId)
        {
            var data = await _textQuizRepository.GetTextQuizResult(
                paging,
                quizId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetUserQuizResult")]
        public async Task<IActionResult> GetUserQuizResult(int quizId, Guid userId)
        {
            var data = await _textQuizRepository.GetUserQuizResult(
                quizId,
                userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetTextQuizResultsByUser")]
        public async Task<IActionResult> GetTextQuizResultsByUser(Guid userId)
        {
            var data = await _textQuizRepository.GetTextQuizResultsByUser(userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetTextQuizStatusByUser")]
        public async Task<IActionResult> GetTextQuizStatusByUser(int quizId,Guid userId)
        {
            var result = await _textQuizRepository
                .GetTextQuizStatusByUser(quizId, userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = result
            });
        }

        [HttpGet("GetTextQuizQuestionsByGameId")]
        public async Task<IActionResult> GetTextQuizQuestionsByGameId(int quizId)
        {
            var data = await _textQuizRepository.GetTextQuizQuestionsByGameId(quizId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });

        }

        [HttpGet("GetTextQuizQuestionAnswersByGameId")]
        public async Task<IActionResult> GetTextQuizQuestionAnswersByGameId(int questionNumber,int quizId)
        {
            var data = await _textQuizRepository
                .GetTextQuizQuestionAnswersByGameId(questionNumber, quizId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetGameSponsors")]
        public async Task<IActionResult> GetGameSponsors( int gameTypeId, int gameId)
        {
            var data = await _textQuizRepository.GetGameSponsors(
                gameTypeId,
                gameId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetGameSponsorDetailsById")]
        public async Task<IActionResult> GetGameSponsorDetailsById(int sponsorId)
        {
            var data = await _textQuizRepository.GetGameSponsorDetailsById(sponsorId);

            if (data == null)
            {
                return Ok(new
                {
                    ResultId = 0,
                    ResultMessage = "Sponsor not found.",
                    Status = false
                });
            }

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpPost("SaveTextQuiz")]
        public async Task<IActionResult> SaveTextQuiz([FromBody] SaveTextQuizModel model)
        {
            try
            {
                var result = await _textQuizRepository.SaveTextQuiz(model);

                return Ok(new
                {
                    ResultId = result.StatusCode > 0 ? 1 : 0,
                    ResultMessage = result.StatusMessage,
                    Status = result.StatusCode > 0,
                    QuizId = result.StatusCode
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
        [HttpGet("GetAllTextQuiz")]
        public async Task<IActionResult> GetAllTextQuiz()
        {
            try
            {
                var data = await _textQuizRepository.GetAllTextQuiz();

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
        [HttpGet("GetTextQuizById")]
        public async Task<IActionResult> GetTextQuizById(long quizId)
        {
            var data = await _textQuizRepository.GetTextQuizById(quizId);

            if (data == null)
            {
                return NotFound(new
                {
                    ResultId = 0,
                    ResultMessage = "Quiz not found",
                    Status = false
                });
            }

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpPost("DeleteTextQuiz")]
        public async Task<IActionResult> DeleteTextQuiz(long quizId)
        {
            try
            {
                var result = await _textQuizRepository.DeleteTextQuiz(quizId);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = result.StatusMessage,
                    Status = true
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
