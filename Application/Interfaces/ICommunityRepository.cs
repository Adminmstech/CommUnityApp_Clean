using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface ICommunityRepository
    {
        Task<CommunityLoginResponse> LoginAsync(CommunityLoginRequest request);
        Task<List<GroupDto>> GetGroupsByCommunityAsync(long communityId);
        Task<List<CharityItem>> GetCharityItemsByCommunityId(long communityId);
        Task<IEnumerable<dynamic>> GetVolunteersList(long? communityId);
        Task<bool> AssignVolunteer(long charityItemId, Guid assignedToUserId);
        Task<AssignedVolunteerModel> GetAssignedVolunteer(int charityItemId);
        Task<CharityItemModel> GetCharityItemDetails(int charityItemId);
        Task<bool> UpdateVolunteerStatusAsync(UpdateStatusRequest request);
        Task<List<CharityRequestModel>> GetCharityItemRequestsList(long communityId);

        Task<(int CharityItemId, string ItemCode)> AddCharityItem(AddCharityItemModel model, string imagePath);
        Task UpdateCharityItemImage(int charityItemId, string imagePath);
        Task<int> RequestCharityItem(RequestCharityItemModel model);

        Task<List<RequestedUserModel>> GetRequestedUsersByItemId(int charityItemId);

        Task AssignVolunteerToRequest(AssignVolunteerModel model);

        Task<List<CharityItemListModel>> GetAllCharityItems();
        Task<List<MyRequestedItemsModel>> GetMyRequestedItems(Guid userId);

        Task<List<ItemCategoryModel>> GetItemCategories();
        }
}
