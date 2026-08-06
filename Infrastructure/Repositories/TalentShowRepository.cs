using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class TalentShowRepository : ITalentShowRepository
    {
        private readonly IConfiguration _configuration;
        private static readonly object DemoLock = new();
        private static readonly Dictionary<long, TalentShowVideoSummary> DemoSummaries = CreateDemoVideos()
            .ToDictionary(
                video => video.TalentShowVideoId,
                video => new TalentShowVideoSummary
                {
                    TalentShowVideoId = video.TalentShowVideoId,
                    LikeCount = video.LikeCount,
                    RatingCount = video.RatingCount,
                    AverageRating = video.AverageRating,
                    JudgeScore = video.JudgeScore,
                    Score = video.Score
                });
        private static readonly Dictionary<long, (bool IsApproved, string ApprovalStatus)> DemoApprovals = CreateDemoVideos()
            .ToDictionary(
                video => video.TalentShowVideoId,
                video => (video.IsApproved, video.ApprovalStatus));

        public TalentShowRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<BaseResponse> SaveCampaignAsync(TalentShowCampaign campaign)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
DECLARE @NormalizedCampaignName NVARCHAR(150) = LTRIM(RTRIM(@CampaignName));
DECLARE @ResolvedDurationDays INT = CASE WHEN ISNULL(@DurationDays, 0) <= 0 THEN DATEDIFF(DAY, @UploadStartDate, @UploadEndDate) ELSE @DurationDays END;
DECLARE @ResolvedChallengeCount INT = CASE WHEN ISNULL(@ChallengeCount, 0) <= 0 THEN 3 ELSE @ChallengeCount END;
DECLARE @ResolvedDaysPerChallenge INT = CASE WHEN ISNULL(@DaysPerChallenge, 0) <= 0 THEN 7 ELSE @DaysPerChallenge END;

IF NULLIF(@NormalizedCampaignName, '') IS NULL
BEGIN
    SELECT 0 AS ResultId, 'Campaign name is required.' AS ResultMessage;
    RETURN;
END;

IF @UploadStartDate IS NULL OR @UploadEndDate IS NULL
BEGIN
    SELECT 0 AS ResultId, 'Upload start and end dates are required.' AS ResultMessage;
    RETURN;
END;

IF @UploadEndDate <= @UploadStartDate
BEGIN
    SELECT 0 AS ResultId, 'Upload end date must be after start date.' AS ResultMessage;
    RETURN;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.TalentShowCampaign
    WHERE CampaignName = @NormalizedCampaignName
      AND TalentShowCampaignId <> ISNULL(@TalentShowCampaignId, 0)
)
BEGIN
    SELECT 0 AS ResultId, 'Campaign name already exists.' AS ResultMessage;
    RETURN;
END;

IF ISNULL(@TalentShowCampaignId, 0) > 0
BEGIN
    UPDATE dbo.TalentShowCampaign
    SET CampaignName = @NormalizedCampaignName,
        EventName = @EventName,
        Description = @Description,
        Guidelines = @Guidelines,
        TermsAndConditions = @TermsAndConditions,
        DurationDays = @ResolvedDurationDays,
        ChallengeCount = @ResolvedChallengeCount,
        DaysPerChallenge = @ResolvedDaysPerChallenge,
        UploadStartDate = @UploadStartDate,
        UploadEndDate = @UploadEndDate,
        IsActive = @IsActive,
        DisplayOrder = ISNULL(@DisplayOrder, 0),
        UpdatedDate = SYSUTCDATETIME()
    WHERE TalentShowCampaignId = @TalentShowCampaignId;

    IF @@ROWCOUNT = 0
        SELECT 0 AS ResultId, 'Campaign not found.' AS ResultMessage;
    ELSE
        SELECT @TalentShowCampaignId AS ResultId, 'Campaign updated successfully.' AS ResultMessage;

    RETURN;
END;

INSERT INTO dbo.TalentShowCampaign
(
    CampaignName,
    EventName,
    Description,
    Guidelines,
    TermsAndConditions,
    DurationDays,
    ChallengeCount,
    DaysPerChallenge,
    UploadStartDate,
    UploadEndDate,
    IsActive,
    DisplayOrder
)
VALUES
(
    @NormalizedCampaignName,
    @EventName,
    @Description,
    @Guidelines,
    @TermsAndConditions,
    @ResolvedDurationDays,
    @ResolvedChallengeCount,
    @ResolvedDaysPerChallenge,
    @UploadStartDate,
    @UploadEndDate,
    @IsActive,
    ISNULL(@DisplayOrder, 0)
);

SELECT CAST(SCOPE_IDENTITY() AS INT) AS ResultId, 'Campaign saved successfully.' AS ResultMessage;";

            return await connection.QueryFirstOrDefaultAsync<BaseResponse>(sql, campaign) ?? new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Campaign was not saved."
                };
        }

        public async Task<List<TalentShowCampaign>> GetCampaignsAsync(bool includeInactive, bool uploadOpenOnly)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
SELECT TalentShowCampaignId,
       CampaignName,
       EventName,
       Description,
       Guidelines,
       TermsAndConditions,
       ISNULL(DurationDays, DATEDIFF(DAY, UploadStartDate, UploadEndDate)) AS DurationDays,
       ISNULL(ChallengeCount, 3) AS ChallengeCount,
       ISNULL(DaysPerChallenge, 7) AS DaysPerChallenge,
       UploadStartDate,
       UploadEndDate,
       IsActive,
       DisplayOrder,
       CAST(CASE
            WHEN IsActive = 1
             AND SYSUTCDATETIME() >= UploadStartDate
             AND SYSUTCDATETIME() <= UploadEndDate
            THEN 1 ELSE 0
       END AS BIT) AS IsUploadOpen,
       CreatedDate,
       UpdatedDate
FROM dbo.TalentShowCampaign
WHERE (@IncludeInactive = 1 OR IsActive = 1)
  AND (
      @UploadOpenOnly = 0
      OR (
          IsActive = 1
          AND SYSUTCDATETIME() >= UploadStartDate
          AND SYSUTCDATETIME() <= UploadEndDate
      )
  )
ORDER BY DisplayOrder, UploadStartDate DESC, CampaignName;";

            try
            {
                var result = (await connection.QueryAsync<TalentShowCampaign>(sql, new
                {
                    IncludeInactive = includeInactive,
                    UploadOpenOnly = uploadOpenOnly
                })).ToList();

                return result.Count == 0 ? GetDemoCampaigns(includeInactive, uploadOpenOnly) : result;
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return GetDemoCampaigns(includeInactive, uploadOpenOnly);
            }
        }

        public async Task<TalentShowCampaign?> GetCampaignByIdAsync(int talentShowCampaignId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
SELECT TalentShowCampaignId,
       CampaignName,
       EventName,
       Description,
       Guidelines,
       TermsAndConditions,
       ISNULL(DurationDays, DATEDIFF(DAY, UploadStartDate, UploadEndDate)) AS DurationDays,
       ISNULL(ChallengeCount, 3) AS ChallengeCount,
       ISNULL(DaysPerChallenge, 7) AS DaysPerChallenge,
       UploadStartDate,
       UploadEndDate,
       IsActive,
       DisplayOrder,
       CAST(CASE
            WHEN IsActive = 1
             AND SYSUTCDATETIME() >= UploadStartDate
             AND SYSUTCDATETIME() <= UploadEndDate
            THEN 1 ELSE 0
       END AS BIT) AS IsUploadOpen,
       CreatedDate,
       UpdatedDate
FROM dbo.TalentShowCampaign
WHERE TalentShowCampaignId = @TalentShowCampaignId;";

            try
            {
                return await connection.QueryFirstOrDefaultAsync<TalentShowCampaign>(sql, new { TalentShowCampaignId = talentShowCampaignId })
                    ?? GetDemoCampaigns(true, false).FirstOrDefault(campaign => campaign.TalentShowCampaignId == talentShowCampaignId);
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return GetDemoCampaigns(true, false).FirstOrDefault(campaign => campaign.TalentShowCampaignId == talentShowCampaignId);
            }
        }

        public async Task<BaseResponse> SaveCategoryAsync(TalentShowCategory category)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
