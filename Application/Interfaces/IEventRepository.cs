using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IEventRepository
    {
        Task<BaseResponse> AddUpdateEventAsync(AddUpdateEventRequest model, string imagePath);
        Task<List<EventCategory>> GetCategoriesAsync();
        Task<BaseResponse> PostEventToGroupsAsync(PostEventToGroupsRequest model);
        Task<List<EventListDto>> GetEventsByCommunityAsync(long communityId);
        Task UpdateEventQRCodeAsync(long eventId, string qrPath);
        Task<BaseResponse> RegisterEventAsync(EventRegistrationRequest model);
        Task<EventDto> GetEventByIdAsync(int eventId);
        Task<EventRegistrationModel?> GetRegistrationByIdAsync(long id);
        Task<IEnumerable<EventRegistrationModel>> GetRegistrationsByEventAsync(long eventId);
        Task<BookingResponse> BookEventAsync(EventBookingRequest request);
        Task<EventDetailsResponse> GetEventDetailsAsync(Guid userId, int eventId);
        Task<IEnumerable<UserBookingResponse>> GetUserBookingsAsync(Guid userId);
        Task<EventCheckoutResponse> GetEventCheckoutAsync(Guid transactionId);
        Task<dynamic> AddEventPaymentAsync(EventPaymentRequest request);
        Task<EventCheckoutSummaryResponse> GetEventCheckoutSummaryAsync(Guid userId, int eventId, int ticketTypeId, int quantity, bool useWallet);
        Task<int> AddEventSponsor(EventSponsorModel model, string logoPath);
        Task<List<EventSponsorModel>> GetEventSponsors(int eventId);
        Task<List<EventSponsorListModel>> GetAllSponsors();
        Task<List<EventSponsorModel>> GetSponsorsByCommunity(int communityId);
        Task AttachSponsorToEvent(EventSponsorMappingModel model);
        Task<List<EventModel>> GetEventsByCommunity(int communityId);
        Task<EventDetailsModel> GetEventDetailsWithSponsors(int eventId);

        Task<List<SponsorModel>> GetSponsorsByEvent(int eventId);

        Task<List<Events>> GetEvents();
        Task<List<Events>> GetEventById(int EventId);
        Task<List<TopEventDto>> GetTop5Events();
        Task<dynamic> PostEventToCommunityUsers(PostEventModel model);

        Task<List<UserModel>> GetUsersByCommunityId(int communityId);

        Task<List<UserPostedEventModel>> GetPostedEventsByUser(Guid userId);

        Task<dynamic> AddBookEvent(BookEventRequest model);

        Task<UserTicketDetailsResponse> GetUserTicketDetails(int ticketId);
        Task<SponsorDetailsResponse> GetSponsorDetailsById(int sponsorId);

        Task<dynamic> GetBookedTicketsByUserId(Guid userId);

        Task<dynamic> CheckInTicket(string ticketCode);
    }
}
