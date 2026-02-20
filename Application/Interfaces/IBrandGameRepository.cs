using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IBrandGameRepository
    {
        Task<BaseResponse> AddUpdateBrandGameAsync(AddUpdateBrandGameRequest model, string brandGameImagePath, string unsuccessfulImagePath);
        Task<BrandGameDto> GetBrandGameByIdAsync(int brandGameId);
        Task<IEnumerable<BrandGameDto>> GetAllBrandGamesAsync();
        Task<IEnumerable<BrandGameDto>> GetBrandGamesByMerchantAsync(int merchantId);
        Task<BaseResponse> DeleteBrandGameAsync(int brandGameId);
    }
}
