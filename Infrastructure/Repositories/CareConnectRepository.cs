using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class CareConnectRepository : ICareConnectRepository
    {
        private readonly IConfiguration _configuration;

        public CareConnectRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IEnumerable<dynamic>> GetServices()
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return await con.QueryAsync(
                "sp_GetCareConnectServices",
                commandType: CommandType.StoredProcedure);
        }
        public async Task<long> CreateRequest(CareRequestModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QuerySingleAsync<long>(
                "sp_CreateCareConnectServiceRequest",
                model,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IEnumerable<dynamic>> GetSupporters(
             int serviceId,
             int communityId,
             decimal latitude,
             decimal longitude)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetCareConnectSupportersByService",
                new
                {
                    ServiceId = serviceId,
                    CommunityId = communityId,
                    Latitude = latitude,
                    Longitude = longitude
                },
                commandType: CommandType.StoredProcedure);
        }

       

        public async Task SendMessage(SendMessageModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            await con.QueryAsync(
                "sp_CareConnectServiceSendMessage",
               new
               {
                   ChatThreadId = model.ChatThreadId,
                   SenderId = model.SenderId,
                   Message = model.Message
               },
        commandType: CommandType.StoredProcedure
    );
        }

        public async Task<dynamic> ConnectSupporter(ConnectSupporterModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstAsync(
                "sp_ConnectSupporter",
                model,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<dynamic>> GetMessages(long chatThreadId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetChatMessages",
                new { ChatThreadId = chatThreadId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task RespondRequest(RespondRequestModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            await con.ExecuteAsync(
                "sp_RespondRequest",
                model,
                commandType: CommandType.StoredProcedure);
        }

        
        public async Task FinalizeSupporter(FinalizeSupporterModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            await con.ExecuteAsync(
                "sp_FinalizeSupporter",
                model,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<long> CreateRequestWithSupporters(CreateRequestWithSupportersModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var supporterIdsCsv = string.Join(",", model.SupporterIds);

            return await con.ExecuteScalarAsync<long>(
                "sp_CreateCareConnectServiceRequest",
                new
                {
                    model.UserId,
                    model.ServiceId,
                    model.Description,
                    model.Latitude,
                    model.Longitude,
                    model.Address,
                    SupporterIds = supporterIdsCsv
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<dynamic>> GetUserMessages(Guid userId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetUserCareConnectMessages",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );
        }
    }

}

