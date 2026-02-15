using CommUnityApp.Models;

namespace CommUnityApp.BAL.Interfaces
{
    public interface IEventRepository
    {

        Task<BaseResponse> AddUpdateEventAsync(
                   AddUpdateEventRequest model,
                   string imagePath);
        Task<List<EventCategory>> GetCategoriesAsync();

        Task<BaseResponse> PostEventToGroupsAsync(PostEventToGroupsRequest model);

        Task<List<EventListDto>> GetEventsByCommunityAsync(long communityId);
      Task UpdateEventQRCodeAsync(
 long eventId,
 string qrPath);

        Task<BaseResponse> RegisterEventAsync(
    EventRegistrationRequest model);
        Task<EventDto> GetEventByIdAsync(int eventId);
    }
}
