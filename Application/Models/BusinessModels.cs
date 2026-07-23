namespace CommUnityApp.ApplicationCore.Models
{
    public class BusinessLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class BusinessLoginResponse
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string Logo { get; set; }
        public string Email { get; set; }
        public string ResultMessage { get; set; }
    }

    public class AddUpdateBusinessRequest
    {
        public int BusinessId { get; set; }
        public int CategoryId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessNumber { get; set; }
        public string OwnerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Suburb { get; set; }
        public string Info { get; set; }
        public string Logo { get; set; }
        public string Password { get; set; }
    }

    public class AddBusinessRequest
    {
        public int BusinessId { get; set; }

        public int CategoryId { get; set; }

        public string BusinessName { get; set; }

        public string? BusinessNumber { get; set; }

        public string OwnerName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string? Country { get; set; }

        public string? Suburb { get; set; }

        public string? LogoBase64 { get; set; }

        public string? Logo { get; set; }

        public string? Info { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? WebLink { get; set; }      // Added

        public string? Password { get; set; }     // Added

        public bool IsVerified { get; set; }

        public int IsActive { get; set; } = 1;
    }

    public class BusinessAddResponse
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; }

        public string? GeneratedPassword { get; set; }
    }

    public class BusinessDetailsDto
    {
        public int BusinessId { get; set; }

        public string? BusinessName { get; set; }

        public string? BusinessNumber { get; set; }

        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public Guid? UserId { get; set; }          

        public string? OwnerName { get; set; }

        public string? BusinessEmail { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }

        public string? Suburb { get; set; }

        public string? Info { get; set; }

        public string? Logo { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? WebLink { get; set; }       

        public string? Password { get; set; }      

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public bool IsFavorite { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CustomerModel
    {
        public Guid UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }
        public int Gender { get; set; }
        public string City { get; set; }

        public int TotalOrders { get; set; }

        public decimal TotalSpent { get; set; }

        public DateTime LastOrderDate { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string Description { get; set; }

        public string? ImageURL { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class FavouriteBusinessRequest
    {
        public long BusinessId { get; set; }

        public Guid UserId { get; set; }
    }
    public class FavouriteBusinessModel
    {
        public long FavouriteId { get; set; }

        public long BusinessId { get; set; }

        public long CategoryId { get; set; }

        public string BusinessName { get; set; }

        public string BusinessNumber { get; set; }

        public string OwnerName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string Suburb { get; set; }

        public string Info { get; set; }

        public string Logo { get; set; }

        public string WebLink { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class BusinessPostEntity
    {
        public long PostId { get; set; }

        public long BusinessId { get; set; }

        public string BusinessName { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string ImagePath { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
    }

    public class BusinessPostDetailsEntity
    {
        public long PostId { get; set; }

        public long BusinessId { get; set; }

        public string BusinessName { get; set; }

        public string Logo { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string ImagePath { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
    }
    public class BusinessPostListEntity
    {
        public long PostId { get; set; }

        public long BusinessId { get; set; }

        public string BusinessName { get; set; }

        public string Logo { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string ImagePath { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
    }
    public class AppBusinessLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AppBusinessLoginResponse
    {
        public int ResultId { get; set; }
        public string ResultMessage { get; set; }
        public bool Status { get; set; }

        public long BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessNumber { get; set; }
        public string OwnerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Logo { get; set; }
    }

    public class BusinessPromotionRedemptionModel
    {
        public long RedemptionId { get; set; }
        public long PromotionId { get; set; }

        public string PromotionTitle { get; set; }
        public string OfferHeadline { get; set; }
        public string PromoCode { get; set; }

        public long ProductId { get; set; }
        public string ProductName { get; set; }

        public Guid UserId { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerMobile { get; set; }

        public string RedemptionCode { get; set; }
        public string QRCodeImage { get; set; }

        public decimal OriginalPrice { get; set; }
        public decimal CoinDiscount { get; set; }
        public int CoinsUsed { get; set; }
        public decimal FinalPrice { get; set; }

        public int IndoCoinsEarned { get; set; }
        public int FriendRewardCoins { get; set; }

        public string RedemptionStatus { get; set; }

        public DateTime? RedeemedAt { get; set; }
        public DateTime? ShopVerifiedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AppBusinessPromotionModel
    {
        public long PromotionId { get; set; }

        public long ProductId { get; set; }

        public long BusinessId { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public string PromotionTitle { get; set; }

        public string OfferHeadline { get; set; }

        public string PromotionType { get; set; }

        public string OtherPromotionText { get; set; }

        public string PromoCode { get; set; }

        public decimal? ActualPrice { get; set; }

        public decimal? PromotionalPrice { get; set; }

        public decimal? DiscountValue { get; set; }

        public decimal? CashbackValue { get; set; }

        public decimal? MinimumPurchaseAmount { get; set; }

        public int? MaxRedemptionLimit { get; set; }

        public int RedeemedCount { get; set; }

        public decimal? MaxCoinsRedemptionPercentage { get; set; }

        public int IndoCoinsEarned { get; set; }

        public int FriendRewardCoins { get; set; }

        public string PromotionImage { get; set; }

        public string QRCodeImage { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public string PromotionStatus { get; set; }

        public int TotalRedemptions { get; set; }
    }

    public class ValidatePromotionRedemptionRequest
    {
        public long BusinessId { get; set; }

        public string RedemptionCode { get; set; }
    }

    public class ValidatePromotionRedemptionResult
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; }

        public bool Status { get; set; }

        public long RedemptionId { get; set; }

        public long PromotionId { get; set; }

        public long BusinessId { get; set; }

        public string PromotionTitle { get; set; }

        public string OfferHeadline { get; set; }

        public string PromotionType { get; set; }

        public string PromoCode { get; set; }

        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public Guid UserId { get; set; }

        public string CustomerName { get; set; }

        public string RedemptionCode { get; set; }

        public decimal OriginalPrice { get; set; }

        public decimal CoinDiscount { get; set; }

        public int CoinsUsed { get; set; }

        public decimal FinalPrice { get; set; }

        public int IndoCoinsEarned { get; set; }

        public int FriendRewardCoins { get; set; }

        public string RedemptionStatus { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class ConfirmPromotionRedemptionRequest
    {
        public long BusinessId { get; set; }

        public long RedemptionId { get; set; }
    }

    public class ConfirmPromotionRedemptionResult
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; }

        public bool Status { get; set; }

        public long RedemptionId { get; set; }
    }

    


    /// Wallet

    public class BusinessWalletModel
    {
        public int BusinessWalletId { get; set; }

        public int BusinessId { get; set; }

        public long AvailableCoins { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class BusinessWalletTransactionModel
    {
        public long Id { get; set; }

        public int BusinessWalletId { get; set; }

        public string TransactionType { get; set; }

        public int Coins { get; set; }

        public string ReferenceType { get; set; }

        public int ReferenceId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Notes { get; set; }
    }

    public class TransactionTypeModel
    {
        public int TransactionTypeId { get; set; }

        public string TypeCode { get; set; }

        public string TypeName { get; set; }

        public string Description { get; set; }

        public bool IsCredit { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class AllocateBusinessCoinsRequest
    {
        public int BusinessId { get; set; }

        public long Coins { get; set; }

        public string Notes { get; set; }
    }

    public class RewardMemberRequest
    {
        public int BusinessId { get; set; }

        public Guid UserId { get; set; }

        public int Coins { get; set; }

        public string ReferenceType { get; set; }

        public int ReferenceId { get; set; }

        public string Notes { get; set; }
    }

    public class AdjustBusinessWalletRequest
    {
        public int BusinessId { get; set; }

        public long Coins { get; set; }

        public string TransactionType { get; set; }

        public string Notes { get; set; }
    }

    public class BusinessWalletDashboardModel
    {
        public int BusinessId { get; set; }

        public string BusinessName { get; set; }

        public long AvailableCoins { get; set; }

        public long TotalAllocatedCoins { get; set; }

        public long TotalRewardedCoins { get; set; }

        public int TotalTransactions { get; set; }
    }

    public class RewardsDashboardModel
    {
        public long AvailableCoins { get; set; }
        public long TotalAllocatedCoins { get; set; }
        public long TotalRewardCoinsSpent { get; set; }
        public int TotalPromotionShares { get; set; }
        public int TotalProductShares { get; set; }
        public int TotalUsersRewarded { get; set; }
    }

    public class ShareRewardHistoryModel
    {
        public string UserName { get; set; }

        public Guid UserId { get; set; }

        public int? PromotionId { get; set; }

        public string PromotionName { get; set; }

        public int? ProductId { get; set; }

        public string ProductName { get; set; }

        public string SharePlatform { get; set; }

        public int RewardCoins { get; set; }

        public DateTime SharedAt { get; set; }
    }
}
