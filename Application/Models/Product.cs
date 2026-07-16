using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int? BusinessId { get; set; }
        public string? BusinessName { get; set; }
        public string? Logo { get; set; }
        public int? CategoryId { get; set; }
        public string? ProductCategory { get; set; }

        public string? ProductName { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public decimal? DiscountPrice { get; set; }
        public decimal? DiscountAmount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? RedemptionCoins { get; set; }

        public bool? ReferAFriend { get; set; }
        public int? IsActive { get; set; }
        public List<ProductImage> Images { get; set; }
    }


    public class ProductCategories
    {
        public int CategoryId { get; set; }

        public string ProductCategory { get; set; } = string.Empty;
    }


    public class ProductImage
    {
        public int ProductImageId { get; set; }

        public int ProductId { get; set; }

        public string? ImagePath { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime? CreatedAt { get; set; }
    }


    public class ProductImageUpload
    {
        public int ProductImageId { get; set; }

        public int ProductId { get; set; }

        public string? ImageBase64 { get; set; }

        public string? ImagePath { get; set; }

        public bool IsPrimary { get; set; }
    }


    public class ProductWithImagesModel
    {
        public Product Product { get; set; } = new();

        public List<ProductImageUpload> Images { get; set; } = new();
    }

    public class ProductDetails
    {
        public int ProductId { get; set; }
        public int? BusinessId { get; set; }

        public int? CategoryId { get; set; }
        public string? ProductCategory { get; set; }

        public string? ProductName { get; set; }
        public string? Description { get; set; }

        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? RedemptionCoins { get; set; }
        public bool? ReferAFriend { get; set; }

        public int? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Business fields (used internally only)

        [JsonIgnore]
        public string? BusinessName { get; set; }

        [JsonIgnore]
        public string? OwnerName { get; set; }

        [JsonIgnore]
        public string? Email { get; set; }

        [JsonIgnore]
        public string? Phone { get; set; }

        [JsonIgnore]
        public string? Address { get; set; }

        [JsonIgnore]
        public string? City { get; set; }

        [JsonIgnore]
        public string? State { get; set; }

        [JsonIgnore]
        public string? Country { get; set; }

        [JsonIgnore]
        public string? Logo { get; set; }

        [JsonIgnore]
        public decimal? Latitude { get; set; }

        [JsonIgnore]
        public decimal? Longitude { get; set; }
    }
    public class ProductFullResponse
    {
        public ProductDetails ProductDetails { get; set; }

        public List<ProductImage> ProductImages { get; set; } = new();

        public BusinessDetails BusinessDetails { get; set; }
    }

    public class BusinessDetails
    {
        public string? BusinessName { get; set; }
        public string? OwnerName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        public string? Logo { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class FavBusineess
    {
        public Guid? UserId { get; set; }
        public int? BusinessId { get; set; }
    }

    public class UserFavoriteBusiness
    {
        public int FavoriteId { get; set; }
        public Guid UserId { get; set; }
        public int BusinessId { get; set; }

        public string BusinessName { get; set; }
        public string OwnerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Logo { get; set; }
    }

    public class FavoriteBusinessResult
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; } = string.Empty;

        public bool IsFavorite { get; set; }
    }

    public class AddToCartRequest
    {
        public Guid UserId { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemResponse
    {
        public int CartId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal DiscountPrice { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }
    }

    public class CartWithImagesModel
    {
        public CartItemResponse Cart { get; set; }

        public List<ProductImageUpload> Images { get; set; } = new List<ProductImageUpload>();
    }

    public class AdminPromotionDto
    {
        public int ProductId { get; set; }

        public int BusinessId { get; set; }

        public string? BusinessName { get; set; }

        public int CategoryId { get; set; }

        public string? ProductCategory { get; set; }

        public string? ProductName { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public DateTime? StartDate { get; set; }

        public string StartDateString
        {
            get
            {
                return StartDate.HasValue
                    ? StartDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }
        public DateTime? EndDate { get; set; }
        public string EndDateString
        {
            get
            {
                return EndDate.HasValue
                    ? EndDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? RedemptionCoins { get; set; }

        public bool ReferAFriend { get; set; }

        public string? PromotionStatus { get; set; }
    }

    //public class ProductPromotionModel
    //{
    //    public int PromotionId { get; set; }

    //    public int ProductId { get; set; }

    //    public int BusinessId { get; set; }

    //    public string PromotionType { get; set; }

    //    public string OtherPromotionText { get; set; }

    //    public string PromoCode { get; set; }

    //    public decimal? DiscountValue { get; set; }

    //    public decimal? CashbackValue { get; set; }

    //    public decimal? MinimumPurchaseAmount { get; set; }

    //    public int? MaxRedemptionLimit { get; set; }

    //    public string BuyGetDetails { get; set; }

    //    public string ComboOfferDetails { get; set; }

    //    public bool IsLimitedDeal { get; set; }

    //    public string PromotionImage { get; set; }

    //    public DateTime? StartDate { get; set; }

    //    public DateTime? EndDate { get; set; }

    //    public bool IsActive { get; set; }
    //}
    public class ProductPromotionModel
    {
        public int PromotionId { get; set; }

        public int BusinessId { get; set; }

        public string FeaturedProducts { get; set; }

        public string PromotionTitle { get; set; }

        public string OfferHeadline { get; set; }

        public decimal PromotionalPrice { get; set; }
        public decimal ActualPrice { get; set; }

        public int MaxCoinsRedemptionPercentage { get; set; }

        public int IndoCoinsEarned { get; set; }

        public int FriendRewardCoins { get; set; }

        public string PromotionImage { get; set; }

        public DateTime? StartDate { get; set; }

        public string StartDateString
        {
            get
            {
                return StartDate.HasValue
                    ? StartDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }
        public DateTime? EndDate { get; set; }
        public string EndDateString
        {
            get
            {
                return EndDate.HasValue
                    ? EndDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }

        public bool IsActive { get; set; }
    }
    public class PromotionResult
    {
        public int Status { get; set; }

        public string Message { get; set; }

        public int PromotionId { get; set; }

        public string PromotionUrl { get; set; }

        public string PromotionToken { get; set; }
    }

    public class PromotionWithImageModel
    {
        public ProductPromotionModel Promotion { get; set; }

        public string PromotionImageBase64 { get; set; }
    }

    //public class PromotionListModel
    //{
    //    public int PromotionId { get; set; }

    //    public int ProductId { get; set; }

    //    public string ProductName { get; set; }

    //    public string ProductDescription { get; set; }

    //    public string PromotionType { get; set; }

    //    public string PromoCode { get; set; }

    //    public decimal DiscountValue { get; set; }

    //    public decimal CashbackValue { get; set; }

    //    public decimal MinimumPurchaseAmount { get; set; }

    //    public int MaxRedemptionLimit { get; set; }

    //    public int RedeemedCount { get; set; }

    //    public string BuyGetDetails { get; set; }

    //    public string ComboOfferDetails { get; set; }

    //    public bool IsLimitedDeal { get; set; }

    //    public string PromotionImage { get; set; }

    //    public string QRCodeImage { get; set; }

    //    public string PromotionUrl { get; set; }

    //    public string PromotionToken { get; set; }

    //    public DateTime StartDate { get; set; }

    //    public DateTime EndDate { get; set; }

    //    public bool IsActive { get; set; }

    //    public DateTime CreatedAt { get; set; }
    //}

    public class PromotionListModel
    {
        public int PromotionId { get; set; }

        public int BusinessId { get; set; }

        public string FeaturedProducts { get; set; }

        public string PromotionTitle { get; set; }

        public string OfferHeadline { get; set; }

        public decimal PromotionalPrice { get; set; }
        public decimal ActualPrice { get; set; }

        public int MaxCoinsRedemptionPercentage { get; set; }

        public int IndoCoinsEarned { get; set; }

        public int FriendRewardCoins { get; set; }

        public string PromotionImage { get; set; }

        public string QRCodeImage { get; set; }

        public string PromotionUrl { get; set; }

        public string PromotionToken { get; set; }

        public DateTime? StartDate { get; set; }

        public string StartDateString
        {
            get
            {
                return StartDate.HasValue
                    ? StartDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }
        public DateTime? EndDate { get; set; }
        public string EndDateString
        {
            get
            {
                return EndDate.HasValue
                    ? EndDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class PromotionDetailsModel
    {
        public int PromotionId { get; set; }

        public int BusinessId { get; set; }

        public string BusinessName { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public string FeaturedProducts { get; set; }

        public string PromotionTitle { get; set; }

        public string OfferHeadline { get; set; }

        public decimal PromotionalPrice { get; set; }
        public decimal ActualPrice { get; set; }


        public int MaxCoinsRedemptionPercentage { get; set; }

        public int IndoCoinsEarned { get; set; }

        public int FriendRewardCoins { get; set; }

        public string PromotionImage { get; set; }

        public string QRCodeImage { get; set; }

        public string PromotionUrl { get; set; }

        public string PromotionToken { get; set; }

        public DateTime? StartDate { get; set; }

        public string StartDateString
        {
            get
            {
                return StartDate.HasValue
                    ? StartDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }
        public DateTime? EndDate { get; set; }
        public string EndDateString
        {
            get
            {
                return EndDate.HasValue
                    ? EndDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class PromotionScanModel
    {
        public int ScanId { get; set; }

        public int PromotionId { get; set; }

        public Guid? UserId { get; set; }

        public string DeviceInfo { get; set; }

        public DateTime ScannedAt { get; set; }
    }
    public class GetPromotionByTokenRequest
    {
        public Guid PromotionToken { get; set; }

        public Guid? UserId { get; set; }
    }
    public class PromotionResponse
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; }

        public PromotionDetailsModel Data { get; set; }
    }


    public class BusinessPromotionModel
    {
        public int PromotionId { get; set; }

        public int BusinessId { get; set; }

        public string FeaturedProducts { get; set; }

        public string PromotionTitle { get; set; }

        public string OfferHeadline { get; set; }

        public decimal PromotionalPrice { get; set; }
        public decimal ActualPrice { get; set; }

        public int MaxCoinsRedemptionPercentage { get; set; }

        public int IndoCoinsEarned { get; set; }

        public int FriendRewardCoins { get; set; }

        public string PromotionImage { get; set; }

        public string QRCodeImage { get; set; }

        public string PromotionUrl { get; set; }

        public string PromotionToken { get; set; }

        public DateTime? StartDate { get; set; }

        public string StartDateString
        {
            get
            {
                return StartDate.HasValue
                    ? StartDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }
        public DateTime? EndDate { get; set; }
        public string EndDateString
        {
            get
            {
                return EndDate.HasValue
                    ? EndDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class PromotionRedemptionRequest
    {
        public long PromotionId { get; set; }

        public Guid UserId { get; set; }
    }

    public class PromotionRedemptionSummary
    {
        public decimal OriginalPrice { get; set; }

        public int CoinsUsed { get; set; }

        public decimal CoinDiscount { get; set; }

        public decimal FinalPrice { get; set; }
    }


    public class PromotionRedemptionResult
    {
        public int ResultId { get; set; }
        public string ResultMessage { get; set; }
        public bool Status { get; set; }

        public long? RedemptionId { get; set; }
        public string RedemptionCode { get; set; }
        public string QRCodeImage { get; set; }
    }

    public class PromotionRedemptionModel
    {
        public long RedemptionId { get; set; }

        public string RedemptionCode { get; set; }

        public decimal FinalPrice { get; set; }

        public int CoinsUsed { get; set; }

        public decimal CoinDiscount { get; set; }

        public string RedemptionStatus { get; set; }

        public string PromotionTitle { get; set; }

        public string OfferHeadline { get; set; }

        public string PromotionImage { get; set; }

        public string QRCodeImage { get; set; }
    }

    public class VerifyPromotionRequest
    {
        public string RedemptionCode { get; set; }
    }

    public class VerifyPromotionResponse
    {
        public int Status { get; set; }

        public string Message { get; set; }
    }

    public class TopProductPromotionEntity
    {
        public long PromotionId { get; set; }

        public long? ProductId { get; set; }

        public long BusinessId { get; set; }

        public string BusinessName { get; set; }

        public string BusinessLogo { get; set; }

        public string FeaturedProducts { get; set; }

        public string PromotionTitle { get; set; }

        public string OfferHeadline { get; set; }

        public decimal PromotionalPrice { get; set; }
        public decimal ActualPrice { get; set; }

        public int MaxCoinsRedemptionPercentage { get; set; }

        public int IndoCoinsEarned { get; set; }

        public int FriendRewardCoins { get; set; }

        public string PromotionImage { get; set; }

        public string QRCodeImage { get; set; }

        public string PromotionUrl { get; set; }

        public Guid? PromotionToken { get; set; }

        public DateTime? StartDate { get; set; }

        public string StartDateString
        {
            get
            {
                return StartDate.HasValue
                    ? StartDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }
        public DateTime? EndDate { get; set; }
        public string EndDateString
        {
            get
            {
                return EndDate.HasValue
                    ? EndDate.Value.ToString("dd MMM yyyy HH:mm")
                    : "";
            }
        }

        public bool IsLimitedDeal { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}