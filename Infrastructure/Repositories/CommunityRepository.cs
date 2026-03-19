using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using static CommUnityApp.ApplicationCore.Models.AssignVolunteerRequest;

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
        public async Task<List<Community>> GetCommunities()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<Community>("Get_AllCommunities", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
    }
}
