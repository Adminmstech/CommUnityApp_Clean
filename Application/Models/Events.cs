namespace CommUnityApp.ApplicationCore.Models
{
    public class Events
    {
        public long EventId { get; set; }

        public long? CommunityId { get; set; }
        public string? CommunityName { get; set; }

        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public string? EventName { get; set; }
        public string? EventImage { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? IsFundRaising { get; set; }
        public int? IsActive { get; set; }

        public int? IsPaidEvent { get; set; }
        public decimal? TicketPrice { get; set; }

        public int? AvailableTickets { get; set; }
        public int? TotalSeats { get; set; }
        public int? TicketsBooked { get; set; }

        public string? Sponsors { get; set; }
        public int? EventAcess { get; set; }

        public int? MaxTicketsPerUser { get; set; }

        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }

        public bool? IsRegistrationRequired { get; set; }
        public bool? IsTicketingEnabled { get; set; }

        public DateTime? CreatedDate { get; set; }
    }

    public class TopEventDto
    {
        public long EventId { get; set; }

        public long? CommunityId { get; set; }
        public string? CommunityName { get; set; }

        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public string? EventName { get; set; }
        public string? EventImage { get; set; }
        public string? Location { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? IsPaidEvent { get; set; }
        public decimal? TicketPrice { get; set; }

        public int? AvailableTickets { get; set; }
    }
}
