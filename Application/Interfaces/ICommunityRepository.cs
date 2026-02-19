using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface ICommunityRepository
    {
        Task<CommunityLoginResponse> LoginAsync(CommunityLoginRequest request);
        Task<List<GroupDto>> GetGroupsByCommunityAsync(long communityId);
    }
}
