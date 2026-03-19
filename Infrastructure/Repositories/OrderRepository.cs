using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class OrderRepository:IOrderRepository
    {
        private readonly IConfiguration _configuration;
        
        public OrderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<PurchaseCartResponse?> PurchaseCart(PurchaseCartRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@PaymentMethod", entity.PaymentMethod);
            parameters.Add("@Notes", entity.Notes ?? (object)DBNull.Value);

            parameters.Add("@DeliveryName", entity.DeliveryName);
            parameters.Add("@DeliveryMobile", entity.DeliveryMobile);
            parameters.Add("@DeliveryAddress", entity.DeliveryAddress);

            parameters.Add("@IsRedeemed", entity.IsRedeemed);

            var result = await connection.QueryFirstOrDefaultAsync<PurchaseCartResponse>(
                "sp_PurchaseCart",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<CheckoutSummaryResponse?> GetCheckoutSummary(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await connection.QueryFirstOrDefaultAsync<CheckoutSummaryResponse>(
                "sp_CheckoutSummary",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }


        public async Task SavePayment(PaymentRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@OrderId", entity.OrderId);
            parameters.Add("@Amount", entity.Amount);
            parameters.Add("@PaymentMethod", entity.PaymentMethod);
            parameters.Add("@TransactionId", entity.TransactionId);
            parameters.Add("@PaymentStatus", entity.PaymentStatus);

            await connection.ExecuteAsync(
                "Add_PaymentTransaction",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<UserOrdersResponse>> GetUserOrders(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await connection.QueryAsync<UserOrdersResponse>(
                "Get_UserOrders",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<OrderItemResponse>> GetOrderItems(int orderId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);

            return await connection.QueryAsync<OrderItemResponse>(
                "Get_OrderItems",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<IEnumerable<OrderItemWithProductModel>> GetOrderItemsWithProductDetails(int orderId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);

            var rows = await connection.QueryAsync<dynamic>(
                "Get_OrderItems_With_ProductDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var result = rows
                .GroupBy(r => r.OrderItemId)
                .Select(g =>
                {
                    var first = g.First();

                    var item = new OrderItemWithProductModel
                    {
                        OrderItemId = first.OrderItemId,
                        OrderId = first.OrderId,
                        Quantity = first.Quantity,
                        Price = first.Price,
                        TotalPrice = first.TotalPrice,

                        ProductDetails = new ProductDetails
                        {
                            ProductId = first.ProductId,
                            ProductName = first.ProductName,
                            Description = first.Description,
                            Price = first.ProductPrice,
                            DiscountPrice = first.DiscountPrice,
                            CategoryId = first.CategoryId,
                            BusinessId = first.BusinessId
                        },

                        ProductImages = g
                            .Where(x => x.ProductImageId != null)
                            .Select(x => new ProductImageUpload
                            {
                                ProductImageId = x.ProductImageId,
                                ProductId = x.ProductId,
                                ImagePath = x.ImagePath,
                                IsPrimary = x.IsPrimary
                            })
                            .DistinctBy(x => x.ProductImageId)
                            .ToList()
                    };

                    return item;
                })
                .ToList();

            return result;
        }

        public async Task<IEnumerable<BusinessOrdersResponse>> GetBusinessOrders(int businessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@BusinessId", businessId);

            return await connection.QueryAsync<BusinessOrdersResponse>(
                "Get_BusinessOrders",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<BusinessOrderDetails> GetOrderedItems(OrdereDetialsRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", request.OrderId);
            parameters.Add("@BusinessId", request.BusinessId);

            using var multi = await connection.QueryMultipleAsync(
                "Get_BusinessOrderItems",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var summary = await multi.ReadFirstOrDefaultAsync<BusinessOrderSummary>();
            var items = (await multi.ReadAsync<BusinessOrderItem>()).ToList();

            return new BusinessOrderDetails
            {
                OrderSummary = summary,
                Items = items
            };
        }


        public async Task<dynamic> UpdateOrderStatus(UpdateOrderStatusRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();

            parameters.Add("@OrderId", request.OrderId);
            parameters.Add("@OrderStatus", request.OrderStatus);

            var result = await connection.QueryFirstOrDefaultAsync(
                "Update_OrderStatus",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }
}
