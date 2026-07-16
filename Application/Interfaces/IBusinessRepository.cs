using CommUnityApp.ApplicationCore.Models;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IBusinessRepository
    {
        Task<BusinessLoginResponse> LoginAsync(BusinessLoginRequest request);
        Task<BaseResponse> RegisterAsync(AddUpdateBusinessRequest request);
        Task<BusinessAddResponse> AddBusinessAsync(AddBusinessRequest request);
        Task<List<BusinessDetailsDto>> GetAllBusinesses(Guid userId);
        Task<BusinessDetailsDto> GetBusinessDetails(int BusinessId);
        Task<List<CustomerModel>> GetBusinessCustomers(int BusinessId);
        Task<List<Category>> GetBusinesscategory();
        Task<BaseResponse> AddRemoveFavouriteBusiness(long businessId, Guid userId);
        Task<List<FavouriteBusinessModel>>GetFavouriteBusinesses(Guid userId);
        Task<List<BusinessPostEntity>> GetTopFiveBusinessPosts();
        Task<BusinessPostDetailsEntity> GetBusinessPostDetails(long postId);
        Task<List<BusinessPostListEntity>> GetAllBusinessPosts(long businessId);

        Task<AppBusinessLoginResponse> BusinessLogin(AppBusinessLoginRequest request);
        Task<List<BusinessPromotionRedemptionModel>> GetBusinessPromotionRedemptions(long businessId);
        Task<List<BusinessPromotionModel>> GetBusinessPromotions(long businessId);
        Task<ValidatePromotionRedemptionResult> ValidatePromotionRedemptionCode(long businessId,string redemptionCode); 
        Task<ConfirmPromotionRedemptionResult>ConfirmPromotionRedemption( ConfirmPromotionRedemptionRequest request);
        //Wallet 
        Task<BaseResponse> AllocateBusinessCoins(AllocateBusinessCoinsRequest request);
        Task<BaseResponse> RewardMemberFromBusiness(RewardMemberRequest request);
        Task<BaseResponse> AdjustBusinessWallet(AdjustBusinessWalletRequest request);
        Task<BusinessWalletModel> GetBusinessWallet(int businessId);
        Task<List<BusinessWalletTransactionModel>> GetBusinessWalletTransactions(int businessId);
        Task<List<TransactionTypeModel>> GetTransactionTypes();
        Task<RewardsDashboardModel> GetRewardsDashboard(int businessId);
        Task<List<ShareRewardHistoryModel>> GetShareRewardHistory( int businessId);

    }

}
