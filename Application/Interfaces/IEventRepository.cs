using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;

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
    }
}
