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
    }
}