DECLARE @NormalizedCategoryName NVARCHAR(100) = LTRIM(RTRIM(@CategoryName));

IF NULLIF(@NormalizedCategoryName, '') IS NULL
BEGIN
    SELECT 0 AS ResultId, 'Category name is required.' AS ResultMessage;
    RETURN;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.TalentShowCategory
    WHERE CategoryName = @NormalizedCategoryName
      AND TalentShowCategoryId <> ISNULL(@TalentShowCategoryId, 0)
)
BEGIN
    SELECT 0 AS ResultId, 'Category name already exists.' AS ResultMessage;
    RETURN;
END;

IF ISNULL(@TalentShowCategoryId, 0) > 0
BEGIN
    UPDATE dbo.TalentShowCategory
    SET CategoryName = @NormalizedCategoryName,
        Description = @Description,
        IsActive = @IsActive,
        DisplayOrder = ISNULL(@DisplayOrder, 0),
        UpdatedDate = SYSUTCDATETIME()
    WHERE TalentShowCategoryId = @TalentShowCategoryId;

    IF @@ROWCOUNT = 0
        SELECT 0 AS ResultId, 'Category not found.' AS ResultMessage;
    ELSE
        SELECT @TalentShowCategoryId AS ResultId, 'Category updated successfully.' AS ResultMessage;

    RETURN;
END;

INSERT INTO dbo.TalentShowCategory (CategoryName, Description, IsActive, DisplayOrder)
VALUES (@NormalizedCategoryName, @Description, @IsActive, ISNULL(@DisplayOrder, 0));

SELECT CAST(SCOPE_IDENTITY() AS INT) AS ResultId, 'Category saved successfully.' AS ResultMessage;";

            return await connection.QueryFirstOrDefaultAsync<BaseResponse>(sql, category) ?? new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Category was not saved."
                };
        }

        public async Task<List<TalentShowCategory>> GetCategoriesAsync(bool includeInactive)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
SELECT TalentShowCategoryId,
       CategoryName,
       Description,
       IsActive,
       DisplayOrder,
       CreatedDate,
       UpdatedDate
FROM dbo.TalentShowCategory
WHERE @IncludeInactive = 1 OR IsActive = 1
ORDER BY DisplayOrder, CategoryName;";

            try
            {
                var result = (await connection.QueryAsync<TalentShowCategory>(sql, new { IncludeInactive = includeInactive })).ToList();
                return result.Count == 0 ? GetDemoCategories(includeInactive) : result;
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return GetDemoCategories(includeInactive);
            }
        }

        public async Task<TalentShowCategory?> GetCategoryByIdAsync(int talentShowCategoryId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
SELECT TalentShowCategoryId,
       CategoryName,
       Description,
       IsActive,
       DisplayOrder,
       CreatedDate,
       UpdatedDate
FROM dbo.TalentShowCategory
WHERE TalentShowCategoryId = @TalentShowCategoryId;";

            try
            {
                return await connection.QueryFirstOrDefaultAsync<TalentShowCategory>(sql, new { TalentShowCategoryId = talentShowCategoryId })
                    ?? GetDemoCategories(true).FirstOrDefault(category => category.TalentShowCategoryId == talentShowCategoryId);
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return GetDemoCategories(true).FirstOrDefault(category => category.TalentShowCategoryId == talentShowCategoryId);
            }
        }

        public async Task<BaseResponse> RegisterForCampaignAsync(TalentShowRegistrationRequest request)
        {
            if (request.TalentShowCampaignId < 0)
            {
                return new BaseResponse
                {
                    ResultId = request.TalentShowCampaignId,
                    ResultMessage = "Demo campaign registration previewed successfully."
                };
            }

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
DECLARE @NormalizedAgeGroup NVARCHAR(40) = NULLIF(LTRIM(RTRIM(@AgeGroup)), '');
DECLARE @NormalizedMemberName NVARCHAR(150) = NULLIF(LTRIM(RTRIM(@MemberName)), '');

IF @TalentShowCampaignId <= 0 OR @TalentShowCategoryId <= 0
BEGIN
    SELECT 0 AS ResultId, 'Campaign and category are required.' AS ResultMessage;
    RETURN;
END;

IF @NormalizedAgeGroup IS NULL
BEGIN
    SELECT 0 AS ResultId, 'Age group is required.' AS ResultMessage;
    RETURN;
END;

IF ISNULL(@AcceptedTerms, 0) = 0
BEGIN
    SELECT 0 AS ResultId, 'Terms and conditions must be accepted.' AS ResultMessage;
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TalentShowCampaign WHERE TalentShowCampaignId = @TalentShowCampaignId AND IsActive = 1)
BEGIN
    SELECT 0 AS ResultId, 'Active campaign is required.' AS ResultMessage;
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TalentShowCategory WHERE TalentShowCategoryId = @TalentShowCategoryId AND IsActive = 1)
BEGIN
    SELECT 0 AS ResultId, 'Active category is required.' AS ResultMessage;
    RETURN;
END;

IF @MemberId IS NULL
BEGIN
    SELECT 0 AS ResultId, 'Member id is required for campaign registration.' AS ResultMessage;
    RETURN;
END;

MERGE dbo.TalentShowRegistration AS target
USING (SELECT @TalentShowCampaignId AS TalentShowCampaignId, @MemberId AS MemberId) AS source
   ON target.TalentShowCampaignId = source.TalentShowCampaignId
  AND target.MemberId = source.MemberId
WHEN MATCHED THEN
    UPDATE SET TalentShowCategoryId = @TalentShowCategoryId,
               MemberName = COALESCE(@NormalizedMemberName, target.MemberName),
               AgeGroup = @NormalizedAgeGroup,
               AcceptedTerms = 1,
               UpdatedDate = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (TalentShowCampaignId, TalentShowCategoryId, MemberId, MemberName, AgeGroup, AcceptedTerms)
    VALUES (@TalentShowCampaignId, @TalentShowCategoryId, @MemberId, @NormalizedMemberName, @NormalizedAgeGroup, 1);

