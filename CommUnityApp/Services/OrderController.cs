using CommUnityApp.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public OrderController(ILogger<OrderController> logger, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;
        }

        [HttpGet("Checkout_Summary")]
        public async Task<IActionResult> GetCheckoutSummary(Guid userId)
        {
            var result = await _unitOfWork.Order.GetCheckoutSummary(userId);

            if (result == null)
            {
                return NotFound("Cart not found");
            }

            return Ok(result);
        }

        [HttpGet("Purchase_Cart")]
        public async Task<IActionResult> PurchaseCart(Guid userId)
        {
            var result = await _unitOfWork.Order.PurchaseCart(userId);

            if (result == null)
            {
                return NotFound("Cart not found");
            }

            return Ok(result);
        }
    }
}
