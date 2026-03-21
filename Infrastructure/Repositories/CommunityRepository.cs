using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly IConfiguration _configuration;

        public CommunityRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<CommunityLoginResponse> LoginAsync(CommunityLoginRequest request)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await connection.QueryFirstOrDefaultAsync<CommunityLoginResponse>(
                "sp_CommunityLogin",
                new
                {
                    UserName = request.UserName,
                    Password = request.Password
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<GroupDto>> GetGroupsByCommunityAsync(long communityId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return (await con.QueryAsync<GroupDto>(
                "sp_GetGroupsByCommunity",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            )).ToList();
        }

        public async Task<List<CharityItem>> GetCharityItemsByCommunityId(long communityId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return (await con.QueryAsync<CharityItem>(
                "SP_GetCharityItemsByCommunityId",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            )).ToList();
        }
        public async Task<IEnumerable<dynamic>> GetVolunteersList(long? communityId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "SP_GetVolunteersList",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> AssignVolunteer(long charityItemId, Guid assignedToUserId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var result = await con.ExecuteAsync(
                "SP_AssignVolunteerToCharity",
                new
                {
                    CharityItemId = charityItemId,
                    AssignedToUserId = assignedToUserId
                },
                commandType: CommandType.StoredProcedure
            );

            return result > 0;
        }
        public async Task<AssignedVolunteerModel> GetAssignedVolunteer(int charityItemId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                return con.QueryFirstOrDefault<AssignedVolunteerModel>(
                    "sp_GetAssignedVolunteerByCharityItem",
                    new { CharityItemId = charityItemId },
                    commandType: CommandType.StoredProcedure);
            }
        }
        public async Task<CharityItemModel> GetCharityItemDetails(int charityItemId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                return await con.QueryFirstOrDefaultAsync<CharityItemModel>(
                    "sp_GetCharityItemDetails",
                    new { CharityItemId = charityItemId },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<bool> UpdateVolunteerStatusAsync(UpdateStatusRequest request)
        {
            try
            {
                using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@CharityItemId", request.CharityItemId);
                    parameters.Add("@UserId", request.UserId);
                    parameters.Add("@Status", request.Status);

                    var result = await con.ExecuteAsync(
                        "SP_UpdateVolunteerStatus",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