SELECT CAST(ISNULL(SCOPE_IDENTITY(), (
    SELECT TalentShowRegistrationId
    FROM dbo.TalentShowRegistration
    WHERE TalentShowCampaignId = @TalentShowCampaignId
      AND MemberId = @MemberId
)) AS INT) AS ResultId,
'Campaign registration saved successfully. Challenge 1 is unlocked.' AS ResultMessage;";

            try
            {
                return await connection.QueryFirstOrDefaultAsync<BaseResponse>(sql, request) ?? new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Campaign registration was not saved."
                };
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Create Talent Show registration tables before registering for campaigns."
                };
            }
        }

        public async Task<TalentShowRegistrationStatus?> GetRegistrationStatusAsync(int talentShowCampaignId, long? memberId, string? visitorKey)
        {
            if (talentShowCampaignId < 0)
                return GetDemoRegistrationStatus(talentShowCampaignId, memberId);

            if (memberId == null)
                return null;

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
WITH ReactionSummary AS
(
    SELECT TalentShowVideoId,
           SUM(CASE WHEN IsLiked = 1 THEN 1 ELSE 0 END) AS LikeCount,
           COUNT(Rating) AS RatingCount,
           CAST(ISNULL(AVG(CAST(Rating AS DECIMAL(5,2))), 0) AS DECIMAL(5,2)) AS AverageRating
    FROM dbo.TalentShowReaction
    GROUP BY TalentShowVideoId
),
Registered AS
(
    SELECT r.TalentShowRegistrationId,
           r.TalentShowCampaignId,
           cp.CampaignName,
           r.TalentShowCategoryId,
           c.CategoryName,
           r.MemberId,
           r.MemberName,
           r.AgeGroup,
           r.AcceptedTerms,
           r.CreatedDate AS RegisteredDate,
           CAST(CASE WHEN SYSUTCDATETIME() > cp.UploadEndDate THEN 1 ELSE 0 END AS BIT) AS IsCampaignEnded
    FROM dbo.TalentShowRegistration r
    INNER JOIN dbo.TalentShowCampaign cp ON cp.TalentShowCampaignId = r.TalentShowCampaignId
    INNER JOIN dbo.TalentShowCategory c ON c.TalentShowCategoryId = r.TalentShowCategoryId
    WHERE r.TalentShowCampaignId = @TalentShowCampaignId
      AND r.MemberId = @MemberId
)
SELECT TalentShowRegistrationId,
       TalentShowCampaignId,
       CampaignName,
       TalentShowCategoryId,
       CategoryName,
       MemberId,
       MemberName,
       AgeGroup,
       AcceptedTerms,
       RegisteredDate,
       IsCampaignEnded
FROM Registered;

SELECT n.ChallengeNumber,
       CASE n.ChallengeNumber
            WHEN 1 THEN 'Beginner'
            WHEN 2 THEN 'Intermediate'
            ELSE 'Advanced / Signature Performance'
       END AS ChallengeLevel,
       CAST(CASE
            WHEN n.ChallengeNumber = 1 THEN 1
            WHEN EXISTS (
                SELECT 1
                FROM dbo.TalentShowVideo pv
                WHERE pv.TalentShowCampaignId = @TalentShowCampaignId
                  AND pv.MemberId = @MemberId
                  AND pv.ChallengeNumber = n.ChallengeNumber - 1
                  AND pv.IsActive = 1
                  AND pv.IsApproved = 1
            ) THEN 1 ELSE 0
       END AS BIT) AS IsUnlocked,
       CAST(CASE WHEN v.TalentShowVideoId IS NULL THEN 0 ELSE 1 END AS BIT) AS IsUploaded,
       v.TalentShowVideoId,
       v.Title,
       v.ApprovalStatus,
       ISNULL(s.AverageRating, 0) AS AverageRating,
       ISNULL(s.RatingCount, 0) AS RatingCount,
       ISNULL(s.LikeCount, 0) AS LikeCount,
       ISNULL(v.JudgeScore, 0) AS JudgeScore,
       v.JudgeFeedback,
       v.JudgeSuggestions
FROM (VALUES (1), (2), (3)) n(ChallengeNumber)
LEFT JOIN dbo.TalentShowVideo v
  ON v.TalentShowCampaignId = @TalentShowCampaignId
 AND v.MemberId = @MemberId
 AND v.ChallengeNumber = n.ChallengeNumber
 AND v.IsActive = 1
LEFT JOIN ReactionSummary s ON s.TalentShowVideoId = v.TalentShowVideoId
ORDER BY n.ChallengeNumber;";

            try
            {
                using var grid = await connection.QueryMultipleAsync(sql, new
                {
                    TalentShowCampaignId = talentShowCampaignId,
                    MemberId = memberId,
                    VisitorKey = visitorKey
                });

                var status = await grid.ReadFirstOrDefaultAsync<TalentShowRegistrationStatus>();
                if (status == null)
                    return null;

                status.Challenges = (await grid.ReadAsync<TalentShowChallengeStatus>()).ToList();
                status.CompletedChallenges = status.Challenges.Count(challenge => challenge.IsUploaded);
                status.CurrentChallengeNumber = status.Challenges.FirstOrDefault(challenge => challenge.IsUnlocked && !challenge.IsUploaded)?.ChallengeNumber
                    ?? Math.Min(status.CompletedChallenges + 1, 3);
                status.PortfolioCompleted = status.CompletedChallenges >= 3;

                return status;
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return null;
            }
        }

        public async Task<BaseResponse> SaveVideoAsync(TalentShowVideo video)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
DECLARE @NormalizedTitle NVARCHAR(160) = LTRIM(RTRIM(@Title));
DECLARE @ResolvedChallengeNumber INT = CASE WHEN ISNULL(@ChallengeNumber, 0) <= 0 THEN 1 ELSE @ChallengeNumber END;
DECLARE @ResolvedAgeGroup NVARCHAR(40) = NULLIF(LTRIM(RTRIM(@AgeGroup)), '');
DECLARE @ResolvedChallengeLevel NVARCHAR(80) = CASE @ResolvedChallengeNumber
    WHEN 1 THEN 'Beginner'
    WHEN 2 THEN 'Intermediate'
    ELSE 'Advanced / Signature Performance'
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.TalentShowCampaign
    WHERE TalentShowCampaignId = @TalentShowCampaignId
      AND IsActive = 1
)
BEGIN
    SELECT 0 AS ResultId, 'Active campaign is required.' AS ResultMessage;
    RETURN;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.TalentShowCampaign
    WHERE TalentShowCampaignId = @TalentShowCampaignId
      AND SYSUTCDATETIME() < UploadStartDate
)
BEGIN
    SELECT 0 AS ResultId, 'Campaign upload window has not started.' AS ResultMessage;
    RETURN;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.TalentShowCampaign
    WHERE TalentShowCampaignId = @TalentShowCampaignId
      AND SYSUTCDATETIME() > UploadEndDate
)
BEGIN
    SELECT 0 AS ResultId, 'Campaign upload window is closed.' AS ResultMessage;
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TalentShowCategory WHERE TalentShowCategoryId = @TalentShowCategoryId AND IsActive = 1)
BEGIN
    SELECT 0 AS ResultId, 'Active category is required.' AS ResultMessage;
    RETURN;
END;

IF NULLIF(@NormalizedTitle, '') IS NULL OR NULLIF(@VideoPath, '') IS NULL
BEGIN
    SELECT 0 AS ResultId, 'Title and video path are required.' AS ResultMessage;
    RETURN;
END;

