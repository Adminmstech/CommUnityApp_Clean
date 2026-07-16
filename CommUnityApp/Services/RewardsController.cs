using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class RewardsController : ControllerBase
    {
        private readonly ILogger<RewardsController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public RewardsController(ILogger<RewardsController> logger, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;
        }

        [HttpGet("Get_MyCoins")]
        public async Task<IActionResult> GetMyCoins(Guid UserId)
        {
            var data = await _unitOfWork.Rewards.GetCoins(UserId);
            return Ok(data);
        }


        [HttpPost("SaveShareRewardConfig")]
        public async Task<IActionResult> SaveShareRewardConfig( SaveShareRewardConfigRequest request)
        {
            try
            {
                var data = await _unitOfWork.Rewards.SaveShareRewardConfig(request);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpGet("GetShareRewardConfig")]
        public async Task<IActionResult> GetShareRewardConfig(int businessId)
        {
            try
            {
                var data = await _unitOfWork.Rewards.GetShareRewardConfig(businessId);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpPost("RewardShare")]
        public async Task<IActionResult> RewardShare(ShareRewardRequest request)
        {
            try
            {
                var result = await _unitOfWork.Rewards.RewardShare(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpGet("GetShareRewards")]
        public async Task<IActionResult> GetShareRewards(int businessId)
        {
            try
            {
                var data = await _unitOfWork.Rewards.GetShareRewards(businessId);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }


        [HttpGet("GetUserShareRewards")]
        public async Task<IActionResult> GetUserShareRewards(Guid userId)
        {
            try
            {
                var data = await _unitOfWork.Rewards .GetUserShareRewards(userId);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }
    }
}
