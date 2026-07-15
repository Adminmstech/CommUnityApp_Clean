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
                    WebLink = request.WebLink,     
                    Password = request.Password,   
                    IsVerified = request.IsVerified,
                    IsActive = request.IsActive
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<BusinessDetailsDto>> GetAllBusinesses(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<BusinessDetailsDto>("Get_AllBusinesses", new
            {
                UserId = userId
            }, commandType: CommandType.StoredProcedure);

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

        public async Task<BaseResponse>AddRemoveFavouriteBusiness(long businessId, Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await connection.QueryFirstOrDefaultAsync<
                BaseResponse>(
                "SP_AddRemoveFavouriteBusiness",
                new
                {
                    BusinessId = businessId,
                    UserId = userId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<FavouriteBusinessModel>>GetFavouriteBusinesses(Guid userId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var result =
                (await connection.QueryAsync<FavouriteBusinessModel>(
                    "SP_GetFavouriteBusinesses",
                    new { UserId = userId },
                    commandType: CommandType.StoredProcedure))
                .ToList();

            string baseUrl = _configuration["BaseUrl"];

            foreach (var item in result)
            {
                if (!string.IsNullOrWhiteSpace(item.Logo))
                {
                    item.Logo =
                        $"{baseUrl}/{item.Logo.TrimStart('/')}";
                }
            }

            return result;
        }

        public async Task<List<BusinessPostEntity>> GetTopFiveBusinessPosts()
        {
            using var connection =
                new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var result =
                await connection.QueryAsync<BusinessPostEntity>(
                    "sp_GetTopFiveBusinessPosts",
                    commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<BusinessPostDetailsEntity> GetBusinessPostDetails(long postId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await connection.QueryFirstOrDefaultAsync<BusinessPostDetailsEntity>(
                "sp_GetBusinessPostDetails",
                new
                {
                    PostId = postId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<BusinessPostListEntity>> GetAllBusinessPosts(long businessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await connection.QueryAsync<BusinessPostListEntity>(
                "sp_GetAllBusinessPosts",
                new
                {
                    BusinessId = businessId
                },
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<AppBusinessLoginResponse> BusinessLogin( AppBusinessLoginRequest request)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstOrDefaultAsync<AppBusinessLoginResponse>(
                "sp_BusinessLogin",
                new
                {
                    request.Email,
                    request.Password
                },
                commandType: CommandType.StoredProcedure);

            return result;
        }


        public async Task<List<BusinessPromotionRedemptionModel>>GetBusinessPromotionRedemptions(long businessId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryAsync<BusinessPromotionRedemptionModel>(
                "sp_GetBusinessPromotionRedemptions",
                new
                {
                    BusinessId = businessId
                },
                commandType: CommandType.StoredProcedure); 

            return result.ToList(); 
        }

        public async Task<List<BusinessPromotionModel>>GetBusinessPromotions(long businessId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryAsync<BusinessPromotionModel>(
                "sp_GetBusinessPromotions",
                new
                {
                    BusinessId = businessId
                },
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<ValidatePromotionRedemptionResult>
     ValidatePromotionRedemptionCode(
         long businessId,
         string redemptionCode)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryFirstOrDefaultAsync<
                ValidatePromotionRedemptionResult>(
                    "sp_ValidatePromotionRedemptionCode",
                    new
                    {
                        BusinessId = businessId,
                        RedemptionCode = redemptionCode
                    },
                    commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<ConfirmPromotionRedemptionResult>ConfirmPromotionRedemption(ConfirmPromotionRedemptionRequest request)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con
                .QueryFirstOrDefaultAsync<ConfirmPromotionRedemptionResult>(
                    "sp_ConfirmPromotionRedemption",
                    new
                    {
                        BusinessId = request.BusinessId,
                        RedemptionId = request.RedemptionId
                    },
                    commandType: CommandType.StoredProcedure);

            return result;
        }



    }
}
