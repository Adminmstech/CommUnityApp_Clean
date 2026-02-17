namespace CommUnityApp.Models
{
    public class Event
    {
    }
    public class AddUpdateEventRequest
    {
        public long EventId { get; set; }          
        public long CommunityId { get; set; }
        public long CategoryId { get; set; }

        public string EventName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int IsFundRaising { get; set; }
        public IFormFile EventImage { get; set; }
    }
    public class BaseResponse
    {
        public int ResultId { get; set; }
        public string ResultMessage { get; set; }
    }
    public class EventCategory
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
    public class PostEventToGroupsRequest
    {
        public long CommunityId { get; set; }
        public long EventId { get; set; }
        public string GroupIds { get; set; } // "1,2,3"
        public long PostedBy { get; set; }
    }
    public class EventListDto
    {
        public long EventId { get; set; }
        public string EventName { get; set; }
        public string CategoryName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public string EventImage { get; set; }
        public string QRCodeImage { get; set; }

        public int IsFundRaising { get; set; }
    }
    public class EventRegistrationRequest
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }

        public int NoOfAdults { get; set; }
        public int NoOfChildren { get; set; }
        public string? SpecialRequest { get; set; }

    }
    public class EventDto
    {
        public int EventId { get; set; }
        public long CommunityId { get; set; }
        public long CategoryId { get; set; }

        public string EventName { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string EventImage { get; set; }
        public string QRCodeImage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public int IsFundRaising { get; set; }

        // Ticketing
        public bool IsPaidEvent { get; set; }
        public decimal? TicketPrice { get; set; }
        public int? TotalSeats { get; set; }
        public int? TicketsBooked { get; set; }
        public int? RemainingSeats { get; set; }
        public int? MaxTicketsPerUser { get; set; }

        // Registration control
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public bool IsRegistrationRequired { get; set; }
        public bool IsTicketingEnabled { get; set; }

        // Extra
        public string Sponsors { get; set; }
        public string EventAcess { get; set; }
        public string EventStatus { get; set; }
    }

    public class EventRegistrationModel
    {
        public long RegistrationId { get; set; }
        public long EventId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public int NoOfAdults { get; set; }
        public int NoOfChildren { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string BookingStatus { get; set; }
        public string SpecialRequest { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime? CheckedInTime { get; set; }
    }
    


}
