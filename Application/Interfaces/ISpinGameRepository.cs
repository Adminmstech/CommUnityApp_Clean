using CommUnityApp.ApplicationCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface ISpinGameRepository
    {
        Task<BaseResponse> AddUpdateSpinGameAsync(AddUpdateSpinGameRequest model);
        Task<SpinGameDto> GetSpinGameByIdAsync(int gameId);
        Task<IEnumerable<SpinGameDto>> GetAllSpinGamesAsync();
        Task<IEnumerable<SpinGameDto>> GetSpinGamesByBusinessAsync(int businessId);
        Task<BaseResponse> DeleteSpinGameAsync(int gameId);
        Task<SpinGameConfigRequest?> GetConfigByIdAsync(int configId);
        Task<IEnumerable<SpinSectionRequest>> GetSectionsByGameIdAsync(int gameId);
        Task<BaseResponse> AddUpdateConfigAsync(SpinGameConfigRequest model);
        Task<BaseResponse> AddUpdateSectionAsync(SpinSectionRequest model);
        Task<BaseResponse> DeleteSectionAsync(int sectionId);
    }
}

