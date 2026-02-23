using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;



namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly ILogger<ForgotPasswordController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public ForgotPasswordController(
            ILogger<ForgotPasswordController> logger,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        // ======================================================
        // 1️⃣ GENERATE OTP
        // ======================================================
        [HttpPost("GenerateOTP")]
        public async Task<IActionResult> GenerateOtp([FromBody] ForgotPasswordRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Email))
                    return BadRequest("Email is required.");

                var result = await _unitOfWork.ForgotPassword.GenerateOtpAsync(model.Email);

                if (result == null || result.ResultId <= 0)
                    return BadRequest(result);

                await _emailService.SendPasswordResetEmailAsync(model.Email, result.OTP);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "OTP sent successfully to your email."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.OTP))
                    return BadRequest("Email and OTP are required.");

                var result = await _unitOfWork.ForgotPassword.VerifyOtpAsync(model.Email, model.OTP);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

       
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.NewPassword))
                    return BadRequest("Email and New Password are required.");

                // 🔐 OPTIONAL (Recommended): Hash password before saving
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                var result = await _unitOfWork.ForgotPassword
                    .ResetPasswordAsync(model.Email, hashedPassword);

                await _emailService.SendPasswordResetSuccessEmailAsync(model.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }
    }
}
