using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public ServiceController(ILogger<ServiceController> logger, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;
        }

        [HttpPost("Add_Appointment")]
        public async Task<IActionResult> Addappointment(ServiceAppointment entity)
        {
            var data = await _unitOfWork.Service.AddServiceAppointment(entity);
            return Ok(data);
        }

        [HttpPost("Get_services")]
        public async Task<IActionResult> GetServices([FromBody] ServiceSearchRequest request)
        {
            request ??= new ServiceSearchRequest(); 

            var data = await _unitOfWork.Service.GetAllServices(request);

            return Ok(data);
        }

        [HttpGet("Get_UserAppointments")]
        public async Task<IActionResult> GetUserAppointments( Guid userId)
        {
            var data = await _unitOfWork.Service.GetUserAppointments(userId);

            return Ok(data);
        }

        [HttpGet("service_details")]
        public async Task<IActionResult> GetServiceDetails(int serviceId)
        {
            var data = await _unitOfWork.Service.GetServiceDetails(serviceId);

            return Ok(new
            {
                ResultId = 1,
                Data = data
            });
        }

        [HttpGet("Get_BusinessAppointments")]
        public async Task<IActionResult> GetBusinessAppointments(int BusinessId)
        {
            
            var data = await _unitOfWork.Service.GetBusinessAppointmts(BusinessId);

            return Ok(data);
        }

        [HttpPost("Update_AppointmentStatus")]
        public async Task<IActionResult> UpdateAppointmentStatus([FromBody] UpdateAppointmentStatusRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var data = await _unitOfWork.Service.UpdateAppointment(request);

            return Ok(new
            {
                success = true,
                message = "Status updated successfully"
            });
        }

        //[HttpPost("Add_Service")]
        //public async Task<IActionResult> Add_Service([FromBody] AddServiceRequest request)
        //{

        //    var data = await _unitOfWork.Service.AddService(request);

        //    return Ok(data);
        //}

        [HttpPost("UploadService")]
        public async Task<IActionResult> Add_Service_With_Images([FromBody] ServiceWithImagesModel request)
        {
            if (request == null || request.Service == null)
                return BadRequest("Invalid service data");

            try
            {
                // 🔹 Save Service
                var serviceResult = await _unitOfWork.Service.AddService(request.Service);

                if (serviceResult == null || serviceResult.ResultId <= 0)
                    return BadRequest("Failed to create service");

                int newServiceId = serviceResult.ResultId; // make sure your SP returns ServiceId

                // 🔹 Save Images
                if (request.Images != null && request.Images.Any())
                {
                    string folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "Uploads",
                        "services"
                    );

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    // ✔ Only one primary image
                    if (request.Images.Count(i => i.IsPrimary) > 1)
                        return BadRequest("Only one primary image allowed");

                    foreach (var img in request.Images)
                    {
                        if (string.IsNullOrWhiteSpace(img.ImageBase64))
                            continue;

                        try
                        {
                            string base64Data = img.ImageBase64;

                            // remove base64 prefix if exists
                            if (base64Data.Contains(","))
                                base64Data = base64Data.Split(',')[1];

                            byte[] imageBytes = Convert.FromBase64String(base64Data);

                            string fileName = Guid.NewGuid() + ".jpg";
                            string filePath = Path.Combine(folderPath, fileName);

                            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                            string relativePath = $"Uploads/services/{fileName}";

                            var serviceImage = new AddOrUpdateServiceImageRequest
                            {
                                ServiceId = newServiceId,
                                ImageUrl = relativePath,
                                IsPrimary = img.IsPrimary,
                                IsActive = true
                            };

                            await _unitOfWork.Service.AddOrUpdateServiceImage(serviceImage);
                        }
                        catch
                        {
                            continue; // skip failed images
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    serviceId = newServiceId,
                    message = "Service created successfully with images"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpGet("Get_ServiceByBusinessId")]
        public async Task<IActionResult> GetServiceByBusinessId(int BusinessId)
        {
            try
            {
                // 🔹 Get Services
                var services = await _unitOfWork.Service.GetBusinessServices(BusinessId);

                var serviceList = new List<ServiceWithImagesResponse>();

                foreach (var service in services)
                {
                    // 🔹 Get Images for each service
                    var images = await _unitOfWork.Service.GetServiceImages(service.ServiceId);

                    var response = new ServiceWithImagesResponse
                    {
                        Service = service,
                        Images = new List<ServiceImageModel>()
                    };

                    if (images != null && images.Any())
                    {
                        foreach (var image in images)
                        {
                            response.Images.Add(new ServiceImageModel
                            {
                                ImageId = image.ImageId,
                                ServiceId = image.ServiceId,
                                ImageUrl = image.ImageUrl,
                                IsPrimary = image.IsPrimary
                            });
                        }
                    }

                    serviceList.Add(response);
                }

                return Ok(serviceList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error retrieving services",
                    Error = ex.Message
                });
            }
        }
    }
}
