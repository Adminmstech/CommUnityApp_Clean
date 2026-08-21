-- =========================================================================
-- Sample Data / Setup Script: Scratch & Win Rewards Game (GameClassificationID = 1)
-- Run this script to populate a default Scratch & Win Game for BusinessId = 1
-- or for all active businesses that do not have one configured.
-- =========================================================================

USE [CommUnityApp_DB]
GO

-- Declare variables for the sample insert
DECLARE @BusinessId INT = 1; -- Update this with your business ID

-- =========================================================================
-- OPTION 1: Insert default Scratch & Win Rewards Game for Business ID = 1
-- =========================================================================
IF EXISTS (SELECT 1 FROM Businesses WHERE BusinessId = @BusinessId)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM BrandGame WHERE BusinessId = @BusinessId AND BrandGame LIKE 'Scratch & Win Rewards%')
    BEGIN
        INSERT INTO BrandGame (
            BrandGame,
            BrandGameTitle,
            BrandGameImage,
            UserGroupId,
            BusinessId,
            BrandGameDesc,
            ConditionsApply,
            GameClassificationID,
            DateStart,
            DateEnd,
            PanelCount,
            PanelOpeningLimit,
            ChanceCount,
            DestinationUrl,
            Status,
            PrimaryOfferText,
            OfferText,
            PrimaryWinMessage,
            SecondaryWinMessage,
            ConsolationMessage,
            PointsAwarded,
            FormColor,
            TextColor,
            PrimaryPrizeCount,
            SecondaryPrizeCount,
            ConsolationPrizeCount,
            TotalEntries,
            PrimaryPrizeBalCount,
            SecondaryPrizeBalCount,
            ConsolationPrizeBalCount,
            OnceIn,
            IsReleased,
            IsPrizeClosed,
            IsArchive
        )
        VALUES (
            'Scratch & Win Rewards',                                                   -- BrandGame (Name)
            'Scratch & Win Exciting Rewards!',                                         -- BrandGameTitle
            NULL,                                                                      -- BrandGameImage
            NULL,                                                                      -- UserGroupId
            @BusinessId,                                                               -- BusinessId
            'Scratch the card to reveal your reward! Win Free Coffee, Burgers, Mystery Gifts, or IndoCoins!', -- BrandGameDesc
            'Terms and conditions apply. One play per customer per day. Must be 18+.',  -- ConditionsApply
            1,                                                                         -- GameClassificationID (1 = Scratch & Win)
            GETDATE(),                                                                 -- DateStart
            DATEADD(MONTH, 3, GETDATE()),                                              -- DateEnd (3 months from now)
            3,                                                                         -- PanelCount
            1,                                                                         -- PanelOpeningLimit
            3,                                                                         -- ChanceCount (attempts per day)
            NULL,                                                                      -- DestinationUrl
            1,                                                                         -- Status (Active)
            'Jackpot - 200 IndoCoins',                                                 -- PrimaryOfferText
            '25 IndoCoins',                                                            -- OfferText (Secondary)
            'Congratulations! You won the Jackpot of 200 IndoCoins!',                  -- PrimaryWinMessage
            'Great job! You won 25 IndoCoins!',                                        -- SecondaryWinMessage
            'Almost There! Earned 5 IC',                                               -- ConsolationMessage
            5,                                                                         -- PointsAwarded (5 IC for consolation)
            '#F6AA31',                                                                 -- FormColor (Orange/Gold)
            '#FFFFFF',                                                                 -- TextColor
            10,                                                                        -- PrimaryPrizeCount
            200,                                                                       -- SecondaryPrizeCount
            400,                                                                       -- ConsolationPrizeCount
            0,                                                                         -- TotalEntries
            10,                                                                        -- PrimaryPrizeBalCount
            200,                                                                       -- SecondaryPrizeBalCount
            400,                                                                       -- ConsolationPrizeBalCount
            1,                                                                         -- OnceIn
            1,                                                                         -- IsReleased (Live)
            0,                                                                         -- IsPrizeClosed
            0                                                                          -- IsArchive
        );

        PRINT 'Sample Scratch & Win Rewards game created successfully for Business ID = 1.';
    END
    ELSE
    BEGIN
        PRINT 'Scratch & Win Rewards game already exists for Business ID = 1.';
    END
