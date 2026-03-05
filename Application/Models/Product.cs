using System;
using System.Collections.Generic;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int? BusinessId { get; set; }

        public int? CategoryId { get; set; }
        public string ProductCategory { get; set; }

        public string? ProductName { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public decimal? DiscountPrice { get; set; }

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
}