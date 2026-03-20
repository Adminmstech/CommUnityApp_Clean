using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    internal class Services
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
}
