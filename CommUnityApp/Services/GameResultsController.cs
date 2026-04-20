using CommUnityApp.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameResultsController:ControllerBase
    {

        private readonly IGameResultsRepository _gameResultsRepository;


        [HttpGet("GetGamePlayMembers")]
        public async Task<IActionResult> GetGamePlayMembers(int page = 1, int size = 10, string search = "")
        {
            var result = await _gameResultsRepository.GetGamePlayMembers(page, size, search);

            return Ok(new
            {
                Data = result.Data,
                TotalCount = result.Total
            });
        }



    }
}
