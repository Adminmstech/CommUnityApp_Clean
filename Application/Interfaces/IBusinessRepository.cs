using CommUnityApp.ApplicationCore.Models;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IBusinessRepository
    {
        Task<BusinessLoginResponse> LoginAsync(BusinessLoginRequest request);
        Task<BaseResponse> RegisterAsync(AddUpdateBusinessRequest request);
    }
}
