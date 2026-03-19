using CommUnityApp.ApplicationCore.Models;
using static CommUnityApp.ApplicationCore.Models.AssignVolunteerRequest;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface ICommunityRepository
    {
        Task<CommunityLoginResponse> LoginAsync(CommunityLoginRequest request);
        Task<List<GroupDto>> GetGroupsByCommunityAsync(long communityId);

        Task<List<CharityItem>> GetCharityItemsByCommunityId(long communityId);
        Task<IEnumerable<dynamic>> GetVolunteersList(long? communityId);
        Task<bool> AssignVolunteer(long charityItemId, Guid assignedToUserId);

        Task<List<Community>> GetCommunities();

    }
}