END
ELSE
BEGIN
    PRINT 'Business ID = 1 not found. Please verify the Businesses table has active records.';
END
GO


-- =========================================================================
-- OPTION 2: Bulk Insert Default Scratch & Win Rewards for ALL Active Businesses
-- =========================================================================
INSERT INTO BrandGame (
    BrandGame,
    BrandGameTitle,
    BrandGameImage,
    UserGroupId,
    BusinessId,
    BrandGameDesc,
    ConditionsApply,
    GameClassificationID,
    DateStart,
    DateEnd,
    PanelCount,
    PanelOpeningLimit,
    ChanceCount,
    DestinationUrl,
    Status,
    PrimaryOfferText,
    OfferText,
    PrimaryWinMessage,
    SecondaryWinMessage,
    ConsolationMessage,
    PointsAwarded,
    FormColor,
    TextColor,
    PrimaryPrizeCount,
    SecondaryPrizeCount,
    ConsolationPrizeCount,
    TotalEntries,
    PrimaryPrizeBalCount,
    SecondaryPrizeBalCount,
    ConsolationPrizeBalCount,
    OnceIn,
    IsReleased,
    IsPrizeClosed,
    IsArchive
)
SELECT 
    'Scratch & Win - ' + b.BusinessName,                                        -- BrandGame (Name)
    'Scratch & Win Exciting Rewards!',                                         -- BrandGameTitle
    NULL,                                                                      -- BrandGameImage
    NULL,                                                                      -- UserGroupId
    b.BusinessId,                                                              -- BusinessId
    'Scratch the card to reveal your reward! Win Free Coffee, Burgers, Mystery Gifts, or IndoCoins!', -- BrandGameDesc
    'Terms and conditions apply. One play per customer per day. Must be 18+.',  -- ConditionsApply
    1,                                                                         -- GameClassificationID (1 = Scratch & Win)
    GETDATE(),                                                                 -- DateStart
    DATEADD(MONTH, 3, GETDATE()),                                              -- DateEnd
    3,                                                                         -- PanelCount
    1,                                                                         -- PanelOpeningLimit
    3,                                                                         -- ChanceCount
    NULL,                                                                      -- DestinationUrl
    1,                                                                         -- Status (Active)
    'Jackpot - 200 IndoCoins',                                                 -- PrimaryOfferText
    '25 IndoCoins',                                                            -- OfferText
    'Congratulations! You won the Jackpot of 200 IndoCoins!',                  -- PrimaryWinMessage
    'Great job! You won 25 IndoCoins!',                                        -- SecondaryWinMessage
    'Almost There! Earned 5 IC',                                               -- ConsolationMessage
    5,                                                                         -- PointsAwarded
    '#F6AA31',                                                                 -- FormColor
    '#FFFFFF',                                                                 -- TextColor
    10,                                                                        -- PrimaryPrizeCount
    200,                                                                       -- SecondaryPrizeCount
    400,                                                                       -- ConsolationPrizeCount
    0,                                                                         -- TotalEntries
    10,                                                                        -- PrimaryPrizeBalCount
    200,                                                                       -- SecondaryPrizeBalCount
    400,                                                                       -- ConsolationPrizeBalCount
    1,                                                                         -- OnceIn
    1,                                                                         -- IsReleased (Live)
    0,                                                                         -- IsPrizeClosed
    0                                                                          -- IsArchive
FROM Businesses b
WHERE b.IsActive = 1
AND NOT EXISTS (
    SELECT 1 FROM BrandGame bg 
    WHERE bg.BusinessId = b.BusinessId 
    AND bg.BrandGame LIKE 'Scratch & Win%'
);

PRINT 'Scratch & Win Rewards games inserted for all remaining active businesses.';
GO
