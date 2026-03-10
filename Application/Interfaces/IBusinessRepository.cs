using CommUnityApp.ApplicationCore.Models;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IBusinessRepository
    {
        Task<BusinessLoginResponse> LoginAsync(BusinessLoginRequest request);
        Task<BaseResponse> RegisterAsync(AddUpdateBusinessRequest request);
        Task<BusinessAddResponse> AddBusinessAsync(AddBusinessRequest request);
        Task<List<BusinessDetailsDto>> GetAllBusinesses();
        Task<BusinessDetailsDto> GetBusinessDetails(int BusinessId);
    }
}
