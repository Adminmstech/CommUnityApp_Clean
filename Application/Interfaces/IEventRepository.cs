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
        Task<EventCheckoutSummaryResponse> GetEventCheckoutSummaryAsync(Guid userId);

        Task<List<Events>> GetEvents();
        Task<List<Events>> GetEventById(int EventId);
        Task<List<TopEventDto>> GetTop5Events();
    }
}
