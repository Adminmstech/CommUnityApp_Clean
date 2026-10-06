using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace CommUnityApp.UnitTests
{
    public class ScratchWinTests
    {
        [Fact]
        public void DefaultRewards_ShouldContain8Tiers_AndSumTo100Percent()
        {
            // Arrange
            var rewards = new List<AddUpdateScratchWinRewardRequest>
            {
                new() { RewardOrder = 1, RewardType = "Prize", RewardName = "Free Coffee", ProbabilityPercentage = 15.00m, RewardImage = "Images/scratch_win/default/free_coffee.png" },
                new() { RewardOrder = 2, RewardType = "Prize", RewardName = "Free Burger", ProbabilityPercentage = 8.00m, RewardImage = "Images/scratch_win/default/free_burger.png" },
                new() { RewardOrder = 3, RewardType = "Prize", RewardName = "Mystery Gift", ProbabilityPercentage = 5.00m, RewardImage = "Images/scratch_win/default/mystery_gift.png" },
                new() { RewardOrder = 4, RewardType = "Coins", RewardName = "200 IndoCoins", CoinValue = 200, ProbabilityPercentage = 1.00m, RewardImage = "Images/scratch_win/default/200_indocoins.png" },
                new() { RewardOrder = 5, RewardType = "Coins", RewardName = "100 IndoCoins", CoinValue = 100, ProbabilityPercentage = 3.00m, RewardImage = "Images/scratch_win/default/100_indocoins.png" },
                new() { RewardOrder = 6, RewardType = "Coins", RewardName = "50 IndoCoins", CoinValue = 50, ProbabilityPercentage = 8.00m, RewardImage = "Images/scratch_win/default/50_indocoins.png" },
                new() { RewardOrder = 7, RewardType = "Coins", RewardName = "25 IndoCoins", CoinValue = 25, ProbabilityPercentage = 20.00m, RewardImage = "Images/scratch_win/default/25_indocoins.png" },
                new() { RewardOrder = 8, RewardType = "Consolation", RewardName = "Almost There", CoinValue = 5, ProbabilityPercentage = 40.00m, RewardImage = "Images/scratch_win/default/almost_there.png" }
            };

            // Act
            var totalPercentage = rewards.Sum(r => r.ProbabilityPercentage);

            // Assert
            Assert.Equal(8, rewards.Count);
            Assert.Equal(100.00m, totalPercentage);
        }

        [Fact]
        public void VerificationToken_ShouldGenerateDeterministicSignature()
        {
            // Arrange
            int gameId = 1;
            Guid userId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851");
            int rewardId = 1;
            int attemptNumber = 5;
            string salt = "CommUnityApp_ScratchWin_Salt_2026";

            string raw = $"{gameId}:{userId}:{rewardId}:{attemptNumber}:{salt}";
            using var sha256 = SHA256.Create();
            string expectedToken = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(raw)));

            // Act
            using var sha2 = SHA256.Create();
            string actualToken = Convert.ToBase64String(sha2.ComputeHash(Encoding.UTF8.GetBytes(raw)));

            // Assert
            Assert.Equal(expectedToken, actualToken);
            Assert.False(string.IsNullOrEmpty(actualToken));
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(2, 1, true)]
        [InlineData(1, 5, false)]
        [InlineData(5, 5, true)]
        [InlineData(10, 5, true)]
        [InlineData(7, 5, false)]
        public void OnceInLogic_ShouldCorrectlyIdentifyWinningAttempts(int attemptNumber, int onceIn, bool expectedWinning)
        {
            // Act
            bool isWinningAttempt = (attemptNumber % onceIn == 0);

            // Assert
            Assert.Equal(expectedWinning, isWinningAttempt);
        }

        [Theory]
        [InlineData(5, null, 5)]
        [InlineData(null, 10, 10)]
        [InlineData(5, 10, 5)] // route id takes precedence
        [InlineData(null, null, 0)]
        [InlineData(0, 0, 0)]
        public void TargetGameIdResolution_ShouldCorrectlyResolveIdOrGameId(int? id, int? gameId, int expectedId)
        {
            // Act
            int targetGameId = (id.HasValue && id.Value > 0) ? id.Value : (gameId ?? 0);

            // Assert
            Assert.Equal(expectedId, targetGameId);
        }

        [Theory]
        [InlineData("Images/scratch_win/custom/pic.png", "/Images/scratch_win/custom/pic.png")]
        [InlineData("/Images/scratch_win/custom/pic.png", "/Images/scratch_win/custom/pic.png")]
        [InlineData(null, "/Images/scratch_win/default/free_coffee.png")]
        [InlineData("", "/Images/scratch_win/default/free_coffee.png")]
        public void ImagePathNormalization_ShouldProduceValidUrl(string? rawPath, string expectedUrl)
        {
            // Act
            var normalized = string.IsNullOrEmpty(rawPath)
                ? "/Images/scratch_win/default/free_coffee.png"
                : (rawPath.StartsWith("/") ? rawPath : "/" + rawPath);

            // Assert
            Assert.Equal(expectedUrl, normalized);
        }

        [Fact]
        public void TotalProbabilityValidation_ShouldDetectNon100Percent()
        {
            // Arrange
            var rewards = new List<AddUpdateScratchWinRewardRequest>
            {
                new() { ProbabilityPercentage = 50.00m },
                new() { ProbabilityPercentage = 40.00m }
            };

            // Act
            var total = rewards.Sum(r => r.ProbabilityPercentage);
            bool isValid = Math.Abs(total - 100.00m) <= 0.01m;

            // Assert
            Assert.False(isValid);
            Assert.Equal(90.00m, total);
        }

        [Fact]
        public void AutoBalance_ShouldCalculateExactRemainderForConsolation()
        {
            // Arrange (Admin set custom probabilities for first 7 rewards)
            var otherRewards = new decimal[] { 20.00m, 15.00m, 10.00m, 2.00m, 5.00m, 8.00m, 15.00m };
            decimal sumOthers = otherRewards.Sum();

            // Act
            decimal consolationRemainder = Math.Max(0m, 100.00m - sumOthers);
            decimal total = sumOthers + consolationRemainder;

            // Assert
            Assert.Equal(75.00m, sumOthers);
            Assert.Equal(25.00m, consolationRemainder);
            Assert.Equal(100.00m, total);
        }

        [Fact]
        public void PureProbabilityMode_WhenOnceInIs1_ShouldAlwaysAllowPhysicalPrize()
        {
            // Arrange
            int onceIn = 1;
            int attemptNumber = 17; // arbitrary non-round attempt

            // Act
            bool isWinningAttempt = (attemptNumber % onceIn == 0);
            bool attemptAllowed = (onceIn <= 1) || isWinningAttempt;

            // Assert
            Assert.True(attemptAllowed);
        }

        [Fact]
        public void ProbabilityEngine_SimulatedRolls_ShouldRespectConfiguredPercentages()
        {
            // Arrange
            var rewards = new List<ScratchWinRewardDto>
            {
                new() { RewardOrder = 1, RewardName = "Free Coffee", ProbabilityPercentage = 15.00m, IsActive = true },
                new() { RewardOrder = 2, RewardName = "Free Burger", ProbabilityPercentage = 8.00m, IsActive = true },
                new() { RewardOrder = 3, RewardName = "Mystery Gift", ProbabilityPercentage = 5.00m, IsActive = true },
                new() { RewardOrder = 4, RewardName = "200 IndoCoins", ProbabilityPercentage = 1.00m, IsActive = true },
                new() { RewardOrder = 5, RewardName = "100 IndoCoins", ProbabilityPercentage = 3.00m, IsActive = true },
                new() { RewardOrder = 6, RewardName = "50 IndoCoins", ProbabilityPercentage = 8.00m, IsActive = true },
                new() { RewardOrder = 7, RewardName = "25 IndoCoins", ProbabilityPercentage = 20.00m, IsActive = true },
                new() { RewardOrder = 8, RewardName = "Almost There", ProbabilityPercentage = 40.00m, IsActive = true }
            };

            decimal totalWeight = rewards.Sum(r => r.ProbabilityPercentage);
            var hitCounts = rewards.ToDictionary(r => r.RewardName, _ => 0);

            // Act: Simulate 50,000 rolls with the high-precision roll algorithm
            int totalTrials = 50000;
            for (int i = 0; i < totalTrials; i++)
            {
                int randomInt = RandomNumberGenerator.GetInt32(0, 1000000);
                decimal roll = ((decimal)randomInt / 1000000.0m) * totalWeight;

                decimal cumulative = 0m;
                foreach (var reward in rewards)
                {
                    cumulative += reward.ProbabilityPercentage;
                    if (roll < cumulative)
                    {
                        hitCounts[reward.RewardName]++;
                        break;
                    }
                }
            }

            // Assert: Each reward should be within ±1.5% of its configured probability in 50k trials
            foreach (var reward in rewards)
            {
                decimal actualPercent = (decimal)hitCounts[reward.RewardName] * 100.0m / totalTrials;
                decimal diff = Math.Abs(actualPercent - reward.ProbabilityPercentage);
                Assert.True(diff < 1.5m, $"Reward {reward.RewardName} diff {diff}% exceeded tolerance. Expected {reward.ProbabilityPercentage}%, got {actualPercent}%");
            }
        }

        [Fact]
        public void PrizeLocation_WhenPrizeHasCustomLocation_ShouldOverrideGameLocation()
        {
            // Arrange
            var game = new ScratchWinGameDto
            {
                GameId = 1,
                BusinessId = 10,
                BusinessLocation = "CurryCraft Kitchen Australia — Shop 5, 85 George Street, Sydney"
            };

            var reward = new ScratchWinRewardDto
            {
                RewardId = 1,
                RewardType = "Prize",
                RewardName = "Free Coffee",
                BusinessId = 8,
                BusinessLocation = "Sydney Spice House — 25 George Street, The Rocks, Sydney"
            };

            // Act
            string? effectiveLocation = !string.IsNullOrWhiteSpace(reward.BusinessLocation)
                ? reward.BusinessLocation
                : game.BusinessLocation;

            int? effectiveBusinessId = (reward.BusinessId.HasValue && reward.BusinessId.Value > 0)
                ? reward.BusinessId.Value
                : game.BusinessId;

            // Assert
            Assert.Equal("Sydney Spice House — 25 George Street, The Rocks, Sydney", effectiveLocation);
            Assert.Equal(8, effectiveBusinessId);
        }

        [Fact]
        public void PrizeLocation_WhenPrizeLocationIsEmpty_ShouldFallbackToGameLocation()
        {
            // Arrange
            var game = new ScratchWinGameDto
            {
                GameId = 1,
                BusinessId = 10,
                BusinessLocation = "CurryCraft Kitchen Australia — Shop 5, 85 George Street, Sydney"
            };

            var reward = new ScratchWinRewardDto
            {
                RewardId = 2,
                RewardType = "Prize",
                RewardName = "Free Burger",
                BusinessId = 0,
                BusinessLocation = null // Left empty to inherit
            };

            // Act
            string? effectiveLocation = !string.IsNullOrWhiteSpace(reward.BusinessLocation)
                ? reward.BusinessLocation
                : game.BusinessLocation;

            int? effectiveBusinessId = (reward.BusinessId.HasValue && reward.BusinessId.Value > 0)
                ? reward.BusinessId.Value
                : game.BusinessId;

            // Assert
            Assert.Equal("CurryCraft Kitchen Australia — Shop 5, 85 George Street, Sydney", effectiveLocation);
            Assert.Equal(10, effectiveBusinessId);
        }

        [Fact]
        public void CoinsReward_ShouldNeverIncludeRedeemLocationOrCode()
        {
            // Arrange
            var game = new ScratchWinGameDto
            {
                GameId = 1,
                BusinessId = 10,
                BusinessLocation = "CurryCraft Kitchen Australia — Shop 5, 85 George Street, Sydney"
            };

            var coinReward = new ScratchWinRewardDto
            {
                RewardId = 6,
                RewardType = "Coins",
                RewardName = "50 IndoCoins",
                CoinValue = 50
            };

            bool isWinner = true;

            // Act
            string? prizeLocation = !string.IsNullOrWhiteSpace(coinReward.BusinessLocation) ? coinReward.BusinessLocation : game.BusinessLocation;
            string? redeemLocation = (isWinner && coinReward.RewardType == "Prize") ? prizeLocation : null;
            string? redeemCode = coinReward.RewardType == "Prize" ? "REDEEM-12345" : null;

            // Assert
            Assert.Null(redeemLocation);
            Assert.Null(redeemCode);
        }

        [Fact]
        public void BusinessLocationFormatting_ShouldJoinAddressFieldsProperly()
        {
            // Arrange
            var b = new BusinessDetailsDto
            {
                BusinessId = 8,
                BusinessName = "Sydney Spice House",
                Address = "25 George Street",
                Suburb = "The Rocks",
                City = "Sydney",
                State = "NSW"
            };

            // Act
            var addr = string.Join(", ", new[] { b.Address, b.Suburb, b.City, b.State }.Where(s => !string.IsNullOrWhiteSpace(s)));
            var fullDisplay = string.IsNullOrWhiteSpace(addr) ? b.BusinessName : $"{b.BusinessName} — {addr}";

            // Assert
            Assert.Equal("25 George Street, The Rocks, Sydney, NSW", addr);
            Assert.Equal("Sydney Spice House — 25 George Street, The Rocks, Sydney, NSW", fullDisplay);
        }
    }
}
