using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using CommUnityApp.ApplicationCore.Models;


namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameResultsController:ControllerBase
    {

        private readonly IGameResultsRepository _gameResultsRepository;

        public GameResultsController(IGameResultsRepository gameResultsRepository)
        {
            _gameResultsRepository = gameResultsRepository;
        }

        [HttpGet("GetBrandGamePlayMembers")]
        public async Task<IActionResult> GetGamePlayMembers(int page = 1, int size = 10, string search = "")
        {
            var result = await _gameResultsRepository.GetGamePlayMembers(page, size, search);

            return Ok(new
            {
                data = result.Data,          
                totalCount = result.Total
            });
        }

        [HttpGet("GetSpinGameResults")]
        public async Task<IActionResult> GetSpinGameResults(int page = 1, int size = 10, string search = "")
        {
            var result = await _gameResultsRepository.GetSpinGameResults(page, size, search);

            return Ok(new
            {
                data = result.Data,
                totalCount = result.Total
            });
        }

       

        [HttpPost("AssignPrize")]
        public async Task<IActionResult> AssignPrize([FromBody] AssignPrizeModel model)
        {
            var result = await _gameResultsRepository.AssignPrize(model);
            return Ok(new { status = result });
        }
    }
}