IF @ResolvedChallengeNumber NOT BETWEEN 1 AND 3
BEGIN
    SELECT 0 AS ResultId, 'Challenge number must be between 1 and 3.' AS ResultMessage;
    RETURN;
END;

IF @MemberId IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.TalentShowRegistration
        WHERE TalentShowCampaignId = @TalentShowCampaignId
          AND TalentShowCategoryId = @TalentShowCategoryId
          AND MemberId = @MemberId
          AND AcceptedTerms = 1
          AND (@ResolvedAgeGroup IS NULL OR AgeGroup = @ResolvedAgeGroup)
    )
    BEGIN
        SELECT 0 AS ResultId, 'Register for this campaign, category, and age group before uploading.' AS ResultMessage;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.TalentShowVideo
        WHERE TalentShowCampaignId = @TalentShowCampaignId
          AND MemberId = @MemberId
          AND ChallengeNumber = @ResolvedChallengeNumber
          AND TalentShowVideoId <> ISNULL(@TalentShowVideoId, 0)
          AND IsActive = 1
    )
    BEGIN
        SELECT 0 AS ResultId, 'This challenge has already been uploaded for the campaign.' AS ResultMessage;
        RETURN;
    END;

    IF @ResolvedChallengeNumber > 1
       AND NOT EXISTS (
           SELECT 1
           FROM dbo.TalentShowVideo
           WHERE TalentShowCampaignId = @TalentShowCampaignId
             AND MemberId = @MemberId
             AND ChallengeNumber = @ResolvedChallengeNumber - 1
             AND IsActive = 1
             AND IsApproved = 1
       )
    BEGIN
        SELECT 0 AS ResultId, 'Previous challenge must be approved before this challenge is unlocked.' AS ResultMessage;
        RETURN;
    END;
END;

IF ISNULL(@TalentShowVideoId, 0) > 0
BEGIN
    UPDATE dbo.TalentShowVideo
    SET TalentShowCampaignId = @TalentShowCampaignId,
        TalentShowCategoryId = @TalentShowCategoryId,
        MemberId = @MemberId,
        MemberName = @MemberName,
        Title = @NormalizedTitle,
        Description = @Description,
        VideoPath = @VideoPath,
        ThumbnailPath = @ThumbnailPath,
        ChallengeNumber = @ResolvedChallengeNumber,
        ChallengeLevel = @ResolvedChallengeLevel,
        AgeGroup = COALESCE(@ResolvedAgeGroup, AgeGroup),
        IsApproved = @IsApproved,
        ApprovalStatus = CASE
            WHEN @IsApproved = 1 THEN 'Approved'
            WHEN NULLIF(@ApprovalStatus, '') IS NULL THEN 'Pending'
            ELSE @ApprovalStatus
        END,
        IsActive = @IsActive,
        UpdatedDate = SYSUTCDATETIME()
    WHERE TalentShowVideoId = @TalentShowVideoId;

    IF @@ROWCOUNT = 0
        SELECT 0 AS ResultId, 'Video not found.' AS ResultMessage;
    ELSE
        SELECT CAST(@TalentShowVideoId AS INT) AS ResultId, 'Video updated successfully.' AS ResultMessage;

    RETURN;
END;

INSERT INTO dbo.TalentShowVideo
(
    TalentShowCampaignId,
    TalentShowCategoryId,
    MemberId,
    MemberName,
    Title,
    Description,
    VideoPath,
    ThumbnailPath,
    ChallengeNumber,
    ChallengeLevel,
    AgeGroup,
    IsApproved,
    ApprovalStatus,
    IsActive
)
VALUES
(
    @TalentShowCampaignId,
    @TalentShowCategoryId,
    @MemberId,
    @MemberName,
    @NormalizedTitle,
    @Description,
    @VideoPath,
    @ThumbnailPath,
    @ResolvedChallengeNumber,
    @ResolvedChallengeLevel,
    @ResolvedAgeGroup,
    @IsApproved,
    CASE
        WHEN @IsApproved = 1 THEN 'Approved'
        WHEN NULLIF(@ApprovalStatus, '') IS NULL THEN 'Pending'
        ELSE @ApprovalStatus
    END,
    @IsActive
);

SELECT CAST(SCOPE_IDENTITY() AS INT) AS ResultId, 'Video submitted for admin approval successfully.' AS ResultMessage;";

            return await connection.QueryFirstOrDefaultAsync<BaseResponse>(sql, video) ?? new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Video was not saved."
                };
        }

        public async Task<List<TalentShowVideo>> GetVideosAsync(TalentShowVideoFilter filter)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
WITH ReactionSummary AS
(
    SELECT TalentShowVideoId,
           SUM(CASE WHEN IsLiked = 1 THEN 1 ELSE 0 END) AS LikeCount,
           COUNT(Rating) AS RatingCount,
           AVG(CAST(Rating AS DECIMAL(5,2))) AS AverageRating
    FROM dbo.TalentShowReaction
    GROUP BY TalentShowVideoId
)
SELECT v.TalentShowVideoId,
       ISNULL(v.TalentShowCampaignId, 0) AS TalentShowCampaignId,
       ISNULL(cp.CampaignName, '') AS CampaignName,
       cp.EventName,
       cp.UploadStartDate,
       cp.UploadEndDate,
       v.TalentShowCategoryId,
       c.CategoryName,
       v.MemberId,
       v.MemberName,
       v.Title,
       v.Description,
       v.VideoPath,
       v.ThumbnailPath,
       ISNULL(v.ChallengeNumber, 1) AS ChallengeNumber,
       ISNULL(v.ChallengeLevel, CASE ISNULL(v.ChallengeNumber, 1)
            WHEN 1 THEN 'Beginner'
            WHEN 2 THEN 'Intermediate'
            ELSE 'Advanced / Signature Performance'
       END) AS ChallengeLevel,
       v.AgeGroup,
       v.IsApproved,
       ISNULL(v.ApprovalStatus, CASE WHEN v.IsApproved = 1 THEN 'Approved' ELSE 'Pending' END) AS ApprovalStatus,
       v.IsActive,
       ISNULL(s.LikeCount, 0) AS LikeCount,
       ISNULL(s.RatingCount, 0) AS RatingCount,
       CAST(ISNULL(s.AverageRating, 0) AS DECIMAL(5,2)) AS AverageRating,
       ISNULL(v.JudgeScore, 0) AS JudgeScore,
       v.JudgeFeedback,
       v.JudgeSuggestions,
       CAST(
           ISNULL(v.JudgeScore, 0)
           + ISNULL(s.LikeCount, 0)
           + (ISNULL(s.AverageRating, 0) * ISNULL(s.RatingCount, 0))
           AS DECIMAL(10,2)
       ) AS Score,
       v.CreatedDate,
       v.UpdatedDate
