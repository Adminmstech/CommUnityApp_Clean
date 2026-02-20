-- =============================================
-- Author:      Antigravity
-- Create date: 2026-02-20
-- Description: Stored Procedures for Business Area (using Businesses table)
-- =============================================

USE [CommUnityApp_DB] -- Adjust database name if necessary
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 0. Table Definitions (If not existing)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Businesses')
BEGIN
    CREATE TABLE [dbo].[Businesses](
        [BusinessId] [int] IDENTITY(1,1) NOT NULL,
        [CategoryId] [int] NOT NULL,
        [BusinessName] [nvarchar](150) NOT NULL,
        [BusinessNumber] [nvarchar](100) NULL,
        [UserId] [uniqueidentifier] NULL,
        [OwnerName] [nvarchar](100) NULL,
        [Email] [nvarchar](150) NULL,
        [Phone] [nvarchar](20) NULL,
        [Address] [nvarchar](300) NULL,
        [City] [nvarchar](100) NULL,
        [State] [nvarchar](100) NULL,
        [Country] [nvarchar](50) NULL,
        [Suburb] [nvarchar](500) NULL,
        [Info] [nvarchar](max) NULL,
        [Logo] [nvarchar](500) NULL,
        [Latitude] [decimal](10, 7) NULL,
        [Longitude] [decimal](10, 7) NULL,
        [IsVerified] [bit] NULL DEFAULT ((0)),
        [IsActive] [bit] NULL DEFAULT ((1)),
        [CreatedAt] [datetime] NULL DEFAULT (getdate()),
        [Password] [nvarchar](300) NULL,
        CONSTRAINT [PK_Businesses] PRIMARY KEY CLUSTERED ([BusinessId] ASC)
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BrandGame')
BEGIN
    CREATE TABLE BrandGame (
        BrandGameID INT PRIMARY KEY IDENTITY(1,1),
        BrandGame NVARCHAR(255) NOT NULL,
        BrandGameTitle NVARCHAR(255),
        BrandGameImage NVARCHAR(MAX),
        UserGroupId INT,
        BusinessId INT,
        BrandGameDesc NVARCHAR(MAX),
        ConditionsApply NVARCHAR(MAX),
        GameClassificationID INT,
        DateStart DATETIME,
        DateEnd DATETIME,
        PanelCount INT DEFAULT 1,
        PanelOpeningLimit INT DEFAULT 1,
        ChanceCount INT DEFAULT 1,
        DateEntered DATETIME DEFAULT GETDATE(),
        DestinationUrl NVARCHAR(MAX),
        Status INT DEFAULT 1,
        PrimaryWinImageId INT,
        SecondaryWinImageId INT,
        ConsolationImageId INT,
        ScratchCoverImageId INT,
        QRImagePath NVARCHAR(MAX),
        LimitCount INT,
        PrimaryOfferText NVARCHAR(MAX),
        OfferText NVARCHAR(MAX),
        PrimaryWinMessage NVARCHAR(MAX),
        SecondaryWinMessage NVARCHAR(MAX),
        ConsolationMessage NVARCHAR(MAX),
        PointsAwarded INT DEFAULT 0,
        PermitNumber NVARCHAR(100),
        ClassNumber NVARCHAR(100),
        FormColor NVARCHAR(50),
        TextColor NVARCHAR(50),
        PromotionalCode NVARCHAR(100),
        PrimaryPrizeCount INT DEFAULT 0,
        SecondaryPrizeCount INT DEFAULT 0,
        ConsolationPrizeCount INT DEFAULT 0,
        TotalEntries INT DEFAULT 0,
        PrimaryPrizeBalCount INT DEFAULT 0,
        SecondaryPrizeBalCount INT DEFAULT 0,
        ConsolationPrizeBalCount INT DEFAULT 0,
        TotalBalCount INT DEFAULT 0,
        PrimaryPrizePromotionId BIGINT DEFAULT 0,
        SecondaryPrizePromotionId BIGINT DEFAULT 0,
        ConsolationPrizePromotionId BIGINT DEFAULT 0,
        QRScanCount INT DEFAULT 0,
        SMSCount INT DEFAULT 0,
        IsPayment INT DEFAULT 0,
        PaymentAmount DECIMAL(18,2) DEFAULT 0,
        UnSuccessfulImage NVARCHAR(MAX),
        ExpiryText NVARCHAR(MAX),
        OnceIn INT DEFAULT 1,
        IsReleased INT DEFAULT 0,
        IsPrizeClosed INT DEFAULT 0,
        ReferaFriend INT DEFAULT 0,
        CurrentInterval NVARCHAR(100),
        IntervalId INT,
        CustomTagIds NVARCHAR(MAX),
        GroupId BIGINT,
        QRlinkedId BIGINT,
        IsArchive INT DEFAULT 0,
        CONSTRAINT FK_BrandGame_Businesses FOREIGN KEY (BusinessId) REFERENCES Businesses(BusinessId)
    );
END
ELSE
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'MerchantID' AND Object_ID = Object_ID(N'BrandGame'))
    BEGIN
        EXEC sp_rename 'BrandGame.MerchantID', 'BusinessId', 'COLUMN';
    END
