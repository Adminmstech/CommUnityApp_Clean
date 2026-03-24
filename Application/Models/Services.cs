using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Services
    {
      

    }
    public class ServiceAppointment
    {
        public int AppointmentId { get; set; }
        public int ServiceId { get; set; }
        public int BusinessId { get; set; }

        public Guid UserId { get; set; } // ✅ GUID

        public DateTime AppointmentDateTime { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class ServiceListResponse
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsBookingRequired { get; set; }

        public int BusinessId { get; set; }
        public string BusinessName { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ServiceSearchRequest
    {
        public string SearchText { get; set; }
        public int? CategoryId { get; set; }
        public int? BusinessId { get; set; }
    }
    public class AppointmentResponse
    {
        public int AppointmentId { get; set; }

        public Guid UserId { get; set; }  // UNIQUEIDENTIFIER → Guid

        public string ServiceName { get; set; }
        public string BusinessName { get; set; }
        public string CategoryName { get; set; }

        public DateTime AppointmentDateTime { get; set; }

        public string Status { get; set; }
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ServiceDetails
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
       
        public bool IsBookingRequired { get; set; }

        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Logo { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string WebLink { get; set; }
        public string CategoryName { get; set; }
    }
    public class RelatedService
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
       
    }
    public class ServiceDetailsResponse
    {
        public ServiceDetails Service { get; set; }
        public List<RelatedService> OtherServices { get; set; }
    }

    public class BusinessAppointmentDto
    {
        public int AppointmentId { get; set; }

        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }

        public int BusinessId { get; set; }

        public DateTime AppointmentDateTime { get; set; }

        public string? Notes { get; set; }

        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid UserId { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }
    }

    public class UpdateAppointmentStatusRequest
    {
        public int AppointmentId { get; set; }

        public string Status { get; set; }  // Pending / Completed / Cancelled
    }

    public class UpdateAppointmentStatusResponse
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }
    }

    public class AddServiceRequest
    {
        public int? BusinessId { get; set; }

        public string? ServiceName { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public int? DurationMinutes { get; set; }

        public bool? IsBookingRequired { get; set; }

        public bool? IsActive { get; set; }
    }

    public class AddOrUpdateServiceImageRequest
    {
        public int? ImageId { get; set; }   

        public int ServiceId { get; set; }

        public string ImageUrl { get; set; }

        public bool? IsPrimary { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ServiceWithImagesModel
    {
        public AddServiceRequest Service { get; set; }
        public List<ServiceImageUploadModel> Images { get; set; }
    }

    public class ServiceImageUploadModel
    {
        public string ImageBase64 { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class ServiceModel
    {
        public int ServiceId { get; set; }
        public int BusinessId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int? DurationMinutes { get; set; }
        public bool? IsBookingRequired { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class ServiceImageModel
    {
        public int ImageId { get; set; }

        public int ServiceId { get; set; }

        public string ImageUrl { get; set; }

        public bool IsPrimary { get; set; }
    }
    public class ServiceWithImagesResponse
    {
        public ServiceModel Service { get; set; }

        public List<ServiceImageModel> Images { get; set; } = new();
    }

}
