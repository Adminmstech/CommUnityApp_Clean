using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly ILogger<BusinessController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly IEmailService _emailService;

        public BusinessController(ILogger<BusinessController> logger, IUnitOfWork unitOfWork, IConfiguration config, IHubContext<AuctionHub> hubContext, IEmailService emailService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;
            _hubContext = hubContext;
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }


        [HttpPost("Add_Business")]
        public async Task<IActionResult> AddBusiness([FromBody] AddBusinessRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string logoPath = null;

                // ✅ Save Logo
                if (!string.IsNullOrWhiteSpace(request.LogoBase64))
                {
                    if (!TryConvertFromBase64(request.LogoBase64, out byte[] fileBytes))
                        return BadRequest("Invalid logo format.");

                    if (fileBytes.Length > 2097152)
                        return BadRequest("Logo size exceeds 2MB limit.");

                    string fileName = $"{Guid.NewGuid():N}.jpg";
                    string directoryPath = Path.Combine("wwwroot", "BusinessLogos");
                    Directory.CreateDirectory(directoryPath);

                    string localFilePath = Path.Combine(directoryPath, fileName);
                    await System.IO.File.WriteAllBytesAsync(localFilePath, fileBytes);

                    logoPath = $"BusinessLogos/{fileName}";
                }

                // ✅ Call DAL
                var result = await _unitOfWork.Business.AddBusinessAsync(new AddBusinessRequest
                {
                    BusinessId = request.BusinessId,
                    CategoryId = request.CategoryId,
                    BusinessName = request.BusinessName,
                    BusinessNumber = request.BusinessNumber,
                    OwnerName = request.OwnerName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    Country = request.Country,
                    Suburb = request.Suburb,
                    Logo = logoPath,
                    Info = request.Info,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsVerified = request.IsVerified,
                    IsActive = request.IsActive
                });

                // ✅ If new user created → send password email
                if (result.ResultId > 0 && !string.IsNullOrEmpty(result.GeneratedPassword))
                {
                    await _emailService.SendBusinessUserCredentialsEmailAsync(
                        request.Email,
                        result.GeneratedPassword
                    );
                }

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

        private bool TryConvertFromBase64(string base64String, out byte[] fileBytes)
{
    fileBytes = null;

    try
    {
        if (base64String.Contains(","))
            base64String = base64String.Split(',')[1];

        fileBytes = Convert.FromBase64String(base64String);
        return true;
    }
    catch
    {
        return false;
    }
}



        [HttpGet("Get_Businesses")]
        public async Task<IActionResult> GetBusinesses()
        {
            var data = await _unitOfWork.Business.GetAllBusinesses();
            return Ok(data);
        }


        [HttpGet("Get_BusinessDetails")]
        public async Task<IActionResult> GetBusinessDetails( int BusinessId)
        {
            var data = await _unitOfWork.Business.GetBusinessDetails(BusinessId);
            return Ok(data);
        }
    }


}
