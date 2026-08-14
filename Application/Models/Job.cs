using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Job
    {
    }

    public class JobPostModel
    {
        public int BusinessId { get; set; }

        public int GroupId { get; set; }
        public Guid PostedBy { get; set; }

        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }

        public string JobType { get; set; }
        public string Experience { get; set; }

        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }

        public string Location { get; set; }
        public string Skills { get; set; }

        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public DateTime LastDateToApply { get; set; }
    }

    public class ApplyAppJobModel
    {
        public long JobId { get; set; }
        public Guid ApplicantId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        public string TotalExperience { get; set; }
        public string Skills { get; set; }

        public string Qualification { get; set; }

        public string? ResumePath { get; set; }
        public string CoverLetter { get; set; }

        public string CurrentLocation { get; set; }
        public decimal ExpectedSalary { get; set; }
        public string NoticePeriod { get; set; }
        public IFormFile? ResumeFile { get; set; }
    }
    public class ApplyJobModel
    {
        public long JobId { get; set; }
        public Guid ApplicantId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? ResumePath { get; set; }
        public string CoverLetter { get; set; }
        public IFormFile? ResumeFile { get; set; }
    }
    public class ApplyJobModelForApp
    {
        public long JobId { get; set; }

        public Guid ApplicantId { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? ResumePath { get; set; }

        public string? ResumeBase64 { get; set; }

        public string? CoverLetter { get; set; }

        public IFormFile? ResumeFile { get; set; }
    }
    public class ApplicationHistoryModel
    {
        public long HistoryId { get; set; }
        public long ApplicationId { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public Guid UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class BusinessJobPostModel
    {
        public int JobId { get; set; }

        public int BusinessId { get; set; }

        public string CompanyName { get; set; }

        public string JobTitle { get; set; }

        public string Description { get; set; }

        public string JobType { get; set; }

        public string Experience { get; set; }

        public decimal SalaryMin { get; set; }

        public decimal SalaryMax { get; set; }

        public string Location { get; set; }

        public bool IsRemote { get; set; }

        public string Skills { get; set; }

        public string ContactEmail { get; set; }

        public string ContactPhone { get; set; }

        public DateTime LastDateToApply { get; set; }
    }

    public class BusinessJobPostListModel
    {
        public int JobId { get; set; }

        public int BusinessId { get; set; }

        public string CompanyName { get; set; }

        public string Logo { get; set; }

        public string JobTitle { get; set; }

        public string Description { get; set; }

        public string JobType { get; set; }

        public string Experience { get; set; }

        public decimal SalaryMin { get; set; }

        public decimal SalaryMax { get; set; }

        public string Location { get; set; }

        public bool IsRemote { get; set; }

        public string Skills { get; set; }

        public string ContactEmail { get; set; }

        public string ContactPhone { get; set; }

        public DateTime LastDateToApply { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class JobApplicantModel
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public Guid ApplicantId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }

        public string Gender { get; set; }
        public string TotalExperience { get; set; }
        public string CurrentCompany { get; set; }
        public string CurrentRole { get; set; }
        public string Skills { get; set; }
        public string Qualification { get; set; }
        public string University { get; set; }

        public string ResumePath { get; set; }
        public string CoverLetter { get; set; }

        public string CurrentLocation { get; set; }
        public string PreferredLocation { get; set; }

        public decimal? CurrentSalary { get; set; }
        public decimal? ExpectedSalary { get; set; }

        public string NoticePeriod { get; set; }
        public string Status { get; set; }

        public DateTime AppliedDate { get; set; }
    }

    public class UserJobPostModel
    {
        public int JobId { get; set; }

        public int? BusinessId { get; set; }

        public int? GroupId { get; set; }

        public string JobTitle { get; set; }

        public string CompanyName { get; set; }

        public string Description { get; set; }

        public string JobType { get; set; }

        public string Experience { get; set; }

        public decimal SalaryMin { get; set; }

        public decimal SalaryMax { get; set; }

        public string Location { get; set; }

        public bool IsRemote { get; set; }

        public string Skills { get; set; }

        public string ContactEmail { get; set; }

        public string ContactPhone { get; set; }

        public DateTime LastDateToApply { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class UserJobListResponse
    {
        public int TotalRecords { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public IEnumerable<UserJobPostModel> Data { get; set; }
            = new List<UserJobPostModel>();
    }

    public class JobApplicationReceivedModel
    {
        // Application
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        public Guid ApplicantId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? Age { get; set; }

        public string Gender { get; set; }

        public string TotalExperience { get; set; }

        public string CurrentCompany { get; set; }

        public string CurrentRole { get; set; }

        public string Skills { get; set; }

        public string Qualification { get; set; }

        public string University { get; set; }

        public string ResumePath { get; set; }

        public string CoverLetter { get; set; }

        public string CurrentLocation { get; set; }

        public string PreferredLocation { get; set; }

        public decimal? CurrentSalary { get; set; }

        public decimal? ExpectedSalary { get; set; }

        public string NoticePeriod { get; set; }

        public string ApplicationStatus { get; set; }

        public DateTime AppliedDate { get; set; }


        // Job
        public string JobTitle { get; set; }

        public string CompanyName { get; set; }

        public string JobType { get; set; }

        public string JobExperience { get; set; }

        public string JobLocation { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public bool IsRemote { get; set; }

        public int? BusinessId { get; set; }

        public string JobStatus { get; set; }

        public DateTime JobCreatedDate { get; set; }
    }

    public class JobApplicationsReceivedResponse
    {
        public int TotalRecords { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public IEnumerable<JobApplicationReceivedModel> Data { get; set; }
            = new List<JobApplicationReceivedModel>();
    }
}
