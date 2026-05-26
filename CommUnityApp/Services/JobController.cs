using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {

        private readonly IJobRepository _jobsRepository;
        private readonly IWebHostEnvironment _environment;


        public JobController(IJobRepository jobsRepository, IWebHostEnvironment environment)
        {
            _jobsRepository = jobsRepository;
            _environment = environment; 
        }

        [HttpPost("PostJob")]
        public async Task<IActionResult> PostJob([FromBody] JobPostModel model)
        {
            var jobId = await _jobsRepository.PostJob(model);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Job posted successfully",
                JobId = jobId
            });
        }
        [HttpPost("ApplyJob")]
        public async Task<IActionResult> ApplyJob(
            [FromBody] ApplyJobModel model)
        {
          
            var appId =
                await _jobsRepository
                .ApplyJob(model);

            string resumePath = "";


            if (!string.IsNullOrEmpty(model.ResumePath))
            {
                string folderPath = 
                    Path.Combine(
                        _environment.WebRootPath,
                        "Uploads",
                        "JobApplications",
                        appId.ToString()
                    );


                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

              

                string base64Data =
                    model.ResumePath.Trim();


                string extension = ".pdf";

                if (base64Data.StartsWith("iVBOR"))
                {
                    extension = ".png";
                }

                else if (base64Data.StartsWith("/9j/"))
                {
                    extension = ".jpg";
                }

                else if (base64Data.StartsWith("JVBER"))
                {
                    extension = ".pdf";
                }

             

                if (base64Data.Contains(","))
                {
                    base64Data =
                        base64Data.Split(',')[1];
                }


                byte[] fileBytes =
                    Convert.FromBase64String(base64Data);

          

                string fileName =
                    "Resume" + extension;

                string filePath =
                    Path.Combine(folderPath,
                                 fileName);


                await System.IO.File
                    .WriteAllBytesAsync(
                        filePath,
                        fileBytes);


                resumePath =
                    $"/Uploads/JobApplications/{appId}/{fileName}";

          

                await _jobsRepository
                    .UpdateResumePath(
                        appId,
                        resumePath);
            }

  
            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Applied successfully",
                ApplicationId = appId,
                ResumePath = resumePath
            });
        }


        [HttpPost("ApplyForJob")]
        public async Task<IActionResult> ApplyJobLink([FromForm] ApplyJobModel model)
        {
            var appId = await _jobsRepository.ApplyJob(model);

            string resumePath = "";

            if (model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                string folderPath = Path.Combine(
                    _environment.WebRootPath,
                    "Uploads",
                    "JobApplications",
                    appId.ToString()
                );

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string extension = Path.GetExtension(model.ResumeFile.FileName);

                string fileName = "Resume" + extension;

                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(stream);
                }

                resumePath = $"/Uploads/JobApplications/{appId}/{fileName}";

                await _jobsRepository.UpdateResumePath(appId, resumePath);
            }

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Applied successfully",
                ApplicationId = appId,
                ResumePath = resumePath
            });
        }
        
        [HttpGet("GetJobsByGroup")]
        public async Task<IActionResult> GetJobsByGroup(int groupId)
        {
            var data = await _jobsRepository.GetJobsByGroup(groupId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Data = data
            });
        }


        [HttpGet("GetJobsByUser")]
        public async Task<IActionResult> GetJobsByUser(Guid userId)
        {
            var data = await _jobsRepository.GetJobsByUser(userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Data = data
            });
        }

        [HttpGet("GetJobDetails")]
        public async Task<IActionResult> GetJobDetails(long jobId)
        {
            var data = await _jobsRepository.GetJobDetails(jobId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Data = data
            });
        }

        [HttpGet("GetMyApplications")]
        public async Task<IActionResult> GetMyApplications(Guid userId)
        {
            var data = await _jobsRepository.GetMyApplications(userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Data = data
            });
        }

        [HttpGet("GetApplicationsByJob")]
        public async Task<IActionResult> GetApplicationsByJob(long jobId)
        {
            var data = await _jobsRepository.GetApplicationsByJob(jobId);

            return Ok(new { ResultId = 1, Data = data });
        }

        [HttpPost("UpdateApplicationStatus")]
        public async Task<IActionResult> UpdateApplicationStatus([FromBody] UpdateStatusModel model)
        {
            var result = await _jobsRepository.UpdateApplicationStatus(model);

            return Ok(result);
        }

        [HttpGet("GetApplicationHistory")]
        public async Task<IActionResult> GetApplicationHistory(long applicationId)
        {
            if (applicationId <= 0)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = "Invalid ApplicationId"
                });
            }

            var data = await _jobsRepository.GetApplicationHistory(applicationId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Data = data
            });
        }
    }
}
