using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using CommUnityApp.InfrastructureLayer.Repositories;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;
using static CommUnityApp.ApplicationCore.Models.AssignVolunteerRequest;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace CommUnityApp.Services
{


    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {

        private readonly ICommunityRepository _communityRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CommunityController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        public CommunityController(
      IWebHostEnvironment env,
      ICommunityRepository communityRepository,
      ILogger<CommunityController> logger,
      IUnitOfWork unitOfWork,
      IConfiguration config)
        {
            _env = env;
            _communityRepository = communityRepository;

            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;
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

        [HttpGet]
        [Route("GetAssignedVolunteer")]
        public async Task<IActionResult> GetAssignedVolunteer(int charityItemId)
        {
            try
            {
                var result = await _communityRepository.GetAssignedVolunteer(charityItemId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("GetCharityItemDetails")]
        public async Task<IActionResult> GetCharityItemDetails(int charityItemId)
        {
            try
            {
                var result = await _communityRepository
                    .GetCharityItemDetails(charityItemId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        status = false,
                        message = "Charity item not found."
                    });
                }

                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                if (result.ImagePaths != null && result.ImagePaths.Any())
                {
                    result.ImagePaths = result.ImagePaths
                        .Select(imagePath =>
                            !string.IsNullOrWhiteSpace(imagePath)
                                ? baseUrl + imagePath
                                : baseUrl + "/images/noimage.png")
                        .ToList();
                }
                else
                {
                    result.ImagePaths = new List<string>
            {
                baseUrl + "/images/noimage.png"
            };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("UpdateVolunteerStatus")]
        public async Task<IActionResult> UpdateVolunteerStatus([FromBody] UpdateStatusRequest request)
        {
            var result = await _communityRepository.UpdateVolunteerStatusAsync(request);

            if (result)
            {
                return Ok(new
                {
                    status = 1,
                    message = "Status updated successfully"
                });
            }

            return BadRequest(new
            {
                status = 0,
                message = "Update failed"
            });
        }

        [HttpGet("GetCharityItemRequestsList")]
        public async Task<IActionResult> GetCharityItemRequestsList(long? communityId)
        {
            long cid = 0;

            if (communityId != null)
            {
                cid = communityId.Value;
            }
            else
            {
                var sessionValue = HttpContext.Session.GetString("CommunityId");

                if (!string.IsNullOrEmpty(sessionValue))
                    cid = Convert.ToInt64(sessionValue);
            }

            if (cid == 0)
                return Unauthorized("Session expired");

            var data = await _communityRepository.GetCharityItemRequestsList(cid);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.ImagePath))
                    item.ImagePath = baseUrl + item.ImagePath;
                else
                    item.ImagePath = baseUrl + "/images/noimage.png";
            }

            return Ok(data);
        }
        [HttpPost("AddCharityItem")]
        public async Task<IActionResult> AddCharityItem(
     [FromBody] AddCharityItemModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "Request body is null."
                    });
                }

                int receivedImageCount = model.ImagePaths?.Count ?? 0;

                // TEMPORARY DEBUG CHECK:
                // Do not allow success if API received no images.
                if (receivedImageCount == 0)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage =
                            "No images received in ImagePaths. Check your request JSON.",
                        ReceivedImageCount = 0
                    });
                }

                if (receivedImageCount > 5)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage =
                            "Maximum 5 images are allowed."
                    });
                }

                var result =
                    await _communityRepository.AddCharityItem(model);

                int charityItemId = result.CharityItemId;

                if (charityItemId <= 0)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage =
                            "Failed to create charity item."
                    });
                }

                var insertedImages = new List<object>();

                string folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "charity",
                    charityItemId.ToString()
                );

                Directory.CreateDirectory(folderPath);

                foreach (string imageBase64 in model.ImagePaths)
                {
                    if (string.IsNullOrWhiteSpace(imageBase64))
                    {
                        continue;
                    }

                    string base64String = imageBase64.Trim();

                    string extension = ".jpg";

                    if (base64String.StartsWith(
                        "data:image/png",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        extension = ".png";
                    }
                    else if (base64String.StartsWith(
                        "data:image/webp",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        extension = ".webp";
                    }
                    else if (base64String.StartsWith(
                        "data:image/gif",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        extension = ".gif";
                    }
                    else if (
                        base64String.StartsWith(
                            "data:image/jpeg",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        base64String.StartsWith(
                            "data:image/jpg",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        extension = ".jpg";
                    }

                    int commaIndex = base64String.IndexOf(',');

                    if (commaIndex >= 0)
                    {
                        base64String =
                            base64String.Substring(commaIndex + 1);
                    }

                    byte[] imageBytes;

                    try
                    {
                        imageBytes =
                            Convert.FromBase64String(base64String);
                    }
                    catch (FormatException)
                    {
                        return BadRequest(new
                        {
                            ResultId = 0,
                            ResultMessage =
                                "Invalid Base64 image data."
                        });
                    }

                    string fileName =
                        $"{Guid.NewGuid()}{extension}";

                    string physicalFilePath = Path.Combine(
                        folderPath,
                        fileName
                    );

                    await System.IO.File.WriteAllBytesAsync(
                        physicalFilePath,
                        imageBytes
                    );

                    string savedImagePath =
                        $"/uploads/charity/{charityItemId}/{fileName}";

                    // INSERT IMAGE AND GET INSERTED ID
                    int charityItemImageId =
                        await _communityRepository.AddCharityItemImage(
                            charityItemId,
                            savedImagePath
                        );

                    // If SP did not return a valid inserted ID, throw error
                    if (charityItemImageId <= 0)
                    {
                        throw new Exception(
                            $"Image insert failed. CharityItemId: {charityItemId}, Path: {savedImagePath}"
                        );
                    }

                    insertedImages.Add(new
                    {
                        CharityItemImageId = charityItemImageId,
                        ImagePath = savedImagePath
                    });
                }

                // If we received images but none were inserted,
                // DO NOT return 200 OK.
                if (insertedImages.Count == 0)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage =
                            "Images were received but no images were inserted.",
                        ReceivedImageCount = receivedImageCount,
                        InsertedImageCount = 0
                    });
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage =
                        "Item and images added successfully.",

                    CharityItemId = charityItemId,

                    ReceivedImageCount = receivedImageCount,

                    InsertedImageCount = insertedImages.Count,

                    Images = insertedImages
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
        [HttpPost("RequestCharityItem")]
        public async Task<IActionResult> RequestCharityItem([FromBody] RequestCharityItemModel model)
        {
            try
            {
                if (model.RequestedQuantity <= 0)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "Invalid quantity.",
                        Status = 0
                    });
                }

                var result = await _communityRepository.RequestCharityItem(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = 0
                });
            }
        }

        [HttpGet("GetRequestedUsersByItemId")]
        public async Task<IActionResult> GetRequestedUsersByItemId(int charityItemId)
        {
            try
            {
                var data = await _communityRepository.GetRequestedUsersByItemId(charityItemId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AssignVolunteerToRequest")]
        public async Task<IActionResult> AssignVolunteerToRequest([FromBody] AssignVolunteerModel model)
        {
            await _communityRepository.AssignVolunteerToRequest(model);
            return Ok(new { message = "Volunteer Assigned Successfully" });
        }

        [HttpGet("GetAllCharityItems")]
        public async Task<IActionResult> GetAllCharityItems()
        {
            try
            {
                var data = await _communityRepository.GetAllCharityItems();

                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                foreach (var item in data)
                {
                    if (item.ImagePaths != null && item.ImagePaths.Any())
                    {
                        item.ImagePaths = item.ImagePaths
                            .Where(path => !string.IsNullOrWhiteSpace(path))
                            .Select(path => baseUrl + path)
                            .ToList();
                    }

                    if (item.ImagePaths == null || !item.ImagePaths.Any())
                    {
                        item.ImagePaths = new List<string>
                {
                    baseUrl + "/images/noimage.png"
                };
                    }
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetMyRequestedItems")]
        public async Task<IActionResult> GetMyRequestedItems(Guid userId)
        {
            try
            {
                var data = await _communityRepository.GetMyRequestedItems(userId);

                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                foreach (var item in data)
                {
                    if (!string.IsNullOrEmpty(item.ImagePath))
                        item.ImagePath = baseUrl + item.ImagePath;
                    else
                        item.ImagePath = baseUrl + "/images/noimage.png";
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Get_Communities")]
        public async Task<IActionResult> GetCommunities()
        {
            var data = await _unitOfWork.Community.GetCommunities();
            return Ok(data);
        }
        [HttpGet("Get_DashboardData")]
        public async Task<IActionResult> GetDashboardData(Guid userId)
        {
            try
            {
                var auctionsTask =
                    _unitOfWork.Auction.GetTop5Auctions();

                var rewardsTask =
                    _unitOfWork.Rewards.GetCoins(userId);

                var postedEventsTask =
                    _unitOfWork.Events.GetTopFivePostedEventsByUser(userId);

                var communityPostsTask =
                    _unitOfWork.Community.GetTopFiveCommunityPostsByUser(userId);

                var messageBoardTask =
                    _unitOfWork.Notification.GetTopFiveMessageBoardPosts();

                var promotionsTask =
                    _unitOfWork.Product.GetTopFiveProductPromotions();

                var businessPostsTask =
                    _unitOfWork.Business.GetTopFiveBusinessPosts();

                await Task.WhenAll(
                    auctionsTask,
                    rewardsTask,
                    postedEventsTask,
                    communityPostsTask,
                    messageBoardTask,
                    promotionsTask,
                    businessPostsTask);

                var auctions = await auctionsTask;

                var rewards = await rewardsTask;

                var postedEvents = await postedEventsTask;

                var communityPosts = await communityPostsTask;

                var messageBoardPosts = await messageBoardTask;

                var promotions = await promotionsTask;

                var businessPosts = await businessPostsTask;

                if (auctions != null && auctions.Any())
                {
                    var auctionIds = auctions
                        .Select(x => x.AuctionId)
                        .ToList();

                    var auctionImages =
                        await _unitOfWork.Auction
                        .GetAuctionImagesByIds(auctionIds);

                    foreach (var auction in auctions)
                    {
                        auction.AuctionImages = auctionImages
                            .Where(x => x.AuctionId == auction.AuctionId)
                            .ToList();
                    }
                }

                return Ok(new DashboardResponse
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Data = new DashboardData
                    {
                        Rewards = rewards,

                        Auctions = auctions ?? new List<AuctionListModel>(),

                        PostedEvents = postedEvents ?? new List<UserPostedEventModel>(),

                        CommunityPosts = communityPosts ?? new List<CommunityPostModel>(),

                        MessageBoardPosts = messageBoardPosts ?? new List<PostResponse>(),

                        TopProductPromotions = promotions ?? new List<TopProductPromotionEntity>(),

                        BusinessPosts = businessPosts ?? new List<BusinessPostEntity>()
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new DashboardResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }
        [HttpGet]
        [Route("GetItemCategories")]
        public async Task<IActionResult> GetItemCategories()
        {
            var data = await _communityRepository.GetItemCategories();
            return Ok(data);
        }

        [HttpGet("GetMembersByCommunity")]
        public async Task<IActionResult> GetMembersByCommunity(int communityId)
        {
            if (communityId == 0)
                return BadRequest("Invalid CommunityId");

            var data = await _communityRepository.GetMembersByCommunity(communityId);

            return Ok(data);
        }


        [HttpGet("GetCommunityUsers")]
        public async Task<IActionResult> GetCommunityUsers(long communityId)
        {
            var users = await _communityRepository.GetCommunityUsers(communityId);
            return Ok(users);
        }
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] CommunityMessageModel model)
        {
            try
            {
                var communityIdStr = HttpContext.Session.GetString("CommunityId");

                if (string.IsNullOrEmpty(communityIdStr))
                    return Unauthorized("Session expired");

                long communityId = Convert.ToInt64(communityIdStr);

                if (model.ReceiverUserId == Guid.Empty)
                    return BadRequest("ReceiverUserId required");

                if (string.IsNullOrWhiteSpace(model.MessageText) && model.ImageFile == null)
                    return BadRequest("Message or Image is required");

                string imagePath = "";

                if (model.ImageFile != null)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/chat");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await model.ImageFile.CopyToAsync(stream);

                    imagePath = "/uploads/chat/" + fileName;
                }

                var id = await _communityRepository.SendMessage(
                    communityId,
                    model.ReceiverUserId,
                    model.MessageText ?? "",
                    imagePath
                );

                return Ok(new { message = "Sent", id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetMessages")]
        public async Task<IActionResult> GetMessages(long communityId, Guid receiverUserId)
        {
            if (communityId == 0)
                return BadRequest("CommunityId required");

            if (receiverUserId == Guid.Empty)
                return BadRequest("ReceiverUserId required");

            var data = await _communityRepository.GetMessages(communityId, receiverUserId);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var item in data)
            {
                if (item.ImagePath != null)
                    item.ImagePath = baseUrl + item.ImagePath;
            }

            return Ok(data);
        }

        [HttpGet("Get_CommunityCategories")]
        public async Task<IActionResult> GetCommunityCategories()
        {
            try
            {
                var result = await _unitOfWork.Community.GetCommunityCategoriesAsync();

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

        [HttpPost("Add_Community")]
        public async Task<IActionResult> AddCommunity([FromBody] AddCommunityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string logoPath = request.Logo;

                // Save Logo
                if (!string.IsNullOrWhiteSpace(request.LogoBase64))
                {
                    if (!TryConvertFromBase64(request.LogoBase64, out byte[] fileBytes))
                        return BadRequest("Invalid logo format.");

                    if (fileBytes.Length > 2097152)
                        return BadRequest("Logo size exceeds 2MB limit.");

                    string fileName = $"{Guid.NewGuid():N}.jpg";
                    string directoryPath = Path.Combine("wwwroot", "CommunityLogos");
                    Directory.CreateDirectory(directoryPath);

                    string localFilePath = Path.Combine(directoryPath, fileName);
                    await System.IO.File.WriteAllBytesAsync(localFilePath, fileBytes);

                    logoPath = $"CommunityLogos/{fileName}";
                }

                var result = await _unitOfWork.Community.AddCommunityAsync(new AddCommunityRequest
                {
                    CommunityId = request.CommunityId,
                    CommunityCategoryId = request.CommunityCategoryId,
                    CommunityName = request.CommunityName,
                    Logo = logoPath,
                    Description = request.Description,
                    ContactName = request.ContactName,
                    ContactEmail = request.ContactEmail,
                    ContactPhone = request.ContactPhone,
                    Website = request.Website,
                    Address = request.Address,
                    OtherInfo = request.OtherInfo,
                    UserName = request.UserName,
                    Password = request.Password,
                    IsActive = request.IsActive
                });

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


        [HttpGet("Get_AllCommunities")]
        public async Task<IActionResult> GetAllCommunities()
        {
            var data = await _unitOfWork.Community.GetCommunitiesAsync();
            return Ok(data);
        }

        [HttpGet("Get_CommunityDetails")]
        public async Task<IActionResult> GetCommunityDetails(int communityId)
        {
            var data = await _unitOfWork.Community.GetCommunityDetailsAsync(communityId);
            return Ok(data);
        }

        [HttpGet("Get_CommunitiesByCategory")]
        public async Task<IActionResult> GetCommunitybyCategory(int communityCategoryId)
        {
            var data = await _unitOfWork.Community.GetCommunitiesByCategoryAsync(communityCategoryId);
            return Ok(data);
        }
         
        [HttpPost("Update_UserCommunity")]
        public async Task<IActionResult> UpdateUserCommunityAsync(UpdateUserCommunityRequest request)
        {
            var data = await _unitOfWork.Community.UpdateUserCommunityAsync(request);
            return Ok(data);
        }
        [HttpPost("AddCommunityPost")]
        public async Task<IActionResult> AddCommunityPost( 
    [FromForm] CommunityPostModel model)
        { 
            try
            {
                string imagePath = "";


                if (model.ImageFile != null)
                {
                    string folderPath =
                        Path.Combine(
                            _env.WebRootPath,
                            "Uploads",
                            "CommunityPosts");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName =
                        Guid.NewGuid().ToString()
                        +
                        Path.GetExtension(
                            model.ImageFile.FileName);

                    string fullPath =
                        Path.Combine(folderPath,
                                     fileName);

                    using (var stream =
                        new FileStream(fullPath,
                                       FileMode.Create))
                    {
                        await model.ImageFile
                            .CopyToAsync(stream);
                    }

                    imagePath =
                        "/Uploads/CommunityPosts/"
                        + fileName;
                }



                model.ImagePath = imagePath;

                var result =
                    await _communityRepository
                    .AddCommunityPost(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("GetCommunityPosts/{communityId}")]
        public async Task<IActionResult> GetPosts(
            int communityId)
        {
            var result =
                await _communityRepository
                .GetCommunityPosts(communityId);

            return Ok(new
            {
                Status = true,
                Data = result
            });
        }

        [HttpDelete("DeleteCommunityPost/{postId}")]
        public async Task<IActionResult> DeleteCommunityPost(int postId)
        {
            var result =
                await _communityRepository
                .DeleteCommunityPost(postId);

            return Ok(result);
        }

        [HttpGet("GetCommunityPostsByUser")]
        public async Task<IActionResult> GetCommunityPostsByUser(Guid userId)
        {
            var result =
           await _communityRepository.GetCommunityPostsByUser(userId);

            return Ok(new
            {
                status = true,
                data = result
            });
        }
        
        [HttpGet("GetUserCommunities")]
        public async Task<IActionResult> GetUserCommunities(Guid userId)
        {
            try
            {
                var result = await _communityRepository
                    .GetUserCommunitiesAsync(userId); 

                return Ok(new
                { 
                    Status = 1,
                    Message = "Success",  
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Status = 0,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("UpdateCharityItem")]
        public async Task<IActionResult> UpdateCharityItem(
    [FromBody] UpdateCharityItemModel model)
        {
            try
            {
                var result = await _communityRepository.UpdateCharityItem(model);

                if (result == null)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "Unable to update charity item.",
                        Status = 0
                    });
                }

                if (result.Status != 1)
                {
                    return Ok(result);
                }

                var savedImagePaths = new List<string>();

                if (model.ImagePaths != null && model.ImagePaths.Any())
                {
                    string folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "charity",
                        model.CharityItemId.ToString());

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    foreach (var image in model.ImagePaths)
                    {
                        if (string.IsNullOrWhiteSpace(image))
                        {
                            continue;
                        }

                        string base64String = image.Trim();

                        if (base64String.Contains(","))
                        {
                            base64String = base64String.Substring(
                                base64String.IndexOf(",") + 1);
                        }

                        base64String = base64String
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Trim();

                        byte[] imageBytes;

                        try
                        {
                            imageBytes = Convert.FromBase64String(base64String);
                        }
                        catch (FormatException)
                        {
                            return BadRequest(new
                            {
                                ResultId = 0,
                                ResultMessage = "One of the provided images contains invalid Base64 data.",
                                Status = 0
                            });
                        }

                      
                        string fileName =
                            Guid.NewGuid().ToString() + ".jpg";

                        string filePath = Path.Combine(
                            folderPath,
                            fileName);

                        await System.IO.File.WriteAllBytesAsync(
                            filePath,
                            imageBytes);

                        string imagePath =
                            "/uploads/charity/"
                            + model.CharityItemId
                            + "/"
                            + fileName;

                        await _communityRepository.UpdateCharityItemImage(
                            model.CharityItemId,
                            imagePath);

                        savedImagePaths.Add(imagePath);
                    }
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Charity item updated successfully",
                    Status = 1,
                    CharityItemId = model.CharityItemId,
                    ImagePaths = savedImagePaths
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = 0
                });
            }
        }

        [HttpGet]
        [Route("GetCharityItemsByUserId")]
       public async Task<IActionResult> GetCharityItemsByUserId(Guid userId)
        {
            var result =
                await _communityRepository.GetCharityItemsByUserId(userId);

            return Ok(result);
        }


        [HttpGet("Get_AllCharityItems")]
        public async Task<IActionResult> Get_AllCharityItems()
        {
            try
            {
                var result = await _unitOfWork.Community.GetAllCharityItems();

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

        [HttpGet("GetTopFivePostedEventsByUser")]
        public async Task<IActionResult> GetPostedEventsByUser(Guid userId)
        {
            var data = await _unitOfWork.Events
                .GetPostedEventsByUser(userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

       
        [HttpGet("GetTopFiveCommunityPostsByUser")]
        public async Task<IActionResult> GetTopFiveCommunityPostsByUser(Guid userId)
        { 
            var data = await _unitOfWork.Community
                .GetTopFiveCommunityPostsByUser(userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpDelete]
        [Route("DeleteCharityItem")]
        public async Task<IActionResult> DeleteCharityItem(long charityItemId)
        {
            try
            {
                var result = await _communityRepository
                    .DeleteCharityItem(charityItemId);

                if (result == null)
                {
                    return Ok(new
                    {
                        ResultId = 0,
                        ResultMessage = "Unable to delete charity item.",
                        Status = 0
                    });
                }

                if (result.Status != 1)
                {
                    return Ok(new
                    {
                        ResultId = 0,
                        ResultMessage = result.Message,
                        Status = 0
                    });
                }

                string folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "charity",
                    charityItemId.ToString());

                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = result.Message,
                    Status = 1,
                    CharityItemId = charityItemId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = 0
                });
            }

        }

        [HttpGet("GetCharityItemsByUserCommunities")]
        public async Task<IActionResult> GetCharityItemsByUserCommunities(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "UserId is required.",
                        Status = false
                    });
                }

                var result = await _communityRepository
                    .GetCharityItemsByUserCommunities(userId);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Charity items fetched successfully.",
                    Status = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }
    }


}

