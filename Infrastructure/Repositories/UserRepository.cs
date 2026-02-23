using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class UserRepository:IUserRepository
    {
        private readonly IConfiguration _configuration;

        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<int> AddAsync(Users entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Users>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Users> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }


        public async Task<BaseResponse> RegisterUser(RegisterRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@CommunityId", entity.CommunityId);
            parameters.Add("@FirstName", entity.FirstName);
            parameters.Add("@LastName", entity.LastName);
            parameters.Add("@Email", entity.Email);
            parameters.Add("@Mobile", entity.Mobile);
            parameters.Add("@Password", entity.Password);
            parameters.Add("@RoleIds", entity.RoleIds);

            var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>("Add_User", parameters, commandType: CommandType.StoredProcedure);

            return result;
        }


        public async Task<BaseResponse> SaveUser(Users entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@CommunityId", entity.CommunityId);

            parameters.Add("@FirstName", entity.FirstName);
            parameters.Add("@LastName", entity.LastName);

            parameters.Add("@Email", entity.Email);
            parameters.Add("@Mobile", entity.Mobile);

            parameters.Add("@Password", entity.Password);
            parameters.Add("@RoleIds", entity.RoleIds);

            parameters.Add("@ProfileImagePath", entity.ProfileImagePath);

            parameters.Add("@AddressLine1", entity.AddressLine1);
            parameters.Add("@AddressLine2", entity.AddressLine2);
            parameters.Add("@ZipCode", entity.ZipCode);
            parameters.Add("@City", entity.City);

            parameters.Add("@IsActive", entity.IsActive);

            var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>( "Add_User",   parameters, commandType: CommandType.StoredProcedure );

            return result;
        }

        public Task<int> UpdateAsync(Users entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> AddOrUpdateUserWallet(UserWallets entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", entity.UserId, DbType.Int32);
            parameters.Add("@Balance", entity.Balance, DbType.Decimal);
            parameters.Add("@RewardCoins", entity.RewardCoins, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>(
                "Add_UserWallet",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<Users> GetUserByUserId(Guid userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.Guid);

            var result = await connection.QueryFirstOrDefaultAsync<Users>("Get_UserByUserId",  parameters,commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<BaseResponse> AddWalletTransaction(WalletTransactions entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@WalletId", entity.WalletId, DbType.Int32);
            parameters.Add("@TransactionType", entity.TransactionType, DbType.String);
            parameters.Add("@Amount", entity.Amount, DbType.Decimal);
            parameters.Add("@Coins", entity.Coins, DbType.Int32);
            parameters.Add("@ReferenceType", entity.ReferenceType, DbType.String);
            parameters.Add("@ReferenceId", entity.ReferenceId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>(
                "Add_WalletTransaction",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<LoginResponse> UserLogin(LoginRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", request.Email, DbType.String);
            parameters.Add("@Password", request.Password, DbType.String);
           
            var result = await connection.QueryFirstOrDefaultAsync<LoginResponse>(
                "Login_User_SHA512",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return result;
        }


        public async Task<List<Roles>> GetRoles()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<Roles>("Get_Roles", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<UserDropdownDto>> GetBusinessUsers()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<UserDropdownDto>("Get_BusinessPeople", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
    }
}
