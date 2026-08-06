using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface ITalentShowRepository
    {
        Task<BaseResponse> SaveCampaignAsync(TalentShowCampaign campaign);
        Task<List<TalentShowCampaign>> GetCampaignsAsync(bool includeInactive, bool uploadOpenOnly);
        Task<TalentShowCampaign?> GetCampaignByIdAsync(int talentShowCampaignId);
        Task<BaseResponse> SaveCategoryAsync(TalentShowCategory category);
        Task<List<TalentShowCategory>> GetCategoriesAsync(bool includeInactive);
        Task<TalentShowCategory?> GetCategoryByIdAsync(int talentShowCategoryId);
        Task<BaseResponse> RegisterForCampaignAsync(TalentShowRegistrationRequest request);
        Task<TalentShowRegistrationStatus?> GetRegistrationStatusAsync(int talentShowCampaignId, long? memberId, string? visitorKey);
        Task<BaseResponse> SaveVideoAsync(TalentShowVideo video);
        Task<List<TalentShowVideo>> GetVideosAsync(TalentShowVideoFilter filter);
        Task<BaseResponse> SaveVideoApprovalAsync(TalentShowVideoApprovalRequest request);
        Task<BaseResponse> SaveReactionAsync(TalentShowReactionRequest request);
        Task<TalentShowVideoSummary?> GetVideoSummaryAsync(long talentShowVideoId);
        Task<List<TalentShowRanking>> GetRankingsAsync(int? talentShowCampaignId, int? talentShowCategoryId, string? ageGroup);
    }
}
