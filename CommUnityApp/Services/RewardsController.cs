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
    }
}
