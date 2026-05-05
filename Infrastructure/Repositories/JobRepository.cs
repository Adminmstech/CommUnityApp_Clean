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
    public class JobRepository: IJobRepository
    {
        private readonly IConfiguration _configuration;

    public JobRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

        public async Task<long> PostJob(JobPostModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.ExecuteScalarAsync<long>(
                "sp_PostJob",
                model,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<long> ApplyJob(ApplyJobModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.ExecuteScalarAsync<long>(
                "sp_ApplyJob",
                model,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<dynamic>> GetJobsByGroup(int groupId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetJobsByGroup",
                new { GroupId = groupId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<dynamic>> GetJobsByUser(Guid userId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetJobsByUser",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<dynamic> GetJobDetails(long jobId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstOrDefaultAsync(
                "sp_GetJobDetails",
                new { JobId = jobId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<dynamic>> GetMyApplications(Guid userId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetMyApplications",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<dynamic>> GetApplicationsByJob(long jobId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync(
                "sp_GetApplicationsByJob",
                new { JobId = jobId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<dynamic> UpdateApplicationStatus(UpdateStatusModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstOrDefaultAsync(
                "sp_UpdateApplicationStatus",
                model,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<ApplicationHistoryModel>> GetApplicationHistory(long applicationId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync<ApplicationHistoryModel>(
                "sp_GetApplicationStatusHistory",
                new { ApplicationId = applicationId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
