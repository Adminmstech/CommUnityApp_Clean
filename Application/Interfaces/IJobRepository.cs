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
    }
}
