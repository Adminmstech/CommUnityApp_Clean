using CommUnityApp.BAL.Interfaces;
using CommUnityApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CommUnityApp.DAL
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
            using var connection =
     new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

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
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return (await con.QueryAsync<GroupDto>(
                "sp_GetGroupsByCommunity",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            )).ToList();
        }

    }

}

