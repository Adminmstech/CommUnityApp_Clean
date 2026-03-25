using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IPasswordService _passwordService;

        public EmailController(
            ILogger<EmailController> logger,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IPasswordService passwordService, IConfiguration config)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _config = config;
        }
        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody] BulkEmailRequest request)
        {
            if (request.Emails == null || !request.Emails.Any())
                return BadRequest(new { message = "No emails provided" });

            try
            {
                int sentCount = 0;

                // 🔥 Works for single OR multiple
                foreach (var email in request.Emails)
                {
                    var sent = await _emailService.SendBulkEmailAsync(email, request.Subject, request.Body);

                    if (sent) sentCount++;
                }

                return Ok(new
                {
                    message = "Emails sent successfully",
                    total = request.Emails.Count,
                    sent = sentCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
