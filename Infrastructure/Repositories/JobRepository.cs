using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.AspNet.SignalR.Infrastructure;
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
    public class JobRepository : IJobRepository
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
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new
            {
                model.JobId,
                model.ApplicantId,
                model.FullName,
                model.Email,
                model.Phone,

                model.ResumePath,
                model.CoverLetter
            };

            return await con.ExecuteScalarAsync<long>(
                "sp_ApplyJob",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task UpdateResumePath(long applicationId, string resumePath)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            await con.ExecuteAsync(
                "UPDATE CommunityJobApplications SET ResumePath = @ResumePath WHERE ApplicationId = @ApplicationId",
                new
                {
                    ResumePath = resumePath,
                    ApplicationId = applicationId
                });
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

        public async Task<BaseResponse> BusinessPostJob(BusinessJobPostModel model)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstOrDefaultAsync<BaseResponse>(
                "sp_BusinessPostJob",
                new
                {
                    model.JobId,
                    model.BusinessId,

                    model.JobTitle,
                    model.Description,
                    model.JobType,
                    model.Experience,
                    model.SalaryMin,
                    model.SalaryMax,
                    model.Location,
                    model.IsRemote,
                    model.Skills,
                    model.ContactEmail,
                    model.ContactPhone,
                    model.LastDateToApply
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<(int TotalRecords, IEnumerable<BusinessJobPostListModel> Jobs)> GetBusinessJobPosts(
         int businessId,
         int pageNumber,
         int pageSize)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi = await con.QueryMultipleAsync(
                "sp_GetBusinessJobPosts",
                new
                {
                    BusinessId = businessId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                commandType: CommandType.StoredProcedure);

            var totalRecords = await multi.ReadFirstAsync<int>();

            var jobs = await multi.ReadAsync<BusinessJobPostListModel>();

            return (totalRecords, jobs);
        }


        public async Task<BaseResponse> DeleteBusinessJobPost(
        int jobId,
        int businessId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstOrDefaultAsync<BaseResponse>(
                "sp_DeleteBusinessJobPost",
                new
                {
                    JobId = jobId,
                    BusinessId = businessId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<(int TotalRecords, IEnumerable<JobApplicantModel> Applicants)>
    GetBusinessJobApplicants(
        int jobId,
        int businessId,
        int pageNumber,
        int pageSize)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi = await con.QueryMultipleAsync(
                "sp_GetBusinessJobApplicants",
                new
                {
                    JobId = jobId,
                    BusinessId = businessId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                commandType: CommandType.StoredProcedure);

            var totalRecords = await multi.ReadFirstAsync<int>();

            var applicants =
                await multi.ReadAsync<JobApplicantModel>();

            return (totalRecords, applicants);
        }


        public async Task<JobApplicantModel?> GetBusinessJobApplicantDetails(
        int applicationId,
        int jobId,
        int businessId)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstOrDefaultAsync<JobApplicantModel>(
                "sp_GetBusinessJobApplicantDetails",
                new
                {
                    ApplicationId = applicationId,
                    JobId = jobId,
                    BusinessId = businessId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<UserJobListResponse> GetAllJobPostsForUsers(int pageNumber,int pageSize,string search)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi =
                await con.QueryMultipleAsync(
                    "sp_GetAllJobPostsForUsers",
                    new
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Search = search
                    },
                    commandType: CommandType.StoredProcedure);

            var totalRecords =
                await multi.ReadFirstAsync<int>();

            var jobs =
                await multi.ReadAsync<UserJobPostModel>();

            var totalPages =
                totalRecords == 0
                    ? 0
                    : (int)Math.Ceiling(
                        (double)totalRecords / pageSize);

            return new UserJobListResponse
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Data = jobs
            };
        }

        public async Task<JobApplicationsReceivedResponse>GetJobApplicationsReceivedByUser( Guid userId,int pageNumber,int pageSize)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            using var multi =
                await con.QueryMultipleAsync(
                    "sp_GetJobApplicationsReceivedByUser",
                    new
                    {
                        UserId = userId,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    },
                    commandType: CommandType.StoredProcedure);


            var totalRecords =
                await multi.ReadFirstOrDefaultAsync<int>();


            var applications =
                (await multi.ReadAsync<JobApplicationReceivedModel>())
                .ToList();


            var totalPages =
                totalRecords == 0
                    ? 0
                    : (int)Math.Ceiling(
                        (double)totalRecords / pageSize);


            return new JobApplicationsReceivedResponse
            {
                TotalRecords = totalRecords,

                PageNumber = pageNumber,

                PageSize = pageSize,

                TotalPages = totalPages,

                Data = applications
            };
        }
    }
}
