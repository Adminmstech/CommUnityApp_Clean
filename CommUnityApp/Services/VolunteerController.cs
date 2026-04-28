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
    }
}
