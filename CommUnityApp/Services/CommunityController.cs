using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
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

        [HttpGet("GetCharityListByCommunity")]
        public async Task<IActionResult> GetCharityListByCommunity()
        {
            long communityId = 0;

            var sessionValue = HttpContext.Session.GetString("CommunityId");

            if (!string.IsNullOrEmpty(sessionValue))
            {
                communityId = Convert.ToInt64(sessionValue);
            }

            if (communityId == 0)
                return Unauthorized("Session expired");

            var data = await _communityRepository.GetCharityItemsByCommunityId(communityId);

            return Ok(data);
        }
        [HttpGet("GetVolunteers")]
        public async Task<IActionResult> GetVolunteers()
        {
            var claim = User.FindFirst("CommunityId")?.Value;

            long? communityId = null;

            if (!string.IsNullOrEmpty(claim))
                communityId = Convert.ToInt64(claim);

            var volunteers = await _communityRepository.GetVolunteersList(communityId);

            return Ok(volunteers);
        }
        [HttpPost("Assign")]
        public async Task<IActionResult> Assign([FromBody] AssignVolunteerRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is null" });

                if (request.CharityItemId <= 0)
                    return BadRequest(new { message = "Invalid Charity Item Id" });

                if (request.AssignedToUserId == Guid.Empty)
                    return BadRequest(new { message = "Invalid Volunteer Id" });

                var result = await _communityRepository.AssignVolunteer(
                    request.CharityItemId,
                    request.AssignedToUserId);

                if (!result)
                    return NotFound(new { message = "Charity item not found or already assigned" });

                return Ok(new
                {
                    success = true,
                    message = "Volunteer Assigned Successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Something went wrong",
                    error = ex.Message
                });
            }
        }
    }
}
