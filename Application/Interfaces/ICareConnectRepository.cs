using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface ICareConnectRepository
    {
        Task<IEnumerable<dynamic>> GetServices();
        Task<IEnumerable<dynamic>> GetSupporters(int serviceId, int communityId, decimal latitude, decimal longitude);
        Task<dynamic> ConnectSupporter(ConnectSupporterModel model);
        Task<long> CreateRequest(CareRequestModel model);
        Task SendMessage(SendMessageModel model);
        Task<IEnumerable<dynamic>> GetMessages(long chatThreadId);
        Task RespondRequest(RespondRequestModel model);
        Task FinalizeSupporter(FinalizeSupporterModel model);
        Task<long> CreateRequestWithSupporters(CreateRequestWithSupportersModel model);
        Task<IEnumerable<dynamic>> GetUserMessages(Guid userId);
    }
}
