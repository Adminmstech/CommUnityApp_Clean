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
    }
}
