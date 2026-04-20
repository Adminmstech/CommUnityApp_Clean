using CommUnityApp.ApplicationCore.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class GameResultsRepository : IGameResultsRepository
    {
        private readonly IConfiguration _configuration;

        public GameResultsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(IEnumerable<dynamic> Data, int Total)> GetGamePlayMembers(int page, int size, string search)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi = await con.QueryFirstOrDefaultAsync(
                "sp_GetGamePlayMembers",
                new
                {
                    PageNumber = page,
                    PageSize = size,
                    Search = string.IsNullOrEmpty(search) ? null : search
                },
                commandType: CommandType.StoredProcedure);

            var data = await multi.ReadAsync();
            var total = await multi.ReadFirstAsync<int>();

            return (data, total);
        }


    }
}
