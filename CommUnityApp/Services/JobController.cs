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
        //[HttpPost("ApplyJob")]
        //public async Task<IActionResult> ApplyJob(
        //    [FromBody] ApplyJobModel model)
        //{

        //    var appId =
        //        await _jobsRepository
        //        .ApplyJob(model);

        //    string resumePath = "";


        //    if (!string.IsNullOrEmpty(model.ResumePath))
        //    {
        //        string folderPath = 
        //            Path.Combine(
        //                _environment.WebRootPath,
        //                "Uploads",
        //                "JobApplications",
        //                appId.ToString()
        //            );


        //        if (!Directory.Exists(folderPath))
        //        {
        //            Directory.CreateDirectory(folderPath);
        //        }



        //        string base64Data =
        //            model.ResumePath.Trim();


        //        string extension = ".pdf";

        //        if (base64Data.StartsWith("iVBOR"))
        //        {
        //            extension = ".png";
        //        }

        //        else if (base64Data.StartsWith("/9j/"))
        //        {
        //            extension = ".jpg";
        //        }

        //        else if (base64Data.StartsWith("JVBER"))
        //        {
        //            extension = ".pdf";
        //        }



        //        if (base64Data.Contains(","))
        //        {
        //            base64Data =
        //                base64Data.Split(',')[1];
        //        }


        //        byte[] fileBytes =
        //            Convert.FromBase64String(base64Data);



        //        string fileName =
        //            "Resume" + extension;

        //        string filePath =
        //            Path.Combine(folderPath,
        //                         fileName);


        //        await System.IO.File
        //            .WriteAllBytesAsync(
        //                filePath,
        //                fileBytes);


        //        resumePath =
        //            $"/Uploads/JobApplications/{appId}/{fileName}";



        //        await _jobsRepository
        //            .UpdateResumePath(
        //                appId,
        //                resumePath);
        //    }


        //    return Ok(new
        //    {
        //        ResultId = 1,
        //        ResultMessage = "Applied successfully",
        //        ApplicationId = appId,
        //        ResumePath = resumePath
        //    });
        //}
        [HttpPost("ApplyJob")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ApplyJob([FromForm] ApplyJobModel model)
        {
            var appId = await _jobsRepository.ApplyJob(model);

            string resumePath = "";

            if (model.ResumeFile != null &&
                model.ResumeFile.Length > 0)
            {
                var extension = Path.GetExtension(
                    model.ResumeFile.FileName)
                    .ToLowerInvariant(); 

                var allowedExtensions = new[] 
                {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
        };

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage =
                            "Only PDF, JPG, JPEG and PNG files are allowed."
                    });
                }

                string folderPath = Path.Combine(
                    _environment.WebRootPath,
                    "Uploads",
                    "JobApplications",
                    appId.ToString()
                );

                Directory.CreateDirectory(folderPath);

                string fileName = "Resume" + extension;

                string filePath = Path.Combine(
                    folderPath,
                    fileName
                );

                await using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(stream);
                }

                resumePath =
                    $"/Uploads/JobApplications/{appId}/{fileName}";

                await _jobsRepository.UpdateResumePath(
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


        [HttpGet("GetPostedJobsByUser")]
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
        [HttpPost("BusinessPostJob")]
        public async Task<IActionResult> BusinessPostJob([FromBody] BusinessJobPostModel model)
        {
            try
            {
                var jobId = await _jobsRepository.BusinessPostJob(model);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = model.JobId == 0
                        ? "Job posted successfully."
                        : "Job updated successfully.",
                    JobId = jobId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }
        [HttpGet("GetBusinessJobPosts")]
        public async Task<IActionResult> GetBusinessJobPosts(int pageNumber = 1,int pageSize = 10)
        {
            try
            {
                var businessIdString =
                    HttpContext.Session.GetString("BusinessId");

                if (!int.TryParse(businessIdString, out int businessId))
                {
                    return Unauthorized(new
                    {
                        ResultId = 0,
                        ResultMessage = "Business session expired."
                    });
                }

                var result = await _jobsRepository.GetBusinessJobPosts(
                    businessId,
                    pageNumber,
                    pageSize);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Jobs retrieved successfully.",
                    TotalRecords = result.TotalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(
                        (double)result.TotalRecords / pageSize),
                    Data = result.Jobs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }


        [HttpDelete("DeleteBusinessJobPost/{jobId:int}")]
        public async Task<IActionResult> DeleteBusinessJobPost(int jobId)
        {
            try
            {
                var businessIdString =
                    HttpContext.Session.GetString("BusinessId");

                if (!int.TryParse(businessIdString, out int businessId))
                {
                    return Unauthorized(new
                    {
                        ResultId = 0,
                        ResultMessage = "Business session expired."
                    });
                }

                var result = await _jobsRepository.DeleteBusinessJobPost(
                    jobId,
                    businessId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpGet("GetBusinessJobApplicants")]
        public async Task<IActionResult> GetBusinessJobApplicants(int jobId,int pageNumber = 1,int pageSize = 10)
        {
            try
            {
                var businessIdString =
                    HttpContext.Session.GetString("BusinessId");

                if (!int.TryParse(
                        businessIdString,
                        out int businessId))
                {
                    return Unauthorized(new
                    {
                        ResultId = 0,
                        ResultMessage = "Business session expired."
                    });
                }

                var result =
                    await _jobsRepository.GetBusinessJobApplicants(
                        jobId,
                        businessId,
                        pageNumber,
                        pageSize);

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage =
                        "Applicants retrieved successfully.",

                    TotalRecords =
                        result.TotalRecords,

                    PageNumber =
                        pageNumber,

                    PageSize =
                        pageSize,

                    TotalPages =
                        (int)Math.Ceiling(
                            (double)result.TotalRecords /
                            pageSize),

                    Data =
                        result.Applicants
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpGet("GetBusinessJobApplicantDetails")]
        public async Task<IActionResult> GetBusinessJobApplicantDetails(int jobId,int applicationId)
        {
            try
            {
                var businessIdString =
                    HttpContext.Session.GetString("BusinessId");

                if (!int.TryParse(
                        businessIdString,
                        out int businessId))
                {
                    return Unauthorized(new
                    {
                        ResultId = 0,
                        ResultMessage = "Business session expired."
                    });
                }

                var applicant =
                    await _jobsRepository.GetBusinessJobApplicantDetails(
                        applicationId,
                        jobId,
                        businessId);

                if (applicant == null)
                {
                    return NotFound(new
                    {
                        ResultId = 0,
                        ResultMessage = "Applicant not found."
                    });
                }

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage =
                        "Applicant details retrieved successfully.",
                    Data = applicant
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }


        [HttpGet("GetAllJobPosts")] 
        public async Task<IActionResult> GetAllJobPosts( int pageNumber = 1, int pageSize = 10, string search = null)
        {
            try
            { 
                var result =
                    await _jobsRepository.GetAllJobPostsForUsers(
                        pageNumber,
                        pageSize,
                        search);

                return Ok(new
                {
                    ResultId = 1,

                    ResultMessage =
                        "Jobs retrieved successfully.",

                    TotalRecords =
                        result.TotalRecords,

                    PageNumber =
                        result.PageNumber,

                    PageSize =
                        result.PageSize,

                    TotalPages =
                        result.TotalPages,

                    Data =
                        result.Data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpGet("GetJobApplicationsReceived")]
        public async Task<IActionResult> GetJobApplicationsReceived(Guid userId,int pageNumber = 1,int pageSize = 10)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "Valid UserId is required."
                    });
                }


                if (pageNumber < 1)
                    pageNumber = 1;


                if (pageSize < 1)
                    pageSize = 10;


                var result =
                    await _jobsRepository
                        .GetJobApplicationsReceivedByUser(
                            userId,
                            pageNumber,
                            pageSize);


                return Ok(new
                {
                    ResultId = 1,

                    ResultMessage =
                        result.TotalRecords > 0
                            ? "Job applications retrieved successfully."
                            : "No job applications found.",

                    TotalRecords =
                        result.TotalRecords,

                    PageNumber =
                        result.PageNumber,

                    PageSize =
                        result.PageSize,

                    TotalPages =
                        result.TotalPages,

                    Data =
                        result.Data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }
    }
}
