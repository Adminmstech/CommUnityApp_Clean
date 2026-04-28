using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Dapper;
using static CommUnityApp.ApplicationCore.Models.AssignVolunteerRequest;

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
                var result = await _communityRepository.GetCharityItemDetails(charityItemId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
        public async Task<IActionResult> AddCharityItem([FromBody] AddCharityItemModel model)
        {
            try
            {
                string imagePath = "";

                var charityItemId = await _communityRepository.AddCharityItem(model, "");

                if (!string.IsNullOrEmpty(model.ImagePath))
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "charity", charityItemId.ToString());

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string filePath = Path.Combine(folderPath, model.FileName);

                    byte[] imageBytes = Convert.FromBase64String(model.ImagePath);
                    System.IO.File.WriteAllBytes(filePath, imageBytes);

                    imagePath = "/uploads/charity/" + charityItemId + "/" + model.FileName;

                    //await _communityRepository.UpdateCharityItemImage(charityItemId, imagePath);
                }

                return Ok(new { message = "Item added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RequestCharityItem")]
        public async Task<IActionResult> RequestCharityItem([FromBody] RequestCharityItemModel model)
        {
            try
            {
                if (model.RequestedQuantity <= 0)
                    return BadRequest("Invalid quantity");

                var requestId = await _communityRepository.RequestCharityItem(model);

                return Ok(new
                {
                    message = "Request submitted successfully",
                    requestId = requestId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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


        //For mobile app dashboard//
        [HttpGet("Get_DashboardData")]
        public async Task<IActionResult> GetDashboardData(Guid userId)
        {
            var response = new DashboardResponse();

            try
            {
                var events = await _unitOfWork.Events.GetTop5Events();
                var auctions = await _unitOfWork.Auction.GetTop5Auctions();
                var communities = await _unitOfWork.Community.GetCommunities();

                // ⭐ Rewards
                var rewards = await _unitOfWork.Rewards.GetCoins(userId);

                // ⭐ Products with Images
                var products = await _unitOfWork.Product.GetAllProducts();
                var productList = new List<ProductWithImagesModel>();

                foreach (var product in products)
                {
                    var images = await _unitOfWork.Product.GetProductImageById(product.ProductId);

                    var productResponse = new ProductWithImagesModel
                    {
                        Product = product,
                        Images = new List<ProductImageUpload>() // ✅ IMPORTANT (avoid null)
                    };

                    if (images != null && images.Count > 0)
                    {
                        foreach (var image in images)
                        {
                            productResponse.Images.Add(new ProductImageUpload
                            {
                                ProductImageId = image.ProductImageId,
                                ProductId = image.ProductId,
                                ImagePath = image.ImagePath,
                                IsPrimary = image.IsPrimary
                            });
                        }
                    }

                    productList.Add(productResponse);
                }

                // ⭐ Auction Images
                var auctionIds = auctions.Select(a => a.AuctionId).ToList();
                var auctionImages = await _unitOfWork.Auction.GetAuctionImagesByIds(auctionIds);

                foreach (var auction in auctions)
                {
                    auction.AuctionImages = auctionImages
                        .Where(img => img.AuctionId == auction.AuctionId)
                        .ToList();
                }

                response.ResultId = 1;
                response.ResultMessage = "Success";
                response.Data = new DashboardData
                {
                    Rewards = rewards,
                    Events = events,
                    Auctions = auctions,
                    Communities = communities,
                    Products = productList // ✅ FIXED
                };

                return Ok(new List<DashboardResponse> { response });
            }
            catch (Exception ex)
            {
                response.ResultId = 0;
                response.ResultMessage = ex.Message;

                return Ok(new List<DashboardResponse> { response });
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
    }
}
