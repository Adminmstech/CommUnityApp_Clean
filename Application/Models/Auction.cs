using Microsoft.AspNetCore.Http;
using System;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Auction
    {
        public int AuctionId { get; set; }

        public int? BusinessId { get; set; }

        public Guid UserId { get; set; }

        public int? ItemTypeId { get; set; }

        public string? ItemTitle { get; set; }

        public string? ItemDescription { get; set; }

        public string? ItemCondition { get; set; }

        public decimal? PriceIncrement { get; set; }

        public decimal? ReservePrice { get; set; }

        public decimal? MinDeposite { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? ItemLocation { get; set; }

        public int? DeleveryMethodId { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }

        public int? AuctionStatus { get; set; }

        public string? CreatedBy { get; set; }

        
    }

    public class AuctionItemImage
    {
        public int ImageId { get; set; }

        public int? AuctionId { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImageName { get; set; }

        public bool? IsPrimary { get; set; }

        public DateTime? CreatedDate { get; set; }
    }

    public class AuctionWithImagesRequest
    {
        public Auction Auction { get; set; }

        public List<AuctionImageUploadModel> Images { get; set; }
    }

    public class AuctionImageUploadModel
    {
        public string ImageBase64 { get; set; }
        public bool IsPrimary { get; set; }
    }
    public class GetAuctionImagedModel
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
        public int AuctionId { get; set; }
        public string ImageName { get; set; }
        public bool IsPrimary { get; set; }
    }
    public class ItemType
    {
        public int ItemTypeId { get; set; }

        public string? ItemTypeName { get; set; }

        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }
    }

    public class AuctionListModel
    {
        public int AuctionId { get; set; }

        public int? BusinessId { get; set; }

        public Guid UserId { get; set; }

        public string? User { get; set; }   

        public int? ItemTypeId { get; set; }

        public string? ItemTitle { get; set; }

        public string? ItemDescription { get; set; }

        public string? ItemCondition { get; set; }

        public decimal? PriceIncrement { get; set; }

        public decimal? ReservePrice { get; set; }

        public decimal? MinDeposite { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? ItemLocation { get; set; }

        public int? DeleveryMethodId { get; set; }

        public int? AuctionStatus { get; set; }

        public string? CreatedBy { get; set; }

        public List<GetAuctionImagedModel> AuctionImages { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AuctionWithImagesModel
    {
        public int AuctionId { get; set; }
        public int? BusinessId { get; set; }
        public Guid UserId { get; set; }
        public string? User { get; set; }
        public int? ItemTypeId { get; set; }
        public string? ItemTitle { get; set; }
        public string? ItemDescription { get; set; }
        public string? ItemCondition { get; set; }
        public decimal? PriceIncrement { get; set; }
        public decimal? ReservePrice { get; set; }
        public decimal? MinDeposite { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? ItemLocation { get; set; }
        public int? DeleveryMethodId { get; set; }
        public int? AuctionStatus { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

        public List<AuctionItemImage> Images { get; set; } = new();
    }

    public class PlaceBidRequest
    {
        public int AuctionId { get; set; }

        public Guid UserId { get; set; }

        public decimal BidAmount { get; set; }
    }
    public class BidDto
    {
        public int AuctionId { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public decimal BidAmount { get; set; }

        public DateTime BidTime { get; set; }

        public string BidTimeFormatted =>
            BidTime.ToString("HH:mm:ss");
    }
    public class  PlaceBidResponse
    {
        public int ResultId { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
        public string UserName { get; set; }= string.Empty;

    }

    public class BidRegistration
    {
        public Guid? BidRegistrationId { get; set; }

        public int AuctionId { get; set; }

        public Guid UserId { get; set; }

        public string? PaymentId { get; set; }

        public int PaymentStatusId { get; set; }
    }
    public class CheckoutRequestModel
    {
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public int AuctionId { get; set; }
    }
    public class BidRegistrationModel
    {
        public int AuctionId { get; set; }
        public string UserId { get; set; }
        public string PaymentId { get; set; }
        public string StripeSessionId { get; set; }
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }



}