END
GO

-- 1. sp_BusinessLogin
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_BusinessLogin')
    DROP PROCEDURE sp_BusinessLogin
GO

CREATE PROCEDURE [dbo].[sp_BusinessLogin]
    @Email NVARCHAR(150),
    @Password NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        BusinessId,
        BusinessName,
        Logo,
        Email
    FROM Businesses
    WHERE Email = @Email 
      AND Password = @Password 
      AND IsActive = 1;
END
GO

-- 2. sp_BusinessRegister
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_BusinessRegister')
    DROP PROCEDURE sp_BusinessRegister
GO

CREATE PROCEDURE [dbo].[sp_BusinessRegister]
    @CategoryId INT,
    @BusinessName NVARCHAR(150),
    @BusinessNumber NVARCHAR(100) = NULL,
    @OwnerName NVARCHAR(100) = NULL,
    @Email NVARCHAR(150),
    @Phone NVARCHAR(20) = NULL,
    @Address NVARCHAR(300) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Country NVARCHAR(50) = NULL,
    @Suburb NVARCHAR(500) = NULL,
    @Info NVARCHAR(MAX) = NULL,
    @Logo NVARCHAR(500) = NULL,
    @Password NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Businesses WHERE Email = @Email)
    BEGIN
        SELECT 0 AS ResultId, 'Email already registered.' AS ResultMessage;
        RETURN;
    END

    INSERT INTO Businesses (
        CategoryId, BusinessName, BusinessNumber, OwnerName, Email, 
        Phone, Address, City, State, Country, Suburb, Info, Logo, 
        Password, IsActive, CreatedAt
    )
    VALUES (
        @CategoryId, @BusinessName, @BusinessNumber, @OwnerName, @Email, 
        @Phone, @Address, @City, @State, @Country, @Suburb, @Info, @Logo, 
        @Password, 1, GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS ResultId, 'Business registered successfully.' AS ResultMessage;
END
GO

-- 3. sp_AddUpdateBrandGame
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_AddUpdateBrandGame')
    DROP PROCEDURE sp_AddUpdateBrandGame
GO

CREATE PROCEDURE [dbo].[sp_AddUpdateBrandGame]
    @BrandGameID INT = 0,
    @BrandGame NVARCHAR(255),
    @BrandGameTitle NVARCHAR(255) = NULL,
    @BrandGameImage NVARCHAR(MAX) = NULL,
    @UserGroupId INT = NULL,
    @BusinessId INT = NULL,
    @BrandGameDesc NVARCHAR(MAX) = NULL,
    @ConditionsApply NVARCHAR(MAX) = NULL,
    @GameClassificationID INT = NULL,
    @DateStart DATETIME = NULL,
    @DateEnd DATETIME = NULL,
    @PanelCount INT = 1,
    @PanelOpeningLimit INT = 1,
    @ChanceCount INT = 1,
    @DestinationUrl NVARCHAR(MAX) = NULL,
    @Status INT = 1,
    @PrimaryWinImageId INT = NULL,
    @SecondaryWinImageId INT = NULL,
    @ConsolationImageId INT = NULL,
    @ScratchCoverImageId INT = NULL,
    @QRImagePath NVARCHAR(MAX) = NULL,
    @LimitCount INT = NULL,
    @PrimaryOfferText NVARCHAR(MAX) = NULL,
    @OfferText NVARCHAR(MAX) = NULL,
    @PrimaryWinMessage NVARCHAR(MAX) = NULL,
    @SecondaryWinMessage NVARCHAR(MAX) = NULL,
    @ConsolationMessage NVARCHAR(MAX) = NULL,
    @PointsAwarded INT = 0,
    @PermitNumber NVARCHAR(100) = NULL,
    @ClassNumber NVARCHAR(100) = NULL,
    @FormColor NVARCHAR(50) = NULL,
    @TextColor NVARCHAR(50) = NULL,
    @PromotionalCode NVARCHAR(100) = NULL,
    @PrimaryPrizeCount INT = 0,
    @SecondaryPrizeCount INT = 0,
    @ConsolationPrizeCount INT = 0,
    @TotalEntries INT = 0,
    @PrimaryPrizePromotionId BIGINT = 0,
    @SecondaryPrizePromotionId BIGINT = 0,
    @ConsolationPrizePromotionId BIGINT = 0,
    @IsPayment INT = 0,
    @PaymentAmount DECIMAL(18,2) = 0,
    @UnSuccessfulImage NVARCHAR(MAX) = NULL,
    @ExpiryText NVARCHAR(MAX) = NULL,
    @OnceIn INT = 1,
    @IsReleased INT = 0,
    @ReferaFriend INT = 0,
    @CurrentInterval NVARCHAR(100) = NULL,
    @IntervalId INT = NULL,
    @CustomTagIds NVARCHAR(MAX) = NULL,
    @GroupId BIGINT = NULL,
    @QRlinkedId BIGINT = NULL,
    @IsArchive INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @BrandGameID = 0
    BEGIN
        INSERT INTO [dbo].[BrandGame] (
            [BrandGame], [BrandGameTitle], [BrandGameImage], [UserGroupId], [BusinessId], 
            [BrandGameDesc], [ConditionsApply], [GameClassificationID], [DateStart], [DateEnd], 
            [PanelCount], [PanelOpeningLimit], [ChanceCount], [DateEntered], [DestinationUrl], 
            [Status], [PrimaryWinImageId], [SecondaryWinImageId], [ConsolationImageId], [ScratchCoverImageId], 
            [QRImagePath], [LimitCount], [PrimaryOfferText], [OfferText], [PrimaryWinMessage], 
            [SecondaryWinMessage], [ConsolationMessage], [PointsAwarded], [PermitNumber], [ClassNumber], 
            [FormColor], [TextColor], [PromotionalCode], [PrimaryPrizeCount], [SecondaryPrizeCount], 
            [ConsolationPrizeCount], [TotalEntries], [PrimaryPrizePromotionId], [SecondaryPrizePromotionId], [ConsolationPrizePromotionId], 
            [IsPayment], [PaymentAmount], [UnSuccessfulImage], [ExpiryText], [OnceIn], 
            [IsReleased], [ReferaFriend], [CurrentInterval], [IntervalId], [CustomTagIds], 
            [GroupId], [QRlinkedId], [IsArchive]
        )
        VALUES (
            @BrandGame, @BrandGameTitle, @BrandGameImage, @UserGroupId, @BusinessId, 
            @BrandGameDesc, @ConditionsApply, @GameClassificationID, @DateStart, @DateEnd, 
            @PanelCount, @PanelOpeningLimit, @ChanceCount, GETDATE(), @DestinationUrl, 
            @Status, @PrimaryWinImageId, @SecondaryWinImageId, @ConsolationImageId, @ScratchCoverImageId, 
            @QRImagePath, @LimitCount, @PrimaryOfferText, @OfferText, @PrimaryWinMessage, 
            @SecondaryWinMessage, @ConsolationMessage, @PointsAwarded, @PermitNumber, @ClassNumber, 
            @FormColor, @TextColor, @PromotionalCode, @PrimaryPrizeCount, @SecondaryPrizeCount, 
            @ConsolationPrizeCount, @TotalEntries, @PrimaryPrizePromotionId, @SecondaryPrizePromotionId, @ConsolationPrizePromotionId, 
            @IsPayment, @PaymentAmount, @UnSuccessfulImage, @ExpiryText, @OnceIn, 
            @IsReleased, @ReferaFriend, @CurrentInterval, @IntervalId, @CustomTagIds, 
            @GroupId, @QRlinkedId, @IsArchive
        );

        SELECT SCOPE_IDENTITY() AS ResultId, 'Brand Game created successfully.' AS ResultMessage;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[BrandGame]
        SET [BrandGame] = @BrandGame,
            [BrandGameTitle] = @BrandGameTitle,
            [BrandGameImage] = CASE WHEN @BrandGameImage IS NOT NULL THEN @BrandGameImage ELSE [BrandGameImage] END,
            [UserGroupId] = @UserGroupId,
            [BusinessId] = @BusinessId,
            [BrandGameDesc] = @BrandGameDesc,
            [ConditionsApply] = @ConditionsApply,
            [GameClassificationID] = @GameClassificationID,
            [DateStart] = @DateStart,
            [DateEnd] = @DateEnd,
            [PanelCount] = @PanelCount,
            [PanelOpeningLimit] = @PanelOpeningLimit,
            [ChanceCount] = @ChanceCount,
            [DestinationUrl] = @DestinationUrl,
            [Status] = @Status,
            [PrimaryWinImageId] = @PrimaryWinImageId,
            [SecondaryWinImageId] = @SecondaryWinImageId,
            [ConsolationImageId] = @ConsolationImageId,
            [ScratchCoverImageId] = @ScratchCoverImageId,
            [QRImagePath] = @QRImagePath,
            [LimitCount] = @LimitCount,
            [PrimaryOfferText] = @PrimaryOfferText,
            [OfferText] = @OfferText,
            [PrimaryWinMessage] = @PrimaryWinMessage,
            [SecondaryWinMessage] = @SecondaryWinMessage,
            [ConsolationMessage] = @ConsolationMessage,
            [PointsAwarded] = @PointsAwarded,
            [PermitNumber] = @PermitNumber,
            [ClassNumber] = @ClassNumber,
            [FormColor] = @FormColor,
            [TextColor] = @TextColor,
            [PromotionalCode] = @PromotionalCode,
            [PrimaryPrizeCount] = @PrimaryPrizeCount,
            [SecondaryPrizeCount] = @SecondaryPrizeCount,
            [ConsolationPrizeCount] = @ConsolationPrizeCount,
            [TotalEntries] = @TotalEntries,
            [PrimaryPrizePromotionId] = @PrimaryPrizePromotionId,
            [SecondaryPrizePromotionId] = @SecondaryPrizePromotionId,
            [ConsolationPrizePromotionId] = @ConsolationPrizePromotionId,
            [IsPayment] = @IsPayment,
            [PaymentAmount] = @PaymentAmount,
            [UnSuccessfulImage] = CASE WHEN @UnSuccessfulImage IS NOT NULL THEN @UnSuccessfulImage ELSE [UnSuccessfulImage] END,
            [ExpiryText] = @ExpiryText,
            [OnceIn] = @OnceIn,
            [IsReleased] = @IsReleased,
            [ReferaFriend] = @ReferaFriend,
            [CurrentInterval] = @CurrentInterval,
            [IntervalId] = @IntervalId,
            [CustomTagIds] = @CustomTagIds,
            [GroupId] = @GroupId,
            [QRlinkedId] = @QRlinkedId,
            [IsArchive] = @IsArchive
        WHERE BrandGameID = @BrandGameID;

        SELECT @BrandGameID AS ResultId, 'Brand Game updated successfully.' AS ResultMessage;
    END
END
GO
