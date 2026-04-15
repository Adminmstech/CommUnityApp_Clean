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
    { public Guid? UserId { get; set; }
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
}