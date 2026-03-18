using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IOrderRepository
    {
        Task<PurchaseCartResponse?> PurchaseCart(PurchaseCartRequest entity);
        Task<CheckoutSummaryResponse?> GetCheckoutSummary(Guid userId);
        Task SavePayment(PaymentRequest entity);
        Task<IEnumerable<UserOrdersResponse>> GetUserOrders(Guid userId);
        Task<IEnumerable<OrderItemResponse>> GetOrderItems(int orderId);
        Task<IEnumerable<OrderItemWithProductModel>> GetOrderItemsWithProductDetails(int orderId);
        Task<IEnumerable<BusinessOrdersResponse>> GetBusinessOrders(int businessId);
        Task<BusinessOrderDetails> GetOrderedItems(OrdereDetialsRequest request);
        Task<dynamic> UpdateOrderStatus(UpdateOrderStatusRequest request);

    }
}
