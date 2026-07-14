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
        public async Task<IEnumerable<CareConnectServiceModel>> GetServices()
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = (await con.QueryAsync<CareConnectServiceModel>(
                "sp_GetCareConnectServices",
                commandType: CommandType.StoredProcedure))
                .ToList();

            foreach (var item in result)
            {
                if (!string.IsNullOrWhiteSpace(item.ServiceImage))
                {
item.ServiceImagePath =
    $"{_configuration["ImageBaseUrl"]}/Uploads/CareConnectServices/{item.ServiceId}/{item.ServiceImage}";
                }
            }

            return result;
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
             decimal longitude,
             Guid userId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
             
            return await con.QueryAsync(
                "sp_GetCareConnectSupportersByService",
                new
                {
                    ServiceId = serviceId,
                    CommunityId = communityId,
                    Latitude = latitude,
                    Longitude = longitude,
                    UserId=userId
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

        public async Task<IEnumerable<dynamic>> GetMessages(long chatThreadId,Guid userId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetChatMessages",
                new { ChatThreadId = chatThreadId,
                UserId=userId},
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

        public async Task<IEnumerable<dynamic>> GetSupporterRequestList(Guid supporterId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetCareConnectSupporterRequestList", 
                new { SupporterId = supporterId },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<dynamic> AddSupporterService(AddSupporterServiceModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

           
            {
                var result =
                    await con
                    .QueryFirstOrDefaultAsync<dynamic>(
                        "sp_AddSupporterService",
                        new
                        {
                            UserId = model.UserId,
                            ServiceIds = model.ServiceIds
                        },
                        commandType:
                        CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<dynamic>SendCareConnectMessage(SendCareMessageModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            {
                return await con
                    .QueryFirstOrDefaultAsync<dynamic>(
                        "sp_SendCareConnectMessage",
                        new
                        {
                            ChatThreadId = model.ChatThreadId,
                            SenderId = model.SenderId,
                            MessageText = model.MessageText,
                            MessageType = model.MessageType
                        },
                        commandType:
                        CommandType.StoredProcedure);
            }
        }

        public async Task<dynamic> AddVolunteerRole( Guid userId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            {
                var result =
                    await con
                    .QueryFirstOrDefaultAsync<dynamic>(
                        "sp_AddUserAsVolunteer",
                        new
                        {
                            UserId = userId
                        },
                        commandType:
                        CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<CareConnectDashboardResponse> GetCareConnectRequests()
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi = await con.QueryMultipleAsync(
                "SP_GetAdminCareConnectList",
                commandType: CommandType.StoredProcedure);

            var counts = await multi.ReadFirstOrDefaultAsync<dynamic>();

            var requests = (await multi.ReadAsync<CareConnectRequestItem>()).ToList();

            return new CareConnectDashboardResponse
            {
                TotalRequests = counts.TotalRequests,
                TotalUsers = counts.TotalUsers,
                Requests = requests
            };
        }

        public async Task<CompleteCareConnectRequestResult>
     CompleteCareConnectRequest(long requestId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con
                .QueryFirstOrDefaultAsync<CompleteCareConnectRequestResult>(
                    "sp_CompleteCareConnectRequest",
                    new
                    {
                        RequestId = requestId
                    },
                    commandType: CommandType.StoredProcedure);

            return result ?? new CompleteCareConnectRequestResult
            {
                ResultId = 0,
                ResultMessage = "Unable to complete Care Connect request.",
                Status = false,
                RequestId = requestId
            };
        }
    }

}