FROM dbo.TalentShowVideo v
INNER JOIN dbo.TalentShowCategory c ON c.TalentShowCategoryId = v.TalentShowCategoryId
LEFT JOIN dbo.TalentShowCampaign cp ON cp.TalentShowCampaignId = v.TalentShowCampaignId
LEFT JOIN ReactionSummary s ON s.TalentShowVideoId = v.TalentShowVideoId
WHERE v.IsActive = 1
  AND (@ApprovedOnly = 0 OR v.IsApproved = 1)
  AND (@TalentShowCampaignId IS NULL OR v.TalentShowCampaignId = @TalentShowCampaignId)
  AND (@TalentShowCategoryId IS NULL OR v.TalentShowCategoryId = @TalentShowCategoryId)
  AND (@ChallengeNumber IS NULL OR v.ChallengeNumber = @ChallengeNumber)
  AND (@AgeGroup IS NULL OR v.AgeGroup = @AgeGroup)
  AND (
      @SearchText IS NULL
      OR v.Title LIKE '%' + @SearchText + '%'
      OR v.MemberName LIKE '%' + @SearchText + '%'
      OR v.Description LIKE '%' + @SearchText + '%'
      OR c.CategoryName LIKE '%' + @SearchText + '%'
      OR cp.CampaignName LIKE '%' + @SearchText + '%'
      OR cp.EventName LIKE '%' + @SearchText + '%'
  )
ORDER BY Score DESC, LikeCount DESC, AverageRating DESC, v.CreatedDate DESC
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;";

            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize is <= 0 or > 100 ? 24 : filter.PageSize;
            var searchText = string.IsNullOrWhiteSpace(filter.SearchText) ? null : filter.SearchText.Trim();

            var parameters = new
            {
                filter.TalentShowCampaignId,
                filter.TalentShowCategoryId,
                filter.ChallengeNumber,
                AgeGroup = string.IsNullOrWhiteSpace(filter.AgeGroup) ? null : filter.AgeGroup.Trim(),
                SearchText = searchText,
                filter.ApprovedOnly,
                PageSize = pageSize,
                Offset = (pageNumber - 1) * pageSize
            };

            try
            {
                var result = (await connection.QueryAsync<TalentShowVideo>(sql, parameters)).ToList();
                return result.Count == 0 ? GetDemoVideos(filter) : result;
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return GetDemoVideos(filter);
            }
        }

        public async Task<BaseResponse> SaveVideoApprovalAsync(TalentShowVideoApprovalRequest request)
        {
            if (request.TalentShowVideoId < 0)
            {
                lock (DemoLock)
                {
                    DemoApprovals[request.TalentShowVideoId] = (
                        request.IsApproved,
                        request.IsApproved ? "Approved" : "Declined");
                }

                return new BaseResponse
                {
                    ResultId = (int)request.TalentShowVideoId,
                    ResultMessage = request.IsApproved
                        ? "Demo video approved for preview."
                        : "Demo video declined for preview."
                };
            }

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
IF @TalentShowVideoId <= 0
BEGIN
    SELECT 0 AS ResultId, 'Video id is required.' AS ResultMessage;
    RETURN;
END;

UPDATE dbo.TalentShowVideo
SET IsApproved = @IsApproved,
    ApprovalStatus = CASE WHEN @IsApproved = 1 THEN 'Approved' ELSE 'Declined' END,
    IsActive = COALESCE(@IsActive, IsActive),
    JudgeScore = COALESCE(@JudgeScore, JudgeScore),
    JudgeFeedback = COALESCE(NULLIF(@JudgeFeedback, ''), JudgeFeedback),
    JudgeSuggestions = COALESCE(NULLIF(@JudgeSuggestions, ''), JudgeSuggestions),
    UpdatedDate = SYSUTCDATETIME()
WHERE TalentShowVideoId = @TalentShowVideoId;

IF @@ROWCOUNT = 0
    SELECT 0 AS ResultId, 'Video not found.' AS ResultMessage;
ELSE
    SELECT CAST(@TalentShowVideoId AS INT) AS ResultId,
           CASE WHEN @IsApproved = 1 THEN 'Video approved successfully.' ELSE 'Video declined successfully.' END AS ResultMessage;";

            try
            {
                return await connection.QueryFirstOrDefaultAsync<BaseResponse>(sql, request) ?? new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Video approval was not updated."
                };
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Create Talent Show tables before approving videos."
                };
            }
        }

        public async Task<BaseResponse> SaveReactionAsync(TalentShowReactionRequest request)
        {
            if (request.TalentShowVideoId < 0)
                return SaveDemoReaction(request);

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
DECLARE @ResolvedVisitorKey NVARCHAR(120) = NULLIF(LTRIM(RTRIM(@VisitorKey)), '');
DECLARE @VoterKey NVARCHAR(140);

IF @Rating IS NOT NULL AND (@Rating < 1 OR @Rating > 5)
BEGIN
    SELECT 0 AS ResultId, 'Rating must be between 1 and 5.' AS ResultMessage;
    RETURN;
END;

IF @IsLike IS NULL AND @Rating IS NULL
BEGIN
    SELECT 0 AS ResultId, 'Like or rating is required.' AS ResultMessage;
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TalentShowVideo WHERE TalentShowVideoId = @TalentShowVideoId AND IsActive = 1)
BEGIN
    SELECT 0 AS ResultId, 'Video not found.' AS ResultMessage;
    RETURN;
END;

SET @VoterKey = CASE
    WHEN @MemberId IS NOT NULL THEN CONCAT('M:', @MemberId)
    WHEN @ResolvedVisitorKey IS NOT NULL THEN CONCAT('V:', @ResolvedVisitorKey)
    ELSE NULL
END;

IF @VoterKey IS NULL
BEGIN
    SELECT 0 AS ResultId, 'Member id or visitor key is required.' AS ResultMessage;
    RETURN;
END;

MERGE dbo.TalentShowReaction AS target
USING (SELECT @TalentShowVideoId AS TalentShowVideoId, @VoterKey AS VoterKey) AS source
   ON target.TalentShowVideoId = source.TalentShowVideoId
  AND target.VoterKey = source.VoterKey
WHEN MATCHED THEN
    UPDATE SET MemberId = COALESCE(@MemberId, target.MemberId),
               VisitorKey = COALESCE(@ResolvedVisitorKey, target.VisitorKey),
               IsLiked = CASE WHEN @IsLike IS NULL THEN target.IsLiked ELSE @IsLike END,
               Rating = CASE WHEN @Rating IS NULL THEN target.Rating ELSE CONVERT(TINYINT, @Rating) END,
               UpdatedDate = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (TalentShowVideoId, MemberId, VisitorKey, VoterKey, IsLiked, Rating)
    VALUES (@TalentShowVideoId, @MemberId, @ResolvedVisitorKey, @VoterKey, ISNULL(@IsLike, 0), CONVERT(TINYINT, @Rating));

