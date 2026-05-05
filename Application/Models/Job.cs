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

    public class ApplyJobModel
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

        public string ResumePath { get; set; }
        public string CoverLetter { get; set; }

        public string CurrentLocation { get; set; }
        public decimal ExpectedSalary { get; set; }
        public string NoticePeriod { get; set; }
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
}
