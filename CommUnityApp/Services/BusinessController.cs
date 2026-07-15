using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
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

                string logoPath = request.Logo;

                // Save New Logo Only If Selected
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
                    WebLink = request.WebLink,
                    Password = request.Password,
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
        public async Task<IActionResult> GetBusinesses(Guid userId)
        {
            var data = await _unitOfWork.Business.GetAllBusinesses(userId);
            return Ok(data);
        }


        [HttpGet("Get_BusinessDetails")]
        public async Task<IActionResult> GetBusinessDetails(int BusinessId)
        {
            var data = await _unitOfWork.Business.GetBusinessDetails(BusinessId);
            return Ok(data);
        }

        [HttpGet("Get_BusinessCustomer")]
        public async Task<IActionResult> GetBusinessCustomer(int BusinessId)
        {
            var data = await _unitOfWork.Business.GetBusinessCustomers(BusinessId);
            return Ok(data);
        }

        [HttpGet("Get_BusinessCategory")]
        public async Task<IActionResult> GetBusinessCategory()
        {
            var data = await _unitOfWork.Business.GetBusinesscategory();
            return Ok(data);
        }
        [HttpPost("AddRemoveFavouriteBusiness")]
        public async Task<IActionResult>AddRemoveFavouriteBusiness([FromBody] FavouriteBusinessRequest request)
        {
            var result = await _unitOfWork.Business.AddRemoveFavouriteBusiness(
                    request.BusinessId,
                    request.UserId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = result.ResultMessage,
                Status = true,
                Data = result
            });
        }

        [HttpGet("GetFavouriteBusinesses")]
        public async Task<IActionResult> GetFavouriteBusinesses( Guid userId)
        {
            var data = await _unitOfWork.Business
                .GetFavouriteBusinesses(userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetTopFiveBusinessPosts")]
        public async Task<IActionResult> GetTopFiveBusinessPosts()
        {
            var data =
                await _unitOfWork.Business.GetTopFiveBusinessPosts();

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetBusinessPostDetails")]
        public async Task<IActionResult> GetBusinessPostDetails(long postId)
        {
            var data =
                await _unitOfWork.Business.GetBusinessPostDetails(postId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpGet("GetAllBusinessPosts")]
        public async Task<IActionResult> GetAllBusinessPosts(long businessId)
        {
            var data =
                await _unitOfWork.Business.GetAllBusinessPosts(businessId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }



        #region Business Wallet

        [HttpPost("AllocateBusinessCoins")]
        public async Task<IActionResult> AllocateBusinessCoins(
            [FromBody] AllocateBusinessCoinsRequest request)
        {
            try
            {
                var result =
                    await _unitOfWork.Business.AllocateBusinessCoins(request);

                return Ok(new
                {
                    ResultId = result.ResultId,
                    ResultMessage = result.ResultMessage,
                    Status = result.ResultId > 0
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

        [HttpPost("RewardMemberFromBusiness")]
        public async Task<IActionResult> RewardMemberFromBusiness(
            [FromBody] RewardMemberRequest request)
        {
            try
            {
                var result =
                    await _unitOfWork.Business.RewardMemberFromBusiness(request);

                return Ok(new
                {
                    ResultId = result.ResultId,
                    ResultMessage = result.ResultMessage,
                    Status = result.ResultId > 0
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

        [HttpPost("AdjustBusinessWallet")]
        public async Task<IActionResult> AdjustBusinessWallet(
            [FromBody] AdjustBusinessWalletRequest request)
        {
            try
            {
                var result =
                    await _unitOfWork.Business.AdjustBusinessWallet(request);

                return Ok(new
                {
                    ResultId = result.ResultId,
                    ResultMessage = result.ResultMessage,
                    Status = result.ResultId > 0
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

        [HttpGet("GetBusinessWallet")]
        public async Task<IActionResult> GetBusinessWallet(int businessId)
        {
            try
            {
                var data =
                    await _unitOfWork.Business.GetBusinessWallet(businessId);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = data
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

        [HttpGet("GetBusinessWalletTransactions")]
        public async Task<IActionResult> GetBusinessWalletTransactions(
            int businessId)
        {
            try
            {
                var data =
                    await _unitOfWork.Business
                        .GetBusinessWalletTransactions(businessId);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = data
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

        [HttpGet("GetTransactionTypes")]
        public async Task<IActionResult> GetTransactionTypes()
        {
            try
            {
                var data =
                    await _unitOfWork.Business.GetTransactionTypes();

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",
                    Status = true,
                    Data = data
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

        #endregion
    }


}
