using CommUnityApp.BAL.Interfaces;
using CommUnityApp.DAL;
using CommUnityApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{

  
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {

        private readonly ICommunityRepository _communityRepository;


        public CommunityController(ICommunityRepository communityRepository)
        {
            _communityRepository = communityRepository;
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] CommunityLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new
                {
                    resultID = 0,
                    resultMessage = "Username and Password are required"
                });
            }

            var admin = await _communityRepository.LoginAsync(request);

            if (admin == null)
            {
                return Ok(new
                {
                    resultID = 0,
                    resultMessage = "Invalid username or password"
                });
            }
            return Ok(new
            {
                resultID = 1,
                resultMessage = "Login successful",
                admin
            });
        }
        [HttpGet("GetByCommunity/{communityId}")]
        public async Task<IActionResult> GetByCommunity(long communityId)
        {
            return Ok(await _communityRepository.GetGroupsByCommunityAsync(communityId));
        }

    }
}
