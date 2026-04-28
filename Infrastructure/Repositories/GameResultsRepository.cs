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

            using var multi = await con.QueryMultipleAsync(  
                "sp_GetBrandGamePlayMembers",
                new
                {
                    PageNumber = page,
                    PageSize = size,
                    Search = string.IsNullOrEmpty(search) ? null : search
                },
                commandType: CommandType.StoredProcedure);

            var data = await multi.ReadAsync();   

            var totalObj = await multi.ReadFirstOrDefaultAsync<dynamic>();  

            int total = totalObj?.TotalCount ?? 0;

            return (data, total);
        }

        public async Task<(IEnumerable<dynamic> Data, int Total)> GetSpinGameResults(int page, int size, string search)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi = await con.QueryMultipleAsync(
                "sp_GetSpinGameResults",
                new
                {
                    PageNumber = page,
                    PageSize = size,
                    Search = string.IsNullOrEmpty(search) ? null : search
                },
                commandType: CommandType.StoredProcedure);

            var data = await multi.ReadAsync();
            var totalObj = await multi.ReadFirstOrDefaultAsync<dynamic>();

            int total = totalObj?.TotalCount ?? 0;

            return (data, total);
        }

        public async Task<bool> AssignPrize(AssignPrizeModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                var result = await con.ExecuteAsync(
                    "sp_AssignPrize",
                    new
                    {
                        model.MemberId,
                        model.Address,
                        model.DeliveryType
                    },
                    commandType: CommandType.StoredProcedure);

                return result > 0;
            }
        }
    }
}
