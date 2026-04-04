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
    public class VolunteerRepository : IVolunteerRepository
    {
        private readonly IConfiguration _configuration;

        public VolunteerRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<dynamic> VolunteerLogin(string email, string password)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                var user = await con.QueryFirstOrDefaultAsync(
                    "SELECT * FROM Users WHERE Email=@Email AND Password=@Password AND IsActive=1",
                    new { Email = email, Password = password });

                return user;
            }
        }

        public async Task<List<VolunteerAssignedItemModel>> GetVolunteerAssignedRequests(Guid volunteerId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                var result = await con.QueryAsync<VolunteerAssignedItemModel>(
                    "sp_GetVolunteerAssignedRequests",
                    new { VolunteerId = volunteerId },
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
        }

        public async Task UpdateVolunteerRequestStatus(VolunteerStatusUpdateModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            {
                await con.ExecuteAsync(
                    "sp_UpdateVolunteerRequestStatus",
                    new
                    {
                        model.RequestId,
                        model.Status
                    },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}

