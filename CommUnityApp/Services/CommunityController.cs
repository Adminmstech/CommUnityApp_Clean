using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using CommUnityApp.InfrastructureLayer.Repositories;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

             
                var result =
                    await _communityRepository
                    .AddCharityItem(model, "");

                int charityItemId =
                    result.CharityItemId;        

                if (!string.IsNullOrEmpty(model.ImagePath))
                {
                    string folderPath =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads",
                            "charity",
                            charityItemId.ToString());


                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                  
                    string base64String =
                        model.ImagePath;

                    if (base64String.Contains(","))
                    {
                        base64String =
                            base64String.Substring(
                                base64String.IndexOf(",") + 1);
                    }                
                    string fileName =
                        Guid.NewGuid().ToString()
                        + Path.GetExtension(model.FileName);

                    string filePath =
                        Path.Combine(folderPath, fileName);


                    byte[] imageBytes =
                        Convert.FromBase64String(base64String);

                    System.IO.File.WriteAllBytes(
                        filePath,
                        imageBytes);


                    imagePath =
                        "/uploads/charity/"
                        + charityItemId
                        + "/"
                        + fileName;

                    await _communityRepository
                        .UpdateCharityItemImage(
                            charityItemId,
                            imagePath);
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage =
                        "Item added successfully",

                    CharityItemId = charityItemId,

                    ImagePath = imagePath
                });
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
                    Products = productList 
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
        public async Task<IActionResult> UpdateCharityItem([FromBody] UpdateCharityItemModel model)
       
        {
            try
            {
                string imagePath = "";

                var result =
                    await _communityRepository
                    .UpdateCharityItem(model);

                if (result.Status != 1)
                {
                    return Ok(result);
                }

                if (!string.IsNullOrEmpty(model.ImagePath)
                    && model.ImagePath.Contains("base64"))
                {
                    string folderPath =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads",
                            "charity",
                            model.CharityItemId.ToString());

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string base64String = model.ImagePath;

                    if (base64String.Contains(","))
                    {
                        base64String =
                            base64String.Substring(
                                base64String.IndexOf(",") + 1);
                    }

                    string fileName =
                        Guid.NewGuid().ToString()
                        + Path.GetExtension(model.ImagePath);

                    string filePath =
                        Path.Combine(folderPath, fileName);

                    byte[] imageBytes =
                        Convert.FromBase64String(base64String);

                    System.IO.File.WriteAllBytes(
                        filePath,
                        imageBytes);

                    imagePath =
                        "/uploads/charity/"
                        + model.CharityItemId
                        + "/"
                        + fileName;

                    await _communityRepository
                        .UpdateCharityItemImage(
                            model.CharityItemId,
                            imagePath);
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Charity item updated successfully",
                    CharityItemId = model.CharityItemId,
                    ImagePath = imagePath
                });
            } 
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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


    }


}

