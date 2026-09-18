using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
        public class CommunityHelpRepository : ICommunityHelpRepository
        {
        private readonly IConfiguration _configuration;

        public CommunityHelpRepository(IConfiguration configuration)
        { 
            _configuration = configuration;
        } 
         

        public async Task<CommunityHelpResult> CreateHelpRequest(
                CreateCommunityHelpRequestModel model)
            {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@CommunityId", model.CommunityId);
                parameters.Add("@RequestedByUserId", model.RequestedByUserId);
                parameters.Add("@RequestTitle", model.RequestTitle);
                parameters.Add("@RequestDescription", model.RequestDescription);
                parameters.Add("@Location", model.Location);
                parameters.Add("@Latitude", model.Latitude);
                parameters.Add("@Longitude", model.Longitude);
                parameters.Add("@IsUrgent", model.IsUrgent);

                return await con.QueryFirstAsync<CommunityHelpResult>(
                    "SP_CreateCommunityHelpRequest",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }


            
            public async Task<(int TotalRecords, List<CommunityHelpRequestResponse> Data)>
                GetCommunityHelpRequests(
                    long communityId,
                    Guid? userId,
                    int pageNumber,
                    int pageSize,
                    string? search,
                    string? status)
            {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@CommunityId", communityId);
                parameters.Add("@UserId", userId);
                parameters.Add("@PageNumber", pageNumber);
                parameters.Add("@PageSize", pageSize);
                parameters.Add("@Search", search);
                parameters.Add("@Status", status);

                using var multi = await con.QueryMultipleAsync(
                    "SP_GetCommunityHelpRequests",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var count = await multi.ReadFirstOrDefaultAsync<dynamic>();

                var data = (await multi.ReadAsync<CommunityHelpRequestResponse>())
                    .ToList();

                int totalRecords = count?.TotalRecords ?? 0;

                return (totalRecords, data);
            }



            public async Task<CommunityHelpRequestDetailsResponse>
                GetHelpRequestDetails(long helpRequestId)
            {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@HelpRequestId", helpRequestId);

                using var multi = await con.QueryMultipleAsync(
                    "SP_GetCommunityHelpRequestDetails",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var request =
                    await multi.ReadFirstOrDefaultAsync<CommunityHelpRequestResponse>();

                var responses =
                    (await multi.ReadAsync<CommunityHelpOfferResponse>())
                    .ToList();

                return new CommunityHelpRequestDetailsResponse
                {
                    HelpRequest = request,
                    Responses = responses
                };
            }


            public async Task<CommunityHelpResult> CreateHelpOffer(
                CreateCommunityHelpOfferModel model)
            {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@HelpRequestId", model.HelpRequestId);
                parameters.Add("@OfferedByUserId", model.OfferedByUserId);
                parameters.Add("@OfferMessage", model.OfferMessage);

                return await con.QueryFirstAsync<CommunityHelpResult>(
                    "SP_CreateCommunityHelpOffer",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }


   
            public async Task<List<CommunityHelpOfferResponse>>
                GetHelpOffers(long helpRequestId)
            {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@HelpRequestId", helpRequestId);

                var result = await con.QueryAsync<CommunityHelpOfferResponse>(
                    "SP_GetCommunityHelpOffers",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }


            public async Task<(int TotalRecords, List<CommunityHelpMyRequestResponse> Data)>
                GetMyHelpRequests(
                    Guid userId,
                    int pageNumber,
                    int pageSize,
                    string? status)
            {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@UserId", userId);
                parameters.Add("@PageNumber", pageNumber);
                parameters.Add("@PageSize", pageSize);
                parameters.Add("@Status", status);

                using var multi = await con.QueryMultipleAsync(
                    "SP_GetMyCommunityHelpRequests",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var count = await multi.ReadFirstOrDefaultAsync<dynamic>();

                var data =
                    (await multi.ReadAsync<CommunityHelpMyRequestResponse>())
                    .ToList();

                int totalRecords = count?.TotalRecords ?? 0;

                return (totalRecords, data);
            }


            public async Task<(int TotalRecords, List<CommunityHelpMyOfferResponse> Data)>
                GetMyHelpOffers(
                    Guid userId,
                    int pageNumber,
                    int pageSize,
                    string? status)
            {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@UserId", userId);
                parameters.Add("@PageNumber", pageNumber);
                parameters.Add("@PageSize", pageSize);
                parameters.Add("@Status", status);

                using var multi = await con.QueryMultipleAsync(
                    "SP_GetMyCommunityHelpOffers",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var count = await multi.ReadFirstOrDefaultAsync<dynamic>();

                var data =
                    (await multi.ReadAsync<CommunityHelpMyOfferResponse>())
                    .ToList();

                int totalRecords = count?.TotalRecords ?? 0;

                return (totalRecords, data);
            }


      
            public async Task<CommunityHelpResult> ConnectHelper(
                ConnectCommunityHelpModel model)
            {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@HelpRequestId", model.HelpRequestId);
                parameters.Add("@HelpOfferId", model.HelpOfferId);
                parameters.Add("@RequesterUserId", model.RequesterUserId);

                return await con.QueryFirstAsync<CommunityHelpResult>(
                    "SP_ConnectCommunityHelpHelper",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }



            public async Task<CommunityHelpResult> CompleteHelp(
                CompleteCommunityHelpModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@ConnectionId", model.ConnectionId);
                parameters.Add("@UserId", model.UserId);

                return await con.QueryFirstAsync<CommunityHelpResult>(
                    "SP_CompleteCommunityHelp",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }


            public async Task<CommunityHelpResult> CloseHelpRequest(
                CloseCommunityHelpRequestModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@HelpRequestId", model.HelpRequestId);
                parameters.Add("@UserId", model.UserId);

                return await con.QueryFirstAsync<CommunityHelpResult>(
                    "SP_CloseCommunityHelpRequest",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }


            public async Task<CommunityHelpResult> CancelHelpRequest(
                CancelCommunityHelpRequestModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

                parameters.Add("@HelpRequestId", model.HelpRequestId);
                parameters.Add("@UserId", model.UserId);

                return await con.QueryFirstAsync<CommunityHelpResult>(
                    "SP_CancelCommunityHelpRequest",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }
    
}
