using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ILogger<CampaignController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public CampaignController(ILogger<CampaignController> logger, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;
        }


        [HttpPost("Campaign_Save")]
        public async Task<IActionResult> SaveCampaign([FromBody] Campaign entity)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // ✅ Required Validations
                if (entity.BusinessId <= 0)
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "BusinessId is required"
                    });

                if (string.IsNullOrWhiteSpace(entity.CampaignName))
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "CampaignName is required"
                    });

                if (entity.StartDate == default || entity.EndDate == default)
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "StartDate and EndDate are required"
                    });

                if (entity.EndDate < entity.StartDate)
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "EndDate must be greater than StartDate"
                    });

                // 🔥 IMAGE HANDLING (Same logic as your icon)
                if (!string.IsNullOrWhiteSpace(entity.CampaignImage))
                {
                    string base64 = entity.CampaignImage;

                    // ✅ REMOVE BASE64 PREFIX
                    if (base64.Contains(","))
                        base64 = base64.Substring(base64.IndexOf(",") + 1);

                    byte[] fileBytes;
                    try
                    {
                        fileBytes = Convert.FromBase64String(base64);
                    }
                    catch
                    {
                        return BadRequest("Invalid image format.");
                    }

                    // Optional: size check (3MB)
                    if (fileBytes.Length > 3145728)
                        return BadRequest("Image size exceeds 3MB limit.");

                    string fileName = $"{Guid.NewGuid():N}.png";
                    string directoryPath = Path.Combine("wwwroot", "CampaignImages");

                    Directory.CreateDirectory(directoryPath);

                    string filePath = Path.Combine(directoryPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    // ✅ Save relative path
                    entity.CampaignImage = $"CampaignImages/{fileName}";
                }

                // 🔹 Call DAL
                var result = await _unitOfWork.Campaign.SaveCampaign(entity);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving campaign");

                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpGet("Campaign_List")]
        public async Task<IActionResult> GetCampaignList()
        {
            try
            {
                var result = await _unitOfWork.Campaign.GetCampaignList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching campaigns");

                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpGet("Business_Campaigns")]
        public async Task<IActionResult> GetCampaignByBusiness()
        {
            try
            {
                var businessId = HttpContext.Session.GetString("BusinessId");

                if (string.IsNullOrEmpty(businessId))
                    return Unauthorized(new
                    {
                        ResultId = 0,
                        ResultMessage = "Session expired"
                    });

                var result = await _unitOfWork.Campaign
                    .GetCampaignsByBusiness(Convert.ToInt32(businessId));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching campaigns");

                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }
    }
}
