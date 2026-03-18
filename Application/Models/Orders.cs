using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Orders
    {
        public int OrderId { get; set; }

        public Guid UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public int CoinsUsed { get; set; }

        public int CoinsEarned { get; set; }

        public decimal FinalAmount { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public string OrderStatus { get; set; }

        public string DeliveryName { get; set; }

        public string DeliveryMobile { get; set; }

        public string DeliveryAddress { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class PurchaseCartRequest
    {
        public Guid UserId { get; set; }

        public string PaymentMethod { get; set; }

        public string Notes { get; set; }

        public string DeliveryName { get; set; }

        public string DeliveryMobile { get; set; }

        public string DeliveryAddress { get; set; }

        public bool IsRedeemed { get; set; }
    }
    public class PurchaseCartResponse
    {
        public int OrderId { get; set; }

        public decimal CartTotal { get; set; }

        public int CoinsUsed { get; set; }

        public int CoinsEarned { get; set; }

        public decimal FinalAmount { get; set; }
    }

    public class CheckoutSummaryResponse
    {
        public decimal CartTotal { get; set; }

        public int CoinsAvailable { get; set; }

        public int CoinsUsed { get; set; }

        public decimal CoinDiscount { get; set; }

        public decimal FinalAmount { get; set; }

        public int CoinsEarned { get; set; }
    }

    public class PaymentRequest
    {
        public Guid UserId { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }

        public string PaymentStatus { get; set; }
    }


    public class UserOrdersResponse
    {
        public int OrderId { get; set; }

        public Guid UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public int CoinsUsed { get; set; }

        public int CoinsEarned { get; set; }

        public decimal FinalAmount { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public string OrderStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ProductName { get; set; }

        public string ImagePath { get; set; }
    }

    public class OrderItemResponse
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        public string ImagePath { get; set; }
    }

    public class OrderItemWithProductModel
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        public ProductDetails ProductDetails { get; set; }

        public List<ProductImageUpload> ProductImages { get; set; } = new();
    }

    public class BusinessOrdersResponse
    {
        public int OrderId { get; set; }

        public string CustomerName { get; set; }

        public int TotalItems { get; set; }

        public decimal OrderAmount { get; set; }

        public int CoinsUsed { get; set; }

        public int CoinsEarned { get; set; }

        public decimal FinalAmount { get; set; }
        public string OrderStatus { get; set; }

        public DateTime OrderDate { get; set; }
    }

    public class BusinessOrderSummary
    {
        public int OrderId { get; set; }

        public int TotalItems { get; set; }

        public decimal BusinessOrderAmount { get; set; }

        public int CoinsUsed { get; set; }

        public int CoinsEarned { get; set; }

        public decimal FinalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class BusinessOrderItem
    {
        public int OrderItemId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        public string ImagePath { get; set; }
    }
    public class BusinessOrderDetails
    {
        public BusinessOrderSummary OrderSummary { get; set; }

        public List<BusinessOrderItem> Items { get; set; }
    }
    public class OrdereDetialsRequest
    {
        public int OrderId { get; set; }
        public int BusinessId { get; set; }
    }
    public class UpdateOrderStatusRequest
    {
        public int OrderId { get; set; }

        public string OrderStatus { get; set; }
    }
}
