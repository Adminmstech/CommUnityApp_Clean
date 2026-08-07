using Microsoft.AspNetCore.Http;

namespace CommUnityApp.ApplicationCore.Models
{
    public class TalentShowCategory
    {
        public int TalentShowCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class TalentShowCampaign
    {
        public int TalentShowCampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string? EventName { get; set; }
        public string? Description { get; set; }
        public string? Guidelines { get; set; }
        public string? TermsAndConditions { get; set; }
        public int DurationDays { get; set; } = 30;
        public int ChallengeCount { get; set; } = 3;
        public int DaysPerChallenge { get; set; } = 7;
        public DateTime UploadStartDate { get; set; }
        public DateTime UploadEndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public bool IsUploadOpen { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class TalentShowVideo
    {
        public long TalentShowVideoId { get; set; }
        public int TalentShowCampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string? EventName { get; set; }
        public DateTime? UploadStartDate { get; set; }
        public DateTime? UploadEndDate { get; set; }
        public int TalentShowCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public long? MemberId { get; set; }
        public string? MemberName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string VideoPath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
        public bool IsActive { get; set; } = true;
        public int LikeCount { get; set; }
        public int RatingCount { get; set; }
        public decimal AverageRating { get; set; }
        public decimal JudgeScore { get; set; }
        public string? JudgeFeedback { get; set; }
        public string? JudgeSuggestions { get; set; }
        public int ChallengeNumber { get; set; } = 1;
        public string ChallengeLevel { get; set; } = "Beginner";
        public string? AgeGroup { get; set; }
        public decimal Score { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class TalentShowVideoFilter
    {
        public int? TalentShowCampaignId { get; set; }
        public int? TalentShowCategoryId { get; set; }
        public int? ChallengeNumber { get; set; }
        public string? AgeGroup { get; set; }
        public string? SearchText { get; set; }
        public bool ApprovedOnly { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 24;
    }

    public class TalentShowVideoUploadRequest
    {
        public int TalentShowCampaignId { get; set; }
        public int TalentShowCategoryId { get; set; }
        public int ChallengeNumber { get; set; } = 1;
        public string? AgeGroup { get; set; }
        public long? MemberId { get; set; }
        public string? MemberName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? VideoFile { get; set; }
        public IFormFile? ThumbnailFile { get; set; }
    }

    public class TalentShowVideoBase64UploadRequest
    {
        public int TalentShowCampaignId { get; set; }
        public int TalentShowCategoryId { get; set; }
        public int ChallengeNumber { get; set; } = 1;
        public string? AgeGroup { get; set; }
        public long? MemberId { get; set; }
        public string? MemberName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string VideoBase64 { get; set; } = string.Empty;
        public string? VideoFileName { get; set; }
        public string? VideoExtension { get; set; }
        public string? ThumbnailBase64 { get; set; }
        public string? ThumbnailFileName { get; set; }
        public string? ThumbnailExtension { get; set; }
    }

    public class TalentShowReactionRequest
    {
        public long TalentShowVideoId { get; set; }
        public long? MemberId { get; set; }
        public string? VisitorKey { get; set; }
        public bool? IsLike { get; set; }
        public int? Rating { get; set; }
    }

    public class TalentShowVideoApprovalRequest
    {
        public long TalentShowVideoId { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovalStatus { get; set; }
        public bool? IsActive { get; set; }
        public decimal? JudgeScore { get; set; }
        public string? JudgeFeedback { get; set; }
        public string? JudgeSuggestions { get; set; }
    }

    public class TalentShowVideoSummary
    {
        public long TalentShowVideoId { get; set; }
        public int LikeCount { get; set; }
        public int RatingCount { get; set; }
        public decimal AverageRating { get; set; }
        public decimal JudgeScore { get; set; }
        public decimal Score { get; set; }
    }

    public class TalentShowRegistrationRequest
    {
        public int TalentShowCampaignId { get; set; }
        public int TalentShowCategoryId { get; set; }
        public long? MemberId { get; set; }
        public string? MemberName { get; set; }
        public string AgeGroup { get; set; } = string.Empty;
        public bool AcceptedTerms { get; set; }
    }

    public class TalentShowRegistrationStatus
    {
        public long TalentShowRegistrationId { get; set; }
        public int TalentShowCampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public int TalentShowCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public long? MemberId { get; set; }
        public string? MemberName { get; set; }
        public string AgeGroup { get; set; } = string.Empty;
        public bool AcceptedTerms { get; set; }
        public int CurrentChallengeNumber { get; set; } = 1;
        public int CompletedChallenges { get; set; }
        public bool PortfolioCompleted { get; set; }
        public bool IsCampaignEnded { get; set; }
        public DateTime RegisteredDate { get; set; }
        public List<TalentShowChallengeStatus> Challenges { get; set; } = new();
    }

    public class TalentShowChallengeStatus
    {
        public int ChallengeNumber { get; set; }
        public string ChallengeLevel { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
        public bool IsUploaded { get; set; }
        public long? TalentShowVideoId { get; set; }
        public string? Title { get; set; }
        public string? ApprovalStatus { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
        public int LikeCount { get; set; }
        public decimal JudgeScore { get; set; }
        public string? JudgeFeedback { get; set; }
        public string? JudgeSuggestions { get; set; }
    }

    public class TalentShowRanking
    {
        public int RankNumber { get; set; }
        public int TalentShowCampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public int TalentShowCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public long? MemberId { get; set; }
        public string? MemberName { get; set; }
        public decimal JudgeScore { get; set; }
        public decimal ViewerRating { get; set; }
        public decimal EngagementScore { get; set; }
        public decimal ConsistencyScore { get; set; }
        public decimal FinalScore { get; set; }
        public int CompletedChallenges { get; set; }
    }
}
