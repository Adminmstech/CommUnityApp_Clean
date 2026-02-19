namespace CommUnityApp.Domain.Entities
{
    public class Event
    {
    }

    public class EventCategory
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
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
