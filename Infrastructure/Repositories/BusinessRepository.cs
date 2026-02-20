using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly IConfiguration _configuration;

        public BusinessRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<BusinessLoginResponse> LoginAsync(BusinessLoginRequest request)
        {
            using var connection = Connection;
            return await connection.QueryFirstOrDefaultAsync<BusinessLoginResponse>(
                "sp_BusinessLogin",
                new
                {
                    Email = request.Email,
                    Password = request.Password
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<BaseResponse> RegisterAsync(AddUpdateBusinessRequest request)
        {
            using var connection = Connection;
            return await connection.QueryFirstOrDefaultAsync<BaseResponse>(
                "sp_BusinessRegister",
                new
                {
                    request.CategoryId,
                    request.BusinessName,
                    request.BusinessNumber,
                    request.OwnerName,
                    request.Email,
                    request.Phone,
                    request.Address,
                    request.City,
                    request.State,
                    request.Country,
                    request.Suburb,
                    request.Info,
                    request.Logo,
                    request.Password
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
