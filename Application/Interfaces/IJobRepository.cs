using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IJobRepository
    {

        Task<long> PostJob(JobPostModel model);
        Task<long> ApplyJob(ApplyJobModel model);
        Task<IEnumerable<dynamic>> GetJobsByGroup(int groupId);
        Task<IEnumerable<dynamic>> GetJobsByUser(Guid userId);
        Task<dynamic> GetJobDetails(long jobId);
        Task<IEnumerable<dynamic>> GetMyApplications(Guid userId);
        Task<IEnumerable<dynamic>> GetApplicationsByJob(long jobId);
        Task<dynamic> UpdateApplicationStatus(UpdateStatusModel model);
        Task<IEnumerable<ApplicationHistoryModel>> GetApplicationHistory(long applicationId);

        Task UpdateResumePath(long applicationId, string resumePath);

        Task<BaseResponse> BusinessPostJob(BusinessJobPostModel model);

        Task<(int TotalRecords, IEnumerable<BusinessJobPostListModel> Jobs)> GetBusinessJobPosts(int businessId,int pageNumber,int pageSize);
        Task<BaseResponse> DeleteBusinessJobPost(int jobId,int businessId);
        Task<(int TotalRecords, IEnumerable<JobApplicantModel> Applicants)>GetBusinessJobApplicants(int jobId,int businessId,int pageNumber,int pageSize);
        Task<JobApplicantModel?> GetBusinessJobApplicantDetails(int applicationId,int jobId,int businessId);

        Task<UserJobListResponse> GetAllJobPostsForUsers(int pageNumber,int pageSize,string search);

        Task<JobApplicationsReceivedResponse> GetJobApplicationsReceivedByUser(Guid userId, int pageNumber, int pageSize);
    }
}
