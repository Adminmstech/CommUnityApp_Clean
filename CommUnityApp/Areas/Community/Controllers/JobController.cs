using CommUnityApp.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Community.Controllers
{
    [Area("Community")]
    public class JobController:Controller
    {
        private readonly IJobRepository _jobsRepository;

        public JobController(IJobRepository jobsRepository)
        {
            _jobsRepository = jobsRepository;
        }
        public IActionResult PostJob()
            {
                return View();
            }
        public IActionResult ApplyJob()
        {
            return View();
        }

        public async Task<IActionResult> Apply(long jobId)
        {
            if (jobId == 0)
                return Content("❌ jobId missing");

            var job = await _jobsRepository.GetJobDetails(jobId);

            if (job == null)
                return Content("❌ Job not found");

            ViewBag.Job = job;

            return View("ApplyJob");
        }

        public IActionResult MyApplications()
            {
                return View();
            }
    
            public IActionResult ApplicationsByJob()
            {
                return View();
            }
    
            public IActionResult UpdateApplicationStatus()
            {
                return View();
            }
    
            public IActionResult ApplicationHistory()
            {
                return View();
            }
    }
}
