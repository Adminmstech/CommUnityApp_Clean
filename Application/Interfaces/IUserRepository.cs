
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IUserRepository
    {
        Task<BaseResponse> SaveUser(Users entity);
        Task<BaseResponse> AddOrUpdateUserWallet(UserWallets entity);
        Task<Users> GetUserByUserId(Guid userId);
        Task<BaseResponse> AddWalletTransaction(WalletTransactions entity);
        Task<LoginResponse> UserLogin(LoginRequest request);
        Task<BaseResponse> RegisterUser(RegisterRequest entity);
        Task<List<Roles>> GetRoles();
        Task<List<UserDropdownDto>> GetBusinessUsers();
    }
}
