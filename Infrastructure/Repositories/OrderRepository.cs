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

        public async Task<PurchaseCartResponse?> PurchaseCart(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await connection.QueryFirstOrDefaultAsync<PurchaseCartResponse>(
                "sp_PurchaseCart",
                parameters,
                commandType: CommandType.StoredProcedure
            );
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
    }
}
