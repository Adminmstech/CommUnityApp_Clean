using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class RewardsController : ControllerBase
    {
        private readonly ILogger<RewardsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public RewardsController(ILogger<RewardsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        [HttpGet("Get_MyCoins")]
        public async Task<IActionResult> GetMyCoins(Guid UserId)
        {
            var data = await _unitOfWork.Rewards.GetCoins(UserId);
            return Ok(data);
        }

        [HttpPost("ClaimDailyStreak")]
        public async Task<IActionResult> ClaimDailyStreak(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "UserId is required."
                });
            }

            try
            {
                var result = await _unitOfWork.Rewards.ClaimDailyStreak(userId);

                if (result == null)
                    return NotFound(new { ResultId = 0, ResultMessage = "User wallet not found." });

                if (!result.Success && result.Message.Contains("wallet", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while claiming daily streak for {UserId}", userId);
                return StatusCode(500, new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "A database error occurred. Please try again later."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while claiming daily streak for {UserId}", userId);
                return StatusCode(500, new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "An unexpected error occurred. Please try again later."
                });
            }
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

        [HttpPost("SaveReferAndEarnConfig")]
        public async Task<IActionResult> SaveReferAndEarnConfig([FromBody] ReferAndEarnConfigRequest request)
        {
            try
            {
                if (request.ReferralCoins <= 0)
                {
                    return BadRequest(new BaseResponse
                    {
                        ResultId = 0,
                        ResultMessage = "ReferralCoins must be greater than zero."
                    });
                }

                var data = await _unitOfWork.Rewards.SaveReferAndEarnConfig(request);

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

        [HttpGet("GetReferAndEarnConfig")]
        public async Task<IActionResult> GetReferAndEarnConfig(int? businessId)
        {
            try
            {
                var data = await _unitOfWork.Rewards.GetReferAndEarnConfig(businessId);

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

        [HttpPost("GenerateReferralCode")]
        public async Task<IActionResult> GenerateReferralCode([FromBody] GenerateReferralCodeRequest request)
        {
            try
            {
                if (request.ReferrerUserId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse
                    {
                        ResultId = 0,
                        ResultMessage = "ReferrerUserId is required."
                    });
                }

                var result = await _unitOfWork.Rewards.GenerateReferralCode(request);

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

        [HttpPost("ApplyReferralCode")]
        public async Task<IActionResult> ApplyReferralCode([FromBody] ApplyReferralCodeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ReferralCode))
                {
                    return BadRequest(new BaseResponse
                    {
                        ResultId = 0,
                        ResultMessage = "ReferralCode is required."
                    });
                }

                if (request.UsedByUserId == Guid.Empty)
                {
                    return BadRequest(new BaseResponse
                    {
                        ResultId = 0,
                        ResultMessage = "UsedByUserId is required."
                    });
                }

                var result = await _unitOfWork.Rewards.ApplyReferralCode(request);

                if (result == null)
                    return NotFound(new { ResultId = 0, ResultMessage = "Referral code not found." });

                if (!result.Status)
                    return BadRequest(result);

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
