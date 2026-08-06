IF OBJECT_ID('dbo.TalentShowCampaign', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TalentShowCampaign
    (
        TalentShowCampaignId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CampaignName NVARCHAR(150) NOT NULL,
        EventName NVARCHAR(150) NULL,
        Description NVARCHAR(1000) NULL,
        Guidelines NVARCHAR(2000) NULL,
        TermsAndConditions NVARCHAR(2000) NULL,
        DurationDays INT NOT NULL CONSTRAINT DF_TalentShowCampaign_DurationDays DEFAULT (30),
        ChallengeCount INT NOT NULL CONSTRAINT DF_TalentShowCampaign_ChallengeCount DEFAULT (3),
        DaysPerChallenge INT NOT NULL CONSTRAINT DF_TalentShowCampaign_DaysPerChallenge DEFAULT (7),
        UploadStartDate DATETIME2 NOT NULL,
        UploadEndDate DATETIME2 NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_TalentShowCampaign_IsActive DEFAULT (1),
        DisplayOrder INT NOT NULL CONSTRAINT DF_TalentShowCampaign_DisplayOrder DEFAULT (0),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TalentShowCampaign_CreatedDate DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END;

IF COL_LENGTH('dbo.TalentShowCampaign', 'Guidelines') IS NULL
    ALTER TABLE dbo.TalentShowCampaign ADD Guidelines NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.TalentShowCampaign', 'TermsAndConditions') IS NULL
    ALTER TABLE dbo.TalentShowCampaign ADD TermsAndConditions NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.TalentShowCampaign', 'DurationDays') IS NULL
    ALTER TABLE dbo.TalentShowCampaign ADD DurationDays INT NOT NULL CONSTRAINT DF_TalentShowCampaign_DurationDays DEFAULT (30);

IF COL_LENGTH('dbo.TalentShowCampaign', 'ChallengeCount') IS NULL
    ALTER TABLE dbo.TalentShowCampaign ADD ChallengeCount INT NOT NULL CONSTRAINT DF_TalentShowCampaign_ChallengeCount DEFAULT (3);

IF COL_LENGTH('dbo.TalentShowCampaign', 'DaysPerChallenge') IS NULL
    ALTER TABLE dbo.TalentShowCampaign ADD DaysPerChallenge INT NOT NULL CONSTRAINT DF_TalentShowCampaign_DaysPerChallenge DEFAULT (7);

IF OBJECT_ID('dbo.TalentShowCategory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TalentShowCategory
    (
        TalentShowCategoryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_TalentShowCategory_IsActive DEFAULT (1),
        DisplayOrder INT NOT NULL CONSTRAINT DF_TalentShowCategory_DisplayOrder DEFAULT (0),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TalentShowCategory_CreatedDate DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END;

IF OBJECT_ID('dbo.TalentShowVideo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TalentShowVideo
    (
        TalentShowVideoId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TalentShowCampaignId INT NOT NULL,
        TalentShowCategoryId INT NOT NULL,
        MemberId BIGINT NULL,
        MemberName NVARCHAR(150) NULL,
        Title NVARCHAR(160) NOT NULL,
        Description NVARCHAR(1000) NULL,
        VideoPath NVARCHAR(500) NOT NULL,
        ThumbnailPath NVARCHAR(500) NULL,
        ChallengeNumber INT NOT NULL CONSTRAINT DF_TalentShowVideo_ChallengeNumber DEFAULT (1),
        ChallengeLevel NVARCHAR(80) NOT NULL CONSTRAINT DF_TalentShowVideo_ChallengeLevel DEFAULT ('Beginner'),
        AgeGroup NVARCHAR(40) NULL,
        JudgeScore DECIMAL(8,2) NOT NULL CONSTRAINT DF_TalentShowVideo_JudgeScore DEFAULT (0),
        JudgeFeedback NVARCHAR(1000) NULL,
        JudgeSuggestions NVARCHAR(1000) NULL,
        IsApproved BIT NOT NULL CONSTRAINT DF_TalentShowVideo_IsApproved DEFAULT (0),
        ApprovalStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_TalentShowVideo_ApprovalStatus DEFAULT ('Pending'),
        IsActive BIT NOT NULL CONSTRAINT DF_TalentShowVideo_IsActive DEFAULT (1),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TalentShowVideo_CreatedDate DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END;

IF COL_LENGTH('dbo.TalentShowVideo', 'ChallengeNumber') IS NULL
    ALTER TABLE dbo.TalentShowVideo ADD ChallengeNumber INT NOT NULL CONSTRAINT DF_TalentShowVideo_ChallengeNumber DEFAULT (1);

IF COL_LENGTH('dbo.TalentShowVideo', 'ChallengeLevel') IS NULL
    ALTER TABLE dbo.TalentShowVideo ADD ChallengeLevel NVARCHAR(80) NOT NULL CONSTRAINT DF_TalentShowVideo_ChallengeLevel DEFAULT ('Beginner');

IF COL_LENGTH('dbo.TalentShowVideo', 'AgeGroup') IS NULL
    ALTER TABLE dbo.TalentShowVideo ADD AgeGroup NVARCHAR(40) NULL;

IF COL_LENGTH('dbo.TalentShowVideo', 'JudgeScore') IS NULL
    ALTER TABLE dbo.TalentShowVideo ADD JudgeScore DECIMAL(8,2) NOT NULL CONSTRAINT DF_TalentShowVideo_JudgeScore DEFAULT (0);

IF COL_LENGTH('dbo.TalentShowVideo', 'JudgeFeedback') IS NULL
    ALTER TABLE dbo.TalentShowVideo ADD JudgeFeedback NVARCHAR(1000) NULL;

IF COL_LENGTH('dbo.TalentShowVideo', 'JudgeSuggestions') IS NULL
    ALTER TABLE dbo.TalentShowVideo ADD JudgeSuggestions NVARCHAR(1000) NULL;

IF OBJECT_ID('dbo.TalentShowReaction', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TalentShowReaction
    (
        TalentShowReactionId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TalentShowVideoId BIGINT NOT NULL,
        MemberId BIGINT NULL,
        VisitorKey NVARCHAR(120) NULL,
        VoterKey NVARCHAR(140) NOT NULL,
        IsLiked BIT NOT NULL CONSTRAINT DF_TalentShowReaction_IsLiked DEFAULT (0),
        Rating TINYINT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TalentShowReaction_CreatedDate DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END;

IF OBJECT_ID('dbo.TalentShowRegistration', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TalentShowRegistration
    (
        TalentShowRegistrationId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TalentShowCampaignId INT NOT NULL,
        TalentShowCategoryId INT NOT NULL,
        MemberId BIGINT NOT NULL,
        MemberName NVARCHAR(150) NULL,
        AgeGroup NVARCHAR(40) NOT NULL,
        AcceptedTerms BIT NOT NULL CONSTRAINT DF_TalentShowRegistration_AcceptedTerms DEFAULT (0),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TalentShowRegistration_CreatedDate DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.TalentShowRegistration')
      AND name = 'UX_TalentShowRegistration_Campaign_Member'
)
BEGIN
    CREATE UNIQUE INDEX UX_TalentShowRegistration_Campaign_Member
    ON dbo.TalentShowRegistration (TalentShowCampaignId, MemberId);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.TalentShowReaction')
      AND name = 'UX_TalentShowReaction_Voter'
)
BEGIN
    CREATE UNIQUE INDEX UX_TalentShowReaction_Voter
    ON dbo.TalentShowReaction (TalentShowVideoId, VoterKey);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.TalentShowVideo')
      AND name = 'IX_TalentShowVideo_Ranking'
)
BEGIN
    CREATE INDEX IX_TalentShowVideo_Ranking
    ON dbo.TalentShowVideo (TalentShowCampaignId, TalentShowCategoryId, AgeGroup, ChallengeNumber, IsApproved, IsActive);
END;
