using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IServiceRepository
    {
        Task<BaseResponse> AddServiceAppointment(ServiceAppointment entity);
        Task<List<ServiceListResponse>> GetAllServices(ServiceSearchRequest request);
        Task<List<AppointmentResponse>> GetUserAppointments(Guid UserId);
        Task<ServiceDetailsResponse> GetServiceDetails(int serviceId);
    }
}
