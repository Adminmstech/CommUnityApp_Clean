using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{

  
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {

        private readonly ICommunityRepository _communityRepository;
        private readonly ILogger<CommunityController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;



        public CommunityController(ILogger<CommunityController> logger, IUnitOfWork unitOfWork, IConfiguration config)
        {
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
                var businesses = await _unitOfWork.Business.GetAllBusinesses();

                // ⭐ Rewards
                var rewards = await _unitOfWork.Rewards.GetCoins(userId);

                // Auction Images
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
                    Rewards = rewards,  // ⭐ added
                    Events = events,
                    Auctions = auctions,
                    Communities = communities,
                    Businesses = businesses
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

    }
}
