using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IPasswordService _passwordService;

        public UserController(
            ILogger<UserController> logger,
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

        [HttpGet("Get_Roles")]
        public async Task<IActionResult> GetRoles()
        {
            var data = await _unitOfWork.User.GetRoles();
            return Ok(data);
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> UserRegister([FromBody] RegisterRequest entity)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string generatedPassword = null;
                bool shouldGeneratePassword = string.IsNullOrWhiteSpace(entity.Password);

                if (shouldGeneratePassword)
                {
                    generatedPassword = _passwordService.GenerateSecurePassword(8);
                    entity.Password = generatedPassword;
                }
                else if (entity.Password.Length < 8)
                {
                    return BadRequest("Password must be at least 8 characters long.");
                }

                var result = await _unitOfWork.User.RegisterUser(entity);

                if (shouldGeneratePassword && result.ResultId > 0 && !string.IsNullOrWhiteSpace(entity.Email))
                {
                    string fullName = string.IsNullOrWhiteSpace(entity.LastName)
                        ? entity.FirstName
                        : $"{entity.FirstName} {entity.LastName}";
                    await _emailService.SendWelcomeEmailAsync(entity.Email, fullName, generatedPassword);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Email}", entity?.Email);
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpPost("Update_User")]
        public async Task<IActionResult> AddUser([FromBody] Users entity)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool isNewUser = entity.UserId == null || entity.UserId == Guid.Empty;
                string plainPassword = string.Empty;

                if (!string.IsNullOrWhiteSpace(entity.ProfileImageBase64))
                {
                    if (!TryConvertFromBase64(entity.ProfileImageBase64, out byte[] fileBytes))
                        return BadRequest("Invalid image format.");

                    if (fileBytes.Length > 2097152)
                        return BadRequest("Image size exceeds 2MB limit.");

                    string fileName = string.Concat(Guid.NewGuid().ToString("N"), ".jpg");
                    string directoryPath = Path.Combine("wwwroot", "ProfilePics");
                    Directory.CreateDirectory(directoryPath);

                    string localFilePath = Path.Combine(directoryPath, fileName);
                    await System.IO.File.WriteAllBytesAsync(localFilePath, fileBytes);

                    entity.ProfileImagePath = string.Concat("ProfilePics/", fileName);
                }

                if (isNewUser)
                {
                    plainPassword = _passwordService.GenerateSecurePassword(8);
                    entity.Password = plainPassword;
                    entity.IsActive = true;
                }

                var result = await _unitOfWork.User.SaveUser(entity);

                if (isNewUser && result.ResultId > 0 && !string.IsNullOrWhiteSpace(entity.Email))
                {
                    string fullName = string.IsNullOrWhiteSpace(entity.LastName)
                        ? entity.FirstName
                        : $"{entity.FirstName} {entity.LastName}";
                    await _emailService.SendWelcomeEmailAsync(entity.Email, fullName, plainPassword);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", entity?.UserId);
                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }




        [HttpPost("Get_UserByUserId")]
        public async Task<IActionResult> GetUserById(Guid UserId)
        {
            var data = await _unitOfWork.User.GetUserByUserId(UserId);
            return Ok(data);
        }



        [HttpPost("Addorupdate_Userwallets")]
        public async Task<IActionResult> AddOrUpdateUserWalletAddService(UserWallets entity)
        {
            var data = await _unitOfWork.User.AddOrUpdateUserWallet(entity);
            return Ok(data);
        }


        [HttpPost("Add_walletTransaction")]
        public async Task<IActionResult> AddWalletTransaction(WalletTransactions entity)
        {
            var data = await _unitOfWork.User.AddWalletTransaction(entity);
            return Ok(data);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var result = await _unitOfWork.User.UserLogin(request);

                if (result == null || result.ResultId == 0)
                {
                    return Unauthorized(new
                    {
                        ResultId = 0,
                        ResultMessage = "Invalid email or password"
                    });
                }

                // 🔐 Create Claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new Claim(ClaimTypes.Name, result.FullName ?? ""),
            new Claim(ClaimTypes.Email, result.Email ?? ""),
            new Claim(ClaimTypes.Role, result.Role ?? "")
        };

                // 🔑 Generate Token
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"])
                );

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(8),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Login successful",
                    Token = tokenString, // ✅ IMPORTANT
                    User = new
                    {
                        result.UserId,
                        result.FullName,
                        result.Email,
                        result.Role
                    },
                    //Redirect = result.Role switch
                    //{
                    //    "Admin" => "/Admin/Dashboard",
                    //    "Business Owner" => "/Business/Dashboard",
                    //    "Customer" => "/Customer/Home",
                    //    _ => "/"
                    //}
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", request?.Email);

                return StatusCode(500, new
                {
                    ResultId = 0,
                    ResultMessage = "Something went wrong"
                });
            }
        }

        private static bool TryConvertFromBase64(string base64String, out byte[] bytes)
        {
            bytes = null;
            if (string.IsNullOrWhiteSpace(base64String))
                return false;

            try
            {
                bytes = Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        [HttpGet("Get_BusinessUsers")]
        public async Task<IActionResult> GetBusinessUsers()
        {
            var data = await _unitOfWork.User.GetBusinessUsers();
            return Ok(data);
        }

    }
}

