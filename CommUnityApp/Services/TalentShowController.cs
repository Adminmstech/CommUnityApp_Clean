using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentShowController : ControllerBase
    {
        private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4",
            ".webm",
            ".mov",
            ".m4v"
        };

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private const long MaxVideoBytes = 262_144_000;
        private const long MaxThumbnailBytes = 5_242_880;
        private const long MaxBase64UploadRequestBytes = 370_000_000;

        private readonly ILogger<TalentShowController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<TalentShowHub> _talentShowHub;

        public TalentShowController(
            ILogger<TalentShowController> logger,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment,
            IHubContext<TalentShowHub> talentShowHub)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _environment = environment;
            _talentShowHub = talentShowHub;
        }

        [HttpGet("campaigns")]
        public async Task<IActionResult> GetCampaigns(
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool uploadOpenOnly = false)
        {
            try
            {
                return Ok(await _unitOfWork.TalentShow.GetCampaignsAsync(includeInactive, uploadOpenOnly));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching talent show campaigns");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpGet("campaigns/{talentShowCampaignId:int}")]
        public async Task<IActionResult> GetCampaign(int talentShowCampaignId)
        {
            try
            {
                var campaign = await _unitOfWork.TalentShow.GetCampaignByIdAsync(talentShowCampaignId);

                if (campaign == null)
                    return NotFound(new { ResultId = 0, ResultMessage = "Campaign not found" });

                return Ok(campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching talent show campaign {TalentShowCampaignId}", talentShowCampaignId);
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpPost("campaigns/save")]
        public async Task<IActionResult> SaveCampaign([FromBody] TalentShowCampaign campaign)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(campaign.CampaignName))
                    return BadRequest(new { ResultId = 0, ResultMessage = "Campaign name is required" });

                if (campaign.UploadEndDate <= campaign.UploadStartDate)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Upload end date must be after start date" });

                var result = await _unitOfWork.TalentShow.SaveCampaignAsync(campaign);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving talent show campaign");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories([FromQuery] bool includeInactive = false)
        {
            try
            {
                return Ok(await _unitOfWork.TalentShow.GetCategoriesAsync(includeInactive));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching talent show categories");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpGet("categories/{talentShowCategoryId:int}")]
        public async Task<IActionResult> GetCategory(int talentShowCategoryId)
        {
            try
            {
                var category = await _unitOfWork.TalentShow.GetCategoryByIdAsync(talentShowCategoryId);

                if (category == null)
                    return NotFound(new { ResultId = 0, ResultMessage = "Category not found" });

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching talent show category {TalentShowCategoryId}", talentShowCategoryId);
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpPost("categories/save")]
        public async Task<IActionResult> SaveCategory([FromBody] TalentShowCategory category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.CategoryName))
                    return BadRequest(new { ResultId = 0, ResultMessage = "Category name is required" });

                var result = await _unitOfWork.TalentShow.SaveCategoryAsync(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving talent show category");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpPost("registrations/save")]
        public async Task<IActionResult> RegisterForCampaign([FromBody] TalentShowRegistrationRequest request)
        {
            try
            {
                if (request.TalentShowCampaignId == 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Campaign is required" });

                if (request.TalentShowCategoryId == 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Category is required" });

                if (string.IsNullOrWhiteSpace(request.AgeGroup))
                    return BadRequest(new { ResultId = 0, ResultMessage = "Age group is required" });

                if (!request.AcceptedTerms)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Terms and conditions must be accepted" });

                request.MemberId ??= GetSessionMemberId();
                request.MemberName = string.IsNullOrWhiteSpace(request.MemberName)
                    ? GetSessionMemberName()
                    : request.MemberName.Trim();

                var result = await _unitOfWork.TalentShow.RegisterForCampaignAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering for talent campaign");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpGet("registrations/status")]
        public async Task<IActionResult> GetRegistrationStatus(
            [FromQuery] int talentShowCampaignId,
            [FromQuery] long? memberId = null,
            [FromQuery] string? visitorKey = null)
        {
            try
            {
                if (talentShowCampaignId == 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Campaign is required" });

                memberId ??= GetSessionMemberId();
                visitorKey = string.IsNullOrWhiteSpace(visitorKey) ? HttpContext.Session.Id : visitorKey.Trim();

                var status = await _unitOfWork.TalentShow.GetRegistrationStatusAsync(talentShowCampaignId, memberId, visitorKey);

                if (status == null)
                    return NotFound(new { ResultId = 0, ResultMessage = "Registration not found" });

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching talent campaign registration status");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpGet("videos")]
        public async Task<IActionResult> GetVideos([FromQuery] TalentShowVideoFilter filter)
        {
            try
            {
                filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
                filter.PageSize = filter.PageSize is <= 0 or > 100 ? 24 : filter.PageSize;

                return Ok(await _unitOfWork.TalentShow.GetVideosAsync(filter));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching talent show videos");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpPost("videos/upload")]
        [RequestSizeLimit(MaxVideoBytes + MaxThumbnailBytes)]
        public async Task<IActionResult> UploadVideo([FromForm] TalentShowVideoUploadRequest request)
        {
            try
            {
                if (request.TalentShowCampaignId <= 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Campaign is required" });

                if (request.TalentShowCategoryId <= 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Category is required" });

                if (request.ChallengeNumber is < 1 or > 3)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Challenge number must be between 1 and 3" });

                if (string.IsNullOrWhiteSpace(request.Title))
                    return BadRequest(new { ResultId = 0, ResultMessage = "Video title is required" });

                if (request.VideoFile == null || request.VideoFile.Length == 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Video file is required" });

                var campaignError = await ValidateUploadCampaignAsync(request.TalentShowCampaignId);
                if (campaignError != null)
                    return BadRequest(campaignError);

                var category = await _unitOfWork.TalentShow.GetCategoryByIdAsync(request.TalentShowCategoryId);
                if (category == null || !category.IsActive)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Selected category is not active" });

                var videoPath = await SaveFileAsync(
                    request.VideoFile,
                    Path.Combine("Uploads", "TalentShow", "Videos"),
                    AllowedVideoExtensions,
                    MaxVideoBytes,
                    "video");

                string? thumbnailPath = null;
                if (request.ThumbnailFile is { Length: > 0 })
                {
                    thumbnailPath = await SaveFileAsync(
                        request.ThumbnailFile,
                        Path.Combine("Uploads", "TalentShow", "Thumbnails"),
                        AllowedImageExtensions,
                        MaxThumbnailBytes,
                        "thumbnail");
                }

                var video = new TalentShowVideo
                {
                    TalentShowCampaignId = request.TalentShowCampaignId,
                    TalentShowCategoryId = request.TalentShowCategoryId,
                    ChallengeNumber = request.ChallengeNumber,
                    ChallengeLevel = ResolveChallengeLevel(request.ChallengeNumber),
                    AgeGroup = request.AgeGroup?.Trim(),
                    MemberId = request.MemberId ?? GetSessionMemberId(),
                    MemberName = string.IsNullOrWhiteSpace(request.MemberName) ? GetSessionMemberName() : request.MemberName.Trim(),
                    Title = request.Title.Trim(),
                    Description = request.Description,
                    VideoPath = videoPath,
                    ThumbnailPath = thumbnailPath,
                    IsApproved = false,
                    ApprovalStatus = "Pending",
                    IsActive = true
                };

                var result = await _unitOfWork.TalentShow.SaveVideoAsync(video);

                if (result.ResultId > 0)
                    await _talentShowHub.Clients.Group("TalentShow").SendAsync("talentShowVideoCreated", result.ResultId);

                return Ok(new
                {
                    result.ResultId,
                    result.ResultMessage,
                    video.VideoPath,
                    video.ThumbnailPath
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ResultId = 0, ResultMessage = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading talent show video");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpPost("videos/upload-base64")]
        [RequestSizeLimit(MaxBase64UploadRequestBytes)]
        public async Task<IActionResult> UploadVideoBase64([FromBody] TalentShowVideoBase64UploadRequest request)
        {
            try
            {
                if (request.TalentShowCampaignId <= 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Campaign is required" });

                if (request.TalentShowCategoryId <= 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Category is required" });

                if (request.ChallengeNumber is < 1 or > 3)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Challenge number must be between 1 and 3" });

                if (string.IsNullOrWhiteSpace(request.Title))
                    return BadRequest(new { ResultId = 0, ResultMessage = "Video title is required" });

                if (string.IsNullOrWhiteSpace(request.VideoBase64))
                    return BadRequest(new { ResultId = 0, ResultMessage = "Video base64 is required" });

                var campaignError = await ValidateUploadCampaignAsync(request.TalentShowCampaignId);
                if (campaignError != null)
                    return BadRequest(campaignError);

                var category = await _unitOfWork.TalentShow.GetCategoryByIdAsync(request.TalentShowCategoryId);
                if (category == null || !category.IsActive)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Selected category is not active" });

                var videoPath = await SaveBase64FileAsync(
                    request.VideoBase64,
                    request.VideoFileName,
                    request.VideoExtension,
                    Path.Combine("Uploads", "TalentShow", "Videos"),
                    AllowedVideoExtensions,
                    MaxVideoBytes,
                    "video");

                string? thumbnailPath = null;
                if (!string.IsNullOrWhiteSpace(request.ThumbnailBase64))
                {
                    thumbnailPath = await SaveBase64FileAsync(
                        request.ThumbnailBase64,
                        request.ThumbnailFileName,
                        request.ThumbnailExtension,
                        Path.Combine("Uploads", "TalentShow", "Thumbnails"),
                        AllowedImageExtensions,
                        MaxThumbnailBytes,
                        "thumbnail");
                }

                var video = new TalentShowVideo
                {
                    TalentShowCampaignId = request.TalentShowCampaignId,
                    TalentShowCategoryId = request.TalentShowCategoryId,
                    ChallengeNumber = request.ChallengeNumber,
                    ChallengeLevel = ResolveChallengeLevel(request.ChallengeNumber),
                    AgeGroup = request.AgeGroup?.Trim(),
                    MemberId = request.MemberId ?? GetSessionMemberId(),
                    MemberName = string.IsNullOrWhiteSpace(request.MemberName) ? GetSessionMemberName() : request.MemberName.Trim(),
                    Title = request.Title.Trim(),
                    Description = request.Description,
                    VideoPath = videoPath,
                    ThumbnailPath = thumbnailPath,
                    IsApproved = false,
                    ApprovalStatus = "Pending",
                    IsActive = true
                };

                var result = await _unitOfWork.TalentShow.SaveVideoAsync(video);

                if (result.ResultId > 0)
                    await _talentShowHub.Clients.Group("TalentShow").SendAsync("talentShowVideoCreated", result.ResultId);

                return Ok(new
                {
                    result.ResultId,
                    result.ResultMessage,
                    video.VideoPath,
                    video.ThumbnailPath
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ResultId = 0, ResultMessage = ex.Message });
            }
            catch (FormatException)
            {
                return BadRequest(new { ResultId = 0, ResultMessage = "Invalid base64 file content." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading base64 talent show video");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpPost("videos/approval")]
        public async Task<IActionResult> SaveVideoApproval([FromBody] TalentShowVideoApprovalRequest request)
        {
            try
            {
                if (request.TalentShowVideoId == 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Video id is required" });

                if (request.JudgeScore is < 0 or > 100)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Judge score must be between 0 and 100" });

                var result = await _unitOfWork.TalentShow.SaveVideoApprovalAsync(request);

                if (result.ResultId != 0)
                    await _talentShowHub.Clients.Group("TalentShow").SendAsync("talentShowVideoCreated", request.TalentShowVideoId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating talent show video approval");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpPost("videos/reaction")]
        public async Task<IActionResult> SaveReaction([FromBody] TalentShowReactionRequest request)
        {
            try
            {
                if (request.TalentShowVideoId == 0)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Video id is required" });

                if (request.IsLike == null && request.Rating == null)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Like or rating is required" });

                if (request.Rating is < 1 or > 5)
                    return BadRequest(new { ResultId = 0, ResultMessage = "Rating must be between 1 and 5" });

                request.MemberId ??= GetSessionMemberId();
                request.VisitorKey = string.IsNullOrWhiteSpace(request.VisitorKey)
                    ? HttpContext.Session.Id
                    : request.VisitorKey.Trim();

                var result = await _unitOfWork.TalentShow.SaveReactionAsync(request);
                var summary = await _unitOfWork.TalentShow.GetVideoSummaryAsync(request.TalentShowVideoId);

                if (summary != null)
                    await _talentShowHub.Clients.Group("TalentShow").SendAsync("talentShowVideoUpdated", summary);

                return Ok(new
                {
                    result.ResultId,
                    result.ResultMessage,
                    Summary = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving talent show reaction");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpGet("videos/{talentShowVideoId:long}/summary")]
        public async Task<IActionResult> GetVideoSummary(long talentShowVideoId)
        {
            try
            {
                var summary = await _unitOfWork.TalentShow.GetVideoSummaryAsync(talentShowVideoId);

                if (summary == null)
                    return NotFound(new { ResultId = 0, ResultMessage = "Video not found" });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching talent show summary {TalentShowVideoId}", talentShowVideoId);
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        [HttpGet("rankings")]
        public async Task<IActionResult> GetRankings(
            [FromQuery] int? talentShowCampaignId = null,
            [FromQuery] int? talentShowCategoryId = null,
            [FromQuery] string? ageGroup = null)
        {
            try
            {
                return Ok(await _unitOfWork.TalentShow.GetRankingsAsync(talentShowCampaignId, talentShowCategoryId, ageGroup));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching talent campaign rankings");
                return StatusCode(500, new { ResultId = -1, ResultMessage = ex.Message });
            }
        }

        private long? GetSessionMemberId()
        {
            var sessionValue = HttpContext.Session.GetString("MemberId")
                ?? HttpContext.Session.GetString("UserId")
                ?? HttpContext.Session.GetString("CommunityId");

            return long.TryParse(sessionValue, out var memberId) ? memberId : null;
        }

        private string? GetSessionMemberName()
        {
            return HttpContext.Session.GetString("MemberName")
                ?? HttpContext.Session.GetString("UserName")
                ?? HttpContext.Session.GetString("CommunityName");
        }

        private static string ResolveChallengeLevel(int challengeNumber)
        {
            return challengeNumber switch
            {
                1 => "Beginner",
                2 => "Intermediate",
                _ => "Advanced / Signature Performance"
            };
        }

        private async Task<object?> ValidateUploadCampaignAsync(int talentShowCampaignId)
        {
            var campaign = await _unitOfWork.TalentShow.GetCampaignByIdAsync(talentShowCampaignId);

            if (campaign == null || !campaign.IsActive)
                return new { ResultId = 0, ResultMessage = "Selected campaign is not active" };

            var now = DateTime.UtcNow;
            if (now < campaign.UploadStartDate)
                return new { ResultId = 0, ResultMessage = "Campaign upload window has not started" };

            if (now > campaign.UploadEndDate)
                return new { ResultId = 0, ResultMessage = "Campaign upload window is closed" };

            return null;
        }

        private async Task<string> SaveFileAsync(
            IFormFile file,
            string relativeFolder,
            HashSet<string> allowedExtensions,
            long maxBytes,
            string fileType)
        {
            if (file.Length > maxBytes)
                throw new InvalidOperationException($"{fileType} file exceeds allowed size.");

            var extension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Invalid {fileType} file format.");

            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRoot, relativeFolder);
            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Path.Combine(relativeFolder, fileName).Replace("\\", "/");
        }

        private async Task<string> SaveBase64FileAsync(
            string base64,
            string? originalFileName,
            string? requestedExtension,
            string relativeFolder,
            HashSet<string> allowedExtensions,
            long maxBytes,
            string fileType)
        {
            var fileBytes = Convert.FromBase64String(RemoveBase64Prefix(base64));

            if (fileBytes.LongLength > maxBytes)
                throw new InvalidOperationException($"{fileType} file exceeds allowed size.");

            var extension = ResolveExtension(originalFileName, requestedExtension);
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Invalid {fileType} file format.");

            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRoot, relativeFolder);
            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

            return Path.Combine(relativeFolder, fileName).Replace("\\", "/");
        }

        private static string RemoveBase64Prefix(string base64)
        {
            var commaIndex = base64.IndexOf(',');
            return commaIndex >= 0 ? base64[(commaIndex + 1)..] : base64.Trim();
        }

        private static string ResolveExtension(string? originalFileName, string? requestedExtension)
        {
            var extension = !string.IsNullOrWhiteSpace(requestedExtension)
                ? requestedExtension.Trim()
                : Path.GetExtension(originalFileName ?? string.Empty);

            if (string.IsNullOrWhiteSpace(extension))
                throw new InvalidOperationException("File extension is required.");

            if (!extension.StartsWith('.'))
                extension = $".{extension}";

            return extension.ToLowerInvariant();
        }
    }
}
