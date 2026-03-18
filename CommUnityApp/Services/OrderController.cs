using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.InfrastructureLayer.Repositories;
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

        [HttpPost("Purchase_Cart")]
        public async Task<IActionResult> PurchaseCart(PurchaseCartRequest P)
        {
            var result = await _unitOfWork.Order.PurchaseCart(P);

            if (result == null)
            {
                return NotFound("Cart not found");
            }

            return Ok(result);
        }

        [HttpPost("Save_Payment")]
        public async Task<IActionResult> SavePayment([FromBody] PaymentRequest request)
        {
            await _unitOfWork.Order.SavePayment(request);

            return Ok(new
            {
                Message = "Payment saved successfully"
            });
        }

        [HttpGet("Get_UserOrders")]
        public async Task<IActionResult> GetUserOrders(Guid userId)
        {
            var orders = await _unitOfWork.Order.GetUserOrders(userId);

            return Ok(orders);
        }

        [HttpGet("Get_OrderItems")]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            var items = await _unitOfWork.Order.GetOrderItemsWithProductDetails(orderId);

            return Ok(items);
        }

        [HttpGet("Get_BusinessOrders")]
        public async Task<IActionResult> GetBusinessOrders(int businessId)
        {
            var result = await _unitOfWork.Order.GetBusinessOrders(businessId);

            return Ok(result);
        }

        [HttpPost("Get_OrderItems")]
        public async Task<IActionResult> GetOrderItems([FromBody] OrdereDetialsRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            var result = await _unitOfWork.Order.GetOrderedItems(request);

            if (result == null)
                return NotFound("Order not found");

            return Ok(result);
        }
        [HttpPost("Update_OrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            var result = await _unitOfWork.Order.UpdateOrderStatus(request);

            return Ok(result);
        }


    }
}
