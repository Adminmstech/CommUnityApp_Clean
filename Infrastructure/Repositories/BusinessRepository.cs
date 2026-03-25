using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
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



        public async Task<BusinessAddResponse> AddBusinessAsync(AddBusinessRequest request)
        {
            using var connection = Connection;

            return await connection.QueryFirstOrDefaultAsync<BusinessAddResponse>(
                "Add_Business",
                new
                {
                    BusinessId = request.BusinessId,
                    CategoryId = request.CategoryId,
                    BusinessName = request.BusinessName,
                    BusinessNumber = request.BusinessNumber,
                    OwnerName = request.OwnerName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    Country = request.Country,
                    Suburb = request.Suburb,
                    Logo = request.Logo,
                    Info = request.Info,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsVerified = request.IsVerified,
                    IsActive = request.IsActive
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<BusinessDetailsDto>> GetAllBusinesses()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<BusinessDetailsDto>("Get_AllBusinesses", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<BusinessDetailsDto> GetBusinessDetails(int BusinessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@BusinessId", BusinessId);

            var result = await connection.QueryAsync<BusinessDetailsDto>(
                "Get_BusinessDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<List<CustomerModel>> GetBusinessCustomers(int BusinessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@BusinessId", BusinessId);

            var result = await connection.QueryAsync<CustomerModel>(
                "Get_BusinessCustomers",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }



        public async Task<List<Category>> GetBusinesscategory()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<Category>("Get_BusinessCategories", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
    }
}
