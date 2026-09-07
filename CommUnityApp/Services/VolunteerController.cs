using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolunteerController : ControllerBase
    {
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IWebHostEnvironment _env;
        public VolunteerController(IWebHostEnvironment env, IVolunteerRepository volunteerRepository)
        {
            _env = env;
            _volunteerRepository = volunteerRepository;
        }

        [HttpPost("VolunteerLogin")]
        public async Task<IActionResult> VolunteerLogin([FromBody] VolunteerLoginModel model)
        {
            var user = await _volunteerRepository.VolunteerLogin(model.Email, model.Password);

            if (user == null)
                return BadRequest("Invalid login");

            if (!user.Role.Contains("4"))
                return BadRequest("You are not a volunteer");

            return Ok(new
            {
                userId = user.UserId,
                name = user.FirstName + " " + user.LastName,
                message = "Login successful"
            });
        }

        [HttpGet("GetVolunteerAssignedRequests")]
        public async Task<IActionResult> GetAssignedRequests(Guid volunteerId)
        {
            var data = await _volunteerRepository.GetVolunteerAssignedRequests(volunteerId);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.ImagePath))
                    item.ImagePath = baseUrl + item.ImagePath;
            }

            return Ok(data);
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] VolunteerStatusUpdateModel model)
        {
            await _volunteerRepository.UpdateVolunteerRequestStatus(model);
            return Ok(new { message = "Status updated successfully" });
        }

        [HttpPost("UpdateCharityDeliveryStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateVolunteerRequestStatusRequest request)
        {
            if (request == null || request.RequestId <= 0)
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Valid requestId is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new
                {
                    resultId = 0,
                    resultMessage = "Status is required."
                });
            }

            try
            {
                var result = await _volunteerRepository.UpdateVolunteerRequestStatus(
                    request.RequestId,
                    request.Status);

                if (result == null)
                {
                    return NotFound(new
                    {
                        resultId = 0,
                        resultMessage = "Charity request not found."
                    });
                }

                return Ok(new
                {
                    resultId = result.ResultId,
                    resultMessage = result.ResultMessage,
                    requestId = result.RequestId,
                    status = result.Status,
                    isDelivered = result.IsDelivered,
                    rewardCredited = result.IsRewardCredited,
                    rewardCoins = result.RewardCoins
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    resultId = 0,
                    resultMessage = ex.Message
                });
            }
        }
    }
}
