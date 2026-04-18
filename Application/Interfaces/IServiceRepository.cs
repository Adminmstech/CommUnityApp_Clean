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
        Task<ServiceHomeResponse> GetAllServices(ServiceSearchRequest request);
        Task<List<AppointmentResponse>> GetUserAppointments(Guid UserId);
        Task<ServiceDetailsResponse> GetServiceDetails(int serviceId);
        Task<List<BusinessAppointmentDto>> GetBusinessAppointmts(int BusinessId);
        Task<UpdateAppointmentStatusResponse> UpdateAppointment(UpdateAppointmentStatusRequest entity);
        Task<BaseResponse> AddService(AddServiceRequest entity);
        Task<BaseResponse> AddOrUpdateServiceImage(AddOrUpdateServiceImageRequest entity);
        Task<List<ServiceModel>> GetBusinessServices(int BusinessId);
        Task<List<ServiceImageModel>> GetServiceImages(int serviceId);
        Task<BaseResponse> AddServiceSubCategory(ServiceSubCategory entity);
        Task<BaseResponse> AddBusinessCategory(BusinessCategory entity);
        Task<CategoryServicesWithImagesResponse> GetServicesByCategory(int categoryId);
        Task<List<ServiceListResponse>> GerserviceBySubcategory(int SubCategoryId);


    }
}