SELECT CAST(@TalentShowVideoId AS INT) AS ResultId, 'Reaction saved successfully.' AS ResultMessage;";

            try
            {
                return await connection.QueryFirstOrDefaultAsync<BaseResponse>(sql, request) ?? new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Reaction was not saved."
                };
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return new BaseResponse
                {
                    ResultId = 0,
                    ResultMessage = "Create Talent Show tables before saving reactions for uploaded videos."
                };
            }
        }

        public async Task<TalentShowVideoSummary?> GetVideoSummaryAsync(long talentShowVideoId)
        {
            if (talentShowVideoId < 0)
                return GetDemoSummary(talentShowVideoId);

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
SELECT v.TalentShowVideoId,
       SUM(CASE WHEN r.IsLiked = 1 THEN 1 ELSE 0 END) AS LikeCount,
       COUNT(r.Rating) AS RatingCount,
       CAST(ISNULL(AVG(CAST(r.Rating AS DECIMAL(5,2))), 0) AS DECIMAL(5,2)) AS AverageRating,
       ISNULL(v.JudgeScore, 0) AS JudgeScore,
       CAST(
           ISNULL(v.JudgeScore, 0)
           + ISNULL(SUM(CASE WHEN r.IsLiked = 1 THEN 1 ELSE 0 END), 0)
           + (ISNULL(AVG(CAST(r.Rating AS DECIMAL(5,2))), 0) * COUNT(r.Rating))
           AS DECIMAL(10,2)
       ) AS Score
FROM dbo.TalentShowVideo v
LEFT JOIN dbo.TalentShowReaction r ON r.TalentShowVideoId = v.TalentShowVideoId
WHERE v.TalentShowVideoId = @TalentShowVideoId
GROUP BY v.TalentShowVideoId, v.JudgeScore;";

            try
            {
                return await connection.QueryFirstOrDefaultAsync<TalentShowVideoSummary>(sql, new { TalentShowVideoId = talentShowVideoId });
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return null;
            }
        }

        public async Task<List<TalentShowRanking>> GetRankingsAsync(int? talentShowCampaignId, int? talentShowCategoryId, string? ageGroup)
        {
            if (talentShowCampaignId is < 0 || talentShowCategoryId is < 0)
                return GetDemoRankings(talentShowCampaignId, talentShowCategoryId, ageGroup);

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            const string sql = @"
WITH ReactionSummary AS
(
    SELECT TalentShowVideoId,
           SUM(CASE WHEN IsLiked = 1 THEN 1 ELSE 0 END) AS LikeCount,
           COUNT(Rating) AS RatingCount,
           CAST(ISNULL(AVG(CAST(Rating AS DECIMAL(5,2))), 0) AS DECIMAL(5,2)) AS AverageRating
    FROM dbo.TalentShowReaction
    GROUP BY TalentShowVideoId
),
MemberScores AS
(
    SELECT v.TalentShowCampaignId,
           cp.CampaignName,
           v.TalentShowCategoryId,
           c.CategoryName,
           ISNULL(v.AgeGroup, r.AgeGroup) AS AgeGroup,
           v.MemberId,
           COALESCE(v.MemberName, r.MemberName) AS MemberName,
           COUNT(DISTINCT v.ChallengeNumber) AS CompletedChallenges,
           CAST(ISNULL(AVG(NULLIF(v.JudgeScore, 0)), 0) AS DECIMAL(8,2)) AS JudgeScore,
           CAST(ISNULL(AVG(s.AverageRating), 0) AS DECIMAL(8,2)) AS ViewerRating,
           CAST(ISNULL(SUM(s.LikeCount), 0) + ISNULL(SUM(s.RatingCount), 0) + ISNULL(COUNT(v.TalentShowVideoId), 0) AS DECIMAL(8,2)) AS EngagementScore,
           CAST((COUNT(DISTINCT v.ChallengeNumber) / 3.0) * 100 AS DECIMAL(8,2)) AS ConsistencyScore
    FROM dbo.TalentShowVideo v
    INNER JOIN dbo.TalentShowCampaign cp ON cp.TalentShowCampaignId = v.TalentShowCampaignId
    INNER JOIN dbo.TalentShowCategory c ON c.TalentShowCategoryId = v.TalentShowCategoryId
    LEFT JOIN dbo.TalentShowRegistration r
      ON r.TalentShowCampaignId = v.TalentShowCampaignId
     AND r.MemberId = v.MemberId
    LEFT JOIN ReactionSummary s ON s.TalentShowVideoId = v.TalentShowVideoId
    WHERE v.IsActive = 1
      AND v.IsApproved = 1
      AND (@TalentShowCampaignId IS NULL OR v.TalentShowCampaignId = @TalentShowCampaignId)
      AND (@TalentShowCategoryId IS NULL OR v.TalentShowCategoryId = @TalentShowCategoryId)
      AND (@AgeGroup IS NULL OR ISNULL(v.AgeGroup, r.AgeGroup) = @AgeGroup)
    GROUP BY v.TalentShowCampaignId,
             cp.CampaignName,
             v.TalentShowCategoryId,
             c.CategoryName,
             ISNULL(v.AgeGroup, r.AgeGroup),
             v.MemberId,
             COALESCE(v.MemberName, r.MemberName)
),
FinalScores AS
(
    SELECT *,
           CAST(
               (JudgeScore * 0.45)
               + (ViewerRating * 10 * 0.25)
               + (CASE WHEN EngagementScore > 100 THEN 100 ELSE EngagementScore END * 0.15)
               + (ConsistencyScore * 0.15)
               AS DECIMAL(8,2)
           ) AS FinalScore
    FROM MemberScores
)
SELECT ROW_NUMBER() OVER (
           PARTITION BY TalentShowCampaignId, TalentShowCategoryId, AgeGroup
           ORDER BY FinalScore DESC, CompletedChallenges DESC, EngagementScore DESC
       ) AS RankNumber,
       TalentShowCampaignId,
       CampaignName,
       TalentShowCategoryId,
       CategoryName,
       ISNULL(AgeGroup, '') AS AgeGroup,
       MemberId,
       MemberName,
       JudgeScore,
       ViewerRating,
       EngagementScore,
       ConsistencyScore,
       FinalScore,
       CompletedChallenges
FROM FinalScores
ORDER BY CategoryName, AgeGroup, RankNumber;";

            try
            {
                return (await connection.QueryAsync<TalentShowRanking>(sql, new
                {
                    TalentShowCampaignId = talentShowCampaignId,
                    TalentShowCategoryId = talentShowCategoryId,
                    AgeGroup = string.IsNullOrWhiteSpace(ageGroup) ? null : ageGroup.Trim()
                })).ToList();
            }
            catch (SqlException ex) when (IsTalentShowSchemaMissing(ex))
            {
                return GetDemoRankings(talentShowCampaignId, talentShowCategoryId, ageGroup);
            }
        }

        private static bool IsTalentShowSchemaMissing(SqlException ex)
        {
            return ex.Errors.Cast<SqlError>().Any(error => error.Number is 208 or 207 or 2812);
        }

        private static List<TalentShowCampaign> GetDemoCampaigns(bool includeInactive, bool uploadOpenOnly)
        {
            var now = DateTime.UtcNow;
            var campaigns = new List<TalentShowCampaign>
            {
                new()
                {
                    TalentShowCampaignId = -1,
                    CampaignName = "Christmas Talent Fest",
                    EventName = "Christmas",
                    Description = "Demo Christmas campaign with an open upload window.",
                    Guidelines = "Register once, stay in the selected category and age group, and upload one challenge each week.",
                    TermsAndConditions = "One rating per member. Uploaded videos may be reviewed by judges and published to the gallery.",
                    DurationDays = 30,
                    ChallengeCount = 3,
                    DaysPerChallenge = 7,
                    UploadStartDate = now.AddDays(-7),
                    UploadEndDate = now.AddDays(21),
                    IsActive = true,
                    IsUploadOpen = true,
                    DisplayOrder = 10
                },
                new()
                {
                    TalentShowCampaignId = -2,
                    CampaignName = "New Year Showcase",
                    EventName = "New Year",
                    Description = "Demo New Year campaign for ended submissions and active voting.",
                    Guidelines = "Complete all three challenges in the same category for the final assessment.",
                    TermsAndConditions = "Community ratings, judge scores, and engagement will be used for ranking.",
                    DurationDays = 30,
                    ChallengeCount = 3,
                    DaysPerChallenge = 7,
                    UploadStartDate = now.AddDays(-30),
                    UploadEndDate = now.AddDays(-2),
                    IsActive = true,
                    IsUploadOpen = false,
                    DisplayOrder = 20
                }
            };

            if (!includeInactive)
                campaigns = campaigns.Where(campaign => campaign.IsActive).ToList();

            if (uploadOpenOnly)
                campaigns = campaigns.Where(campaign => campaign.IsUploadOpen).ToList();

            return campaigns;
        }

        private static List<TalentShowCategory> GetDemoCategories(bool includeInactive)
        {
            var categories = new List<TalentShowCategory>
            {
                new() { TalentShowCategoryId = -1, CategoryName = "Music", Description = "Demo music performances.", IsActive = true, DisplayOrder = 10 },
                new() { TalentShowCategoryId = -2, CategoryName = "Singing", Description = "Demo vocal performances.", IsActive = true, DisplayOrder = 20 },
                new() { TalentShowCategoryId = -3, CategoryName = "Preaching", Description = "Demo spiritual talks and short sermons.", IsActive = true, DisplayOrder = 30 },
                new() { TalentShowCategoryId = -4, CategoryName = "Dance", Description = "Demo dance performances.", IsActive = true, DisplayOrder = 40 },
                new() { TalentShowCategoryId = -5, CategoryName = "Instrumental", Description = "Demo instrumental entries.", IsActive = true, DisplayOrder = 50 }
            };

            return includeInactive ? categories : categories.Where(category => category.IsActive).ToList();
        }

        private static List<TalentShowVideo> GetDemoVideos(TalentShowVideoFilter filter)
        {
            var searchText = filter.SearchText?.Trim();
            var videos = CreateDemoVideos();

            if (filter.TalentShowCampaignId.HasValue)
                videos = videos
                    .Where(video => video.TalentShowCampaignId == filter.TalentShowCampaignId.Value)
                    .ToList();

            if (filter.TalentShowCategoryId.HasValue)
                videos = videos
                    .Where(video => video.TalentShowCategoryId == filter.TalentShowCategoryId.Value)
                    .ToList();

            if (filter.ChallengeNumber.HasValue)
                videos = videos
                    .Where(video => video.ChallengeNumber == filter.ChallengeNumber.Value)
                    .ToList();

            if (!string.IsNullOrWhiteSpace(filter.AgeGroup))
                videos = videos
                    .Where(video => string.Equals(video.AgeGroup, filter.AgeGroup.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                videos = videos
                    .Where(video =>
                        Contains(video.Title, searchText)
                        || Contains(video.MemberName, searchText)
                        || Contains(video.Description, searchText)
                        || Contains(video.CampaignName, searchText)
                        || Contains(video.EventName, searchText)
                        || Contains(video.CategoryName, searchText))
                    .ToList();
            }

            foreach (var video in videos)
            {
                var approval = GetDemoApproval(video.TalentShowVideoId);
                if (approval.HasValue)
                {
                    video.IsApproved = approval.Value.IsApproved;
                    video.ApprovalStatus = approval.Value.ApprovalStatus;
                }

                var summary = GetDemoSummary(video.TalentShowVideoId);
                if (summary == null)
                    continue;

                video.LikeCount = summary.LikeCount;
                video.RatingCount = summary.RatingCount;
                video.AverageRating = summary.AverageRating;
                video.Score = summary.Score;
            }

            if (filter.ApprovedOnly)
                videos = videos.Where(video => video.IsApproved).ToList();

            return videos
                .OrderByDescending(video => video.Score)
                .ThenByDescending(video => video.LikeCount)
                .ThenByDescending(video => video.AverageRating)
                .ToList();
        }

        private static List<TalentShowVideo> CreateDemoVideos()
        {
            return new List<TalentShowVideo>
            {
                new()
                {
                    TalentShowVideoId = -1,
                    TalentShowCampaignId = -2,
                    CampaignName = "New Year Showcase",
                    EventName = "New Year",
                    UploadStartDate = DateTime.UtcNow.AddDays(-30),
                    UploadEndDate = DateTime.UtcNow.AddDays(-2),
                    TalentShowCategoryId = -2,
                    CategoryName = "Singing",
                    AgeGroup = "13-17",
                    ChallengeNumber = 3,
                    ChallengeLevel = "Advanced / Signature Performance",
                    MemberName = "Ananya R.",
                    Title = "Classical Vocal Demo",
                    Description = "Sample entry showing how member videos will appear after uploads.",
                    VideoPath = "https://www.w3schools.com/html/mov_bbb.mp4",
                    LikeCount = 42,
                    RatingCount = 16,
                    AverageRating = 4.7m,
                    JudgeScore = 91m,
                    JudgeFeedback = "Strong control and expression across the final performance.",
                    JudgeSuggestions = "Keep working on stage presence for live settings.",
                    Score = 208.2m,
                    IsApproved = true,
                    ApprovalStatus = "Approved",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    TalentShowVideoId = -2,
                    TalentShowCampaignId = -1,
                    CampaignName = "Christmas Talent Fest",
                    EventName = "Christmas",
                    UploadStartDate = DateTime.UtcNow.AddDays(-7),
                    UploadEndDate = DateTime.UtcNow.AddDays(21),
                    TalentShowCategoryId = -1,
                    CategoryName = "Music",
                    AgeGroup = "18+",
                    ChallengeNumber = 2,
                    ChallengeLevel = "Intermediate",
                    MemberName = "Ravi K.",
                    Title = "Keyboard Melody Demo",
                    Description = "Demo music performance with live likes and ratings.",
                    VideoPath = "https://interactive-examples.mdn.mozilla.net/media/cc0-videos/flower.mp4",
                    LikeCount = 34,
                    RatingCount = 12,
                    AverageRating = 4.4m,
                    JudgeScore = 84m,
                    JudgeFeedback = "Good rhythm and clean transitions.",
                    JudgeSuggestions = "Add a stronger signature ending for challenge 3.",
                    Score = 170.8m,
                    IsApproved = true,
                    ApprovalStatus = "Approved",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    TalentShowVideoId = -3,
                    TalentShowCampaignId = -1,
                    CampaignName = "Christmas Talent Fest",
                    EventName = "Christmas",
                    UploadStartDate = DateTime.UtcNow.AddDays(-7),
                    UploadEndDate = DateTime.UtcNow.AddDays(21),
                    TalentShowCategoryId = -3,
                    CategoryName = "Preaching",
                    AgeGroup = "18+",
                    ChallengeNumber = 1,
                    ChallengeLevel = "Beginner",
                    MemberName = "Pastor Daniel",
                    Title = "Short Message Demo",
                    Description = "Sample preaching category video for preview before database setup.",
                    VideoPath = "https://www.w3schools.com/html/movie.mp4",
                    LikeCount = 27,
                    RatingCount = 10,
                    AverageRating = 4.6m,
                    JudgeScore = 88m,
                    JudgeFeedback = "Clear message with confident delivery.",
                    JudgeSuggestions = "Use tighter timing for the next level.",
                    Score = 161.0m,
                    IsApproved = true,
                    ApprovalStatus = "Approved",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-1)
                }
            };
        }

        private static bool Contains(string? source, string value)
        {
            return source?.Contains(value, StringComparison.OrdinalIgnoreCase) == true;
        }

        private static BaseResponse SaveDemoReaction(TalentShowReactionRequest request)
        {
            lock (DemoLock)
            {
                if (!DemoSummaries.TryGetValue(request.TalentShowVideoId, out var summary))
                {
                    return new BaseResponse
                    {
                        ResultId = 0,
                        ResultMessage = "Demo video not found."
                    };
                }

                if (request.IsLike == true)
                    summary.LikeCount += 1;

                if (request.Rating.HasValue)
                {
                    var ratingTotal = summary.AverageRating * summary.RatingCount;
                    summary.RatingCount += 1;
                    summary.AverageRating = Math.Round((ratingTotal + request.Rating.Value) / summary.RatingCount, 2);
                }

                summary.Score = summary.JudgeScore + summary.LikeCount + (summary.AverageRating * summary.RatingCount);
            }

            return new BaseResponse
            {
                ResultId = (int)request.TalentShowVideoId,
                ResultMessage = "Demo reaction saved successfully."
            };
        }

        private static TalentShowVideoSummary? GetDemoSummary(long talentShowVideoId)
        {
            lock (DemoLock)
            {
                if (!DemoSummaries.TryGetValue(talentShowVideoId, out var summary))
                    return null;

                return new TalentShowVideoSummary
                {
                    TalentShowVideoId = summary.TalentShowVideoId,
                    LikeCount = summary.LikeCount,
                    RatingCount = summary.RatingCount,
                    AverageRating = summary.AverageRating,
                    JudgeScore = summary.JudgeScore,
                    Score = summary.Score
                };
            }
        }

        private static TalentShowRegistrationStatus? GetDemoRegistrationStatus(int talentShowCampaignId, long? memberId)
        {
            var campaign = GetDemoCampaigns(true, false)
                .FirstOrDefault(item => item.TalentShowCampaignId == talentShowCampaignId);
            var category = GetDemoCategories(true)
                .FirstOrDefault(item => item.TalentShowCategoryId == -1);

            if (campaign == null || category == null)
                return null;

            var videos = CreateDemoVideos()
                .Where(video => video.TalentShowCampaignId == talentShowCampaignId)
                .OrderBy(video => video.ChallengeNumber)
                .ToList();

            var challenges = Enumerable.Range(1, 3)
                .Select(number =>
                {
                    var video = videos.FirstOrDefault(item => item.ChallengeNumber == number);
                    return new TalentShowChallengeStatus
                    {
                        ChallengeNumber = number,
                        ChallengeLevel = number switch
                        {
                            1 => "Beginner",
                            2 => "Intermediate",
                            _ => "Advanced / Signature Performance"
                        },
                        IsUnlocked = number == 1 || videos.Any(item => item.ChallengeNumber == number - 1 && item.IsApproved),
                        IsUploaded = video != null,
                        TalentShowVideoId = video?.TalentShowVideoId,
                        Title = video?.Title,
                        ApprovalStatus = video?.ApprovalStatus,
                        AverageRating = video?.AverageRating ?? 0,
                        RatingCount = video?.RatingCount ?? 0,
                        LikeCount = video?.LikeCount ?? 0,
                        JudgeScore = video?.JudgeScore ?? 0,
                        JudgeFeedback = video?.JudgeFeedback,
                        JudgeSuggestions = video?.JudgeSuggestions
                    };
                })
                .ToList();

            return new TalentShowRegistrationStatus
            {
                TalentShowRegistrationId = -1,
                TalentShowCampaignId = campaign.TalentShowCampaignId,
                CampaignName = campaign.CampaignName,
                TalentShowCategoryId = category.TalentShowCategoryId,
                CategoryName = category.CategoryName,
                MemberId = memberId,
                MemberName = "Demo Member",
                AgeGroup = "18+",
                AcceptedTerms = true,
                CurrentChallengeNumber = challenges.FirstOrDefault(challenge => challenge.IsUnlocked && !challenge.IsUploaded)?.ChallengeNumber ?? 3,
                CompletedChallenges = challenges.Count(challenge => challenge.IsUploaded),
                PortfolioCompleted = challenges.All(challenge => challenge.IsUploaded),
                IsCampaignEnded = DateTime.UtcNow > campaign.UploadEndDate,
                RegisteredDate = DateTime.UtcNow.AddDays(-6),
                Challenges = challenges
            };
        }

        private static List<TalentShowRanking> GetDemoRankings(int? talentShowCampaignId, int? talentShowCategoryId, string? ageGroup)
        {
            var rankings = new List<TalentShowRanking>
            {
                new()
                {
                    RankNumber = 1,
                    TalentShowCampaignId = -2,
                    CampaignName = "New Year Showcase",
                    TalentShowCategoryId = -2,
                    CategoryName = "Singing",
                    AgeGroup = "13-17",
                    MemberId = -101,
                    MemberName = "Ananya R.",
                    JudgeScore = 91m,
                    ViewerRating = 4.7m,
                    EngagementScore = 58m,
                    ConsistencyScore = 100m,
                    FinalScore = 76.4m,
                    CompletedChallenges = 3
                },
                new()
                {
                    RankNumber = 1,
                    TalentShowCampaignId = -1,
                    CampaignName = "Christmas Talent Fest",
                    TalentShowCategoryId = -1,
                    CategoryName = "Music",
                    AgeGroup = "18+",
                    MemberId = -102,
                    MemberName = "Ravi K.",
                    JudgeScore = 84m,
                    ViewerRating = 4.4m,
                    EngagementScore = 46m,
                    ConsistencyScore = 66.67m,
                    FinalScore = 65.6m,
                    CompletedChallenges = 2
                },
                new()
                {
                    RankNumber = 1,
                    TalentShowCampaignId = -1,
                    CampaignName = "Christmas Talent Fest",
                    TalentShowCategoryId = -3,
                    CategoryName = "Preaching",
                    AgeGroup = "18+",
                    MemberId = -103,
                    MemberName = "Pastor Daniel",
                    JudgeScore = 88m,
                    ViewerRating = 4.6m,
                    EngagementScore = 37m,
                    ConsistencyScore = 33.33m,
                    FinalScore = 61.65m,
                    CompletedChallenges = 1
                }
            };

            if (talentShowCampaignId.HasValue)
                rankings = rankings.Where(item => item.TalentShowCampaignId == talentShowCampaignId.Value).ToList();

            if (talentShowCategoryId.HasValue)
                rankings = rankings.Where(item => item.TalentShowCategoryId == talentShowCategoryId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(ageGroup))
                rankings = rankings
                    .Where(item => string.Equals(item.AgeGroup, ageGroup.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

            return rankings
                .OrderBy(item => item.CategoryName)
                .ThenBy(item => item.AgeGroup)
                .ThenBy(item => item.RankNumber)
                .ToList();
        }

        private static (bool IsApproved, string ApprovalStatus)? GetDemoApproval(long talentShowVideoId)
        {
            lock (DemoLock)
            {
                if (!DemoApprovals.TryGetValue(talentShowVideoId, out var approval))
                    return null;

                return approval;
            }
        }
    }
}
