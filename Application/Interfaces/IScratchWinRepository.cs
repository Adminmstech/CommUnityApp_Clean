using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IScratchWinRepository
    {
        Task<BaseResponse> AddUpdateScratchWinGameAsync(AddUpdateScratchWinGameRequest model);
        Task<ScratchWinGameDto?> GetScratchWinGameByIdAsync(int gameId);
        Task<ScratchWinGameDto?> GetActiveScratchWinGameAsync();
        Task<IEnumerable<ScratchWinGameDto>> GetAllScratchWinGamesAsync();
        Task<BaseResponse> DeleteScratchWinGameAsync(int gameId);
        Task<BaseResponse> ToggleGameStatusAsync(int gameId);
        Task<int> GetTotalPlaysCountAsync(int gameId);
        Task<bool> TryConsumeRewardStockAsync(int rewardId);
        Task<int> TrackGameplayAsync(int gameId, Guid userId, int? rewardId, string rewardName, string rewardType, int coins, bool isWinner, int attemptNumber, string? redeemCode, string? qrCodePath, string? redeemLocation = null);
        Task AddRewardCoinsAsync(Guid userId, int coins, int gameId);
        Task<IEnumerable<ScratchWinPlayHistoryDto>> GetGameplayHistoryAsync(int? gameId, int pageNumber = 1, int pageSize = 50);
    }
}
