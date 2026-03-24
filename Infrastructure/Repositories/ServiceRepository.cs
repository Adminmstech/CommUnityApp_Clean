using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class ServiceRepository: IServiceRepository
    {
        private readonly IConfiguration _configuration;

        public ServiceRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<BaseResponse> AddServiceAppointment(ServiceAppointment entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@ServiceId", entity.ServiceId);
            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@AppointmentDateTime", entity.AppointmentDateTime);
            parameters.Add("@Notes", entity.Notes);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_ServiceAppointment",   
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }


        public async Task<List<ServiceListResponse>> GetAllServices(ServiceSearchRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@SearchText", request?.SearchText ?? (object)DBNull.Value);
            parameters.Add("@CategoryId", request?.CategoryId ?? (object)DBNull.Value);
            parameters.Add("@BusinessId", request?.BusinessId ?? (object)DBNull.Value);

            var result = await connection.QueryAsync<ServiceListResponse>(
                "sp_GetAllServices",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<List<AppointmentResponse>> GetUserAppointments(Guid UserId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", UserId);

            var result = await connection.QueryAsync<AppointmentResponse>(
                "Get_AppointmentsByUserId",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<ServiceDetailsResponse> GetServiceDetails(int serviceId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@ServiceId", serviceId);

            using var multi = await connection.QueryMultipleAsync(
                "Get_ServiceDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var service = await multi.ReadFirstOrDefaultAsync<ServiceDetails>();
            var otherServices = (await multi.ReadAsync<RelatedService>()).ToList();

            return new ServiceDetailsResponse
            {
                Service = service,
                OtherServices = otherServices
            };
        }

        public async Task<List<BusinessAppointmentDto>> GetBusinessAppointmts( int BusinessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@BusinessId", BusinessId);

            var result = await connection.QueryAsync<BusinessAppointmentDto>(
                "Get_BusinessAppointments",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<UpdateAppointmentStatusResponse> UpdateAppointment(UpdateAppointmentStatusRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@AppointmentId", entity.AppointmentId);
            parameters.Add("@Status", entity.Status);
           

            var result = await connection.QueryAsync<UpdateAppointmentStatusResponse>(
                "Update_AppointmentStatus",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<BaseResponse> AddService(AddServiceRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@BusinessId", entity.BusinessId);
            parameters.Add("@ServiceName", entity.ServiceName);
            parameters.Add("@Description", entity.Description);
            parameters.Add("@Price", entity.Price);
            parameters.Add("@DurationMinutes", entity.DurationMinutes);
            parameters.Add("@IsBookingRequired", entity.IsBookingRequired);
            parameters.Add("@IsActive", entity.IsActive);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_Service",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<BaseResponse> AddOrUpdateServiceImage(AddOrUpdateServiceImageRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();

            parameters.Add("@ImageId", entity.ImageId);
            parameters.Add("@ServiceId", entity.ServiceId);
            parameters.Add("@ImageUrl", entity.ImageUrl);
            parameters.Add("@IsPrimary", entity.IsPrimary);
            parameters.Add("@IsActive", entity.IsActive);

            return await connection.QueryFirstOrDefaultAsync<BaseResponse>( "Add_ServiceImage",parameters, commandType: CommandType.StoredProcedure);
        }


        public async Task<List<ServiceModel>> GetBusinessServices(int BusinessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@BusinessId", BusinessId);

            var result = await connection.QueryAsync<ServiceModel>(
                "Get_BusinessServices",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<List<ServiceImageModel>> GetServiceImages(int serviceId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            var parameters = new DynamicParameters();
            parameters.Add("@ServiceId", serviceId);

            var result = await connection.QueryAsync<ServiceImageModel>(
                "Get_ServiceImagesByServiceId",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }
}
