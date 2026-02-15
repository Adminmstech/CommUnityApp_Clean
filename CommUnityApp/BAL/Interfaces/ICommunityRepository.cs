using CommUnityApp.Models;

namespace CommUnityApp.BAL.Interfaces
{
    public interface ICommunityRepository
    {

        Task<CommunityLoginResponse> LoginAsync(CommunityLoginRequest request);
        Task<List<GroupDto>> GetGroupsByCommunityAsync(long communityId);
    }
}
