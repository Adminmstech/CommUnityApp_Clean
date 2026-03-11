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
    public class RewardsRepository:IRewardsRepository
    {
        private readonly IConfiguration _configuration;

        public RewardsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<Rewards?> GetCoins(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var result = await connection.QueryFirstOrDefaultAsync<Rewards>(
                "Get_UserEarnedCoins",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

    }
}
