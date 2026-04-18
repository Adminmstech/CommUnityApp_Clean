using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
        public async Task<IActionResult> GenerateOtp([FromBody] ForgotPasswordRequest model, CancellationToken cancellationToken = default)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Email is required."
                });
            }

            if (!IsValidEmail(model.Email))
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Invalid email format."
                });
            }

            try
            {
                var result = await _unitOfWork.ForgotPassword.GenerateOtpAsync(model.Email);

                if (result == null || result.ResultId <= 0)
                {
                    _logger.LogWarning("Failed to generate OTP for email: {Email}", model.Email);
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = result?.ResultMessage ?? "Failed to generate OTP. Please check if the email exists."
                    });
                }

                // Send email asynchronously
                try
                {
                    await _emailService.SendPasswordResetEmailAsync(model.Email, result.OTP);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send OTP email to {Email}", model.Email);
                    return StatusCode(500, new
                    {
                        ResultId = -1,
                        ResultMessage = "OTP was generated but email sending failed. Please try again."
                    });
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "OTP sent successfully to your email."
                });
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Database error while generating OTP for {Email}", model.Email);
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = "A database error occurred. Please try again later."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating OTP for {Email}", model.Email);
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = "An unexpected error occurred. Please try again later."
                });
            }
        }

        // ======================================================
        // 2️⃣ VERIFY OTP
        // ======================================================
        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Invalid request data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.OTP))
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Email and OTP are required."
                });
            }

            if (!IsValidEmail(model.Email))
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Invalid email format."
                });
            }

            try
            {
                var result = await _unitOfWork.ForgotPassword.VerifyOtpAsync(model.Email, model.OTP);

                if (result == null || result.ResultId <= 0)
                {
                    _logger.LogWarning("Failed OTP verification attempt for email: {Email}", model.Email);
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = result?.ResultMessage ?? "Invalid or expired OTP."
                    });
                }

                return Ok(result);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Database error while verifying OTP for {Email}", model.Email);
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = "A database error occurred. Please try again later."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error verifying OTP for {Email}", model.Email);
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = "An unexpected error occurred. Please try again later."
                });
            }
        }

        // ======================================================
        // 3️⃣ RESET PASSWORD
        // ======================================================
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Invalid request data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Email and new password are required."
                });
            }

            if (!IsValidEmail(model.Email))
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Invalid email format."
                });
            }

            // Enhanced password validation
            var passwordValidation = ValidatePassword(model.NewPassword);
            if (!passwordValidation.IsValid)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = passwordValidation.ErrorMessage
                });
            }

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                var result = await _unitOfWork.ForgotPassword.ResetPasswordAsync(model.Email, hashedPassword);

                if (result == null || result.ResultId <= 0)
                {
                    _logger.LogWarning("Failed to reset password for email: {Email}", model.Email);
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = result?.ResultMessage ?? "Failed to reset password."
                    });
                }

                // Send confirmation email asynchronously
                try
                {
                    await _emailService.SendPasswordResetSuccessEmailAsync(model.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send password reset confirmation email to {Email}", model.Email);
                    // Don't fail the request if email fails - password was already reset
                }

                return Ok(result);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Database error while resetting password for {Email}", model.Email);
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = "A database error occurred. Please try again later."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error resetting password for {Email}", model.Email);
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = "An unexpected error occurred. Please try again later."
                });
            }
        }

        // ======================================================
        // HELPER METHODS
        // ======================================================
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty.");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters long.");

            if (password.Length > 128)
                return (false, "Password must not exceed 128 characters.");

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            if (!hasUpper || !hasLower || !hasDigit)
                return (false, "Password must contain at least one uppercase letter, one lowercase letter, and one digit.");

            return (true, string.Empty);
        }
    }
}
