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
            try
            {
                request ??= new ServiceSearchRequest();

                var data = await _unitOfWork.Service.GetAllServices(request);

                var topServicesWithImages = new List<ServiceWithImagesResponse>();

                // 🔹 Loop through services
                foreach (var service in data.TopServices)
                {
                    // 🔹 Get images for each service
                    var images = await _unitOfWork.Service.GetServiceImages(service.ServiceId);

                    var response = new ServiceWithImagesResponse
                    {
                        Service = new ServiceModel
                        {
                            ServiceId = service.ServiceId,
                            BusinessId = service.BusinessId,
                            ServiceName = service.ServiceName,
                            Description = service.Description,
                            Price = service.Price,
                            DurationMinutes = service.DurationMinutes,
                            IsBookingRequired = service.IsBookingRequired,
                            IsActive = true,
                            CreatedAt = service.CreatedAt
                        },
                        Images = images?.Select(img => new ServiceImageModel
                        {
                            ImageId = img.ImageId,
                            ServiceId = img.ServiceId,
                            ImageUrl = img.ImageUrl,
                            IsPrimary = img.IsPrimary
                        }).ToList() ?? new List<ServiceImageModel>()
                    };

                    topServicesWithImages.Add(response);
                }

                // 🔥 FINAL RESPONSE
                return Ok(new
                {
                    topServices = topServicesWithImages,
                    categories = data.Categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving services",
                    error = ex.Message
                });
            }
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
            try
            {
                if (serviceId <= 0)
                    return BadRequest("Invalid ServiceId");

                // 🔹 Get service details
                var data = await _unitOfWork.Service.GetServiceDetails(serviceId);

                if (data == null || data.Service == null)
                    return NotFound(new
                    {
                        ResultId = 0,
                        Message = "Service not found"
                    });

                // 🔹 Get images
                var images = await _unitOfWork.Service.GetServiceImages(serviceId);

                // ✅ FIX: Use strongly typed list
                var imageList = images != null && images.Any()
                    ? images.Select(img => new ServiceImageModel
                    {
                        ImageId = img.ImageId,
                        ServiceId = img.ServiceId,
                        ImageUrl = img.ImageUrl,
                        IsPrimary = img.IsPrimary
                    }).ToList()
                    : new List<ServiceImageModel>();

                return Ok(new
                {
                    ResultId = 1,
                    Data = new
                    {
                        service = data.Service,
                        images = imageList
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ResultId = -1,
                    Message = "Error retrieving service details",
                    Error = ex.Message
                });
            }
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


        [HttpPost("SubCategory_Save")]
        public async Task<IActionResult> SaveSubCategory([FromBody] ServiceSubCategory entity)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (entity.CategoryId <= 0)
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "CategoryId is required"
                    });

                if (string.IsNullOrWhiteSpace(entity.SubCategoryName))
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "SubCategoryName is required"
                    });

                // 🔥 ICON HANDLING (Same as your image logic)
                if (!string.IsNullOrWhiteSpace(entity.Icon))
                {
                    string base64 = entity.Icon;

                    // ✅ REMOVE PREFIX
                    if (base64.Contains(","))
                        base64 = base64.Substring(base64.IndexOf(",") + 1);

                    byte[] fileBytes;
                    try
                    {
                        fileBytes = Convert.FromBase64String(base64);
                    }
                    catch
                    {
                        return BadRequest("Invalid icon format.");
                    }

                    // Optional: size check (2MB)
                    if (fileBytes.Length > 2097152)
                        return BadRequest("Icon size exceeds 2MB limit.");

                    string fileName = $"{Guid.NewGuid():N}.png";
                    string directoryPath = Path.Combine("wwwroot", "SubCategoryIcons");

                    Directory.CreateDirectory(directoryPath);

                    string filePath = Path.Combine(directoryPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    // Save relative path
                    entity.Icon = $"SubCategoryIcons/{fileName}";
                }

                // 🔹 Call DAL
                var result = await _unitOfWork.Service.AddServiceSubCategory(entity);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving subcategory");

                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }



        [HttpPost("Category_Save")]
        public async Task<IActionResult> SaveCategory([FromBody] BusinessCategory entity)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrWhiteSpace(entity.CategoryName))
                    return BadRequest(new
                    {
                        ResultId = 0,
                        ResultMessage = "CategoryName is required"
                    });

                // 🔥 IMAGE HANDLING (same as your User API)
                if (!string.IsNullOrWhiteSpace(entity.ImageBase64))
                {
                    string base64 = entity.ImageBase64;

                    // Remove prefix
                    if (base64.Contains(","))
                        base64 = base64.Substring(base64.IndexOf(",") + 1);

                    byte[] fileBytes;

                    try
                    {
                        fileBytes = Convert.FromBase64String(base64);
                    }
                    catch
                    {
                        return BadRequest("Invalid image format.");
                    }

                    // Max 2MB
                    if (fileBytes.Length > 2097152)
                        return BadRequest("Image size exceeds 2MB limit.");

                    string fileName = $"{Guid.NewGuid():N}.png";
                    string folderPath = Path.Combine("wwwroot", "CategoryImages");

                    Directory.CreateDirectory(folderPath);

                    string filePath = Path.Combine(folderPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    entity.ImageURL = $"CategoryImages/{fileName}";
                }

                // 🔹 Call DAL
                var result = await _unitOfWork.Service.AddBusinessCategory(entity);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving category");

                return StatusCode(500, new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }


        [HttpGet("GetServicesByCategory")]
        public async Task<IActionResult> GetServicesByCategory(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return BadRequest(new
                    {
                        resultId = 0,
                        message = "Invalid CategoryId"
                    });
                }

                var data = await _unitOfWork.Service.GetServicesByCategory(categoryId);

                // 🔥 If SubCategories exist
                if (data.SubCategories != null && data.SubCategories.Any())
                {
                    return Ok(new
                    {
                        resultId = 1,
                        type = "subcategories",
                        data = data.SubCategories
                    });
                }

                // 🔥 Else return Services WITH IMAGES
                return Ok(new
                {
                    resultId = 1,
                    type = "services",
                    data = data.Services
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    resultId = -1,
                    message = "Error retrieving category data",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetServicesBySubCategory")]
        public async Task<IActionResult> GetServicesBySubCategory(int subCategoryId)
        {
            try
            {
                if (subCategoryId <= 0)
                    return BadRequest("Invalid SubCategoryId");

                // 🔹 Get Services
                var services = await _unitOfWork.Service.GerserviceBySubcategory(subCategoryId);

                var serviceList = new List<ServiceWithImagesResponse>();

                foreach (var service in services)
                {
                    // 🔹 Get Images
                    var images = await _unitOfWork.Service.GetServiceImages(service.ServiceId);

                    var response = new ServiceWithImagesResponse
                    {
                        Service = new ServiceModel
                        {
                            ServiceId = service.ServiceId,
                            BusinessId = service.BusinessId,
                            ServiceName = service.ServiceName,
                            Description = service.Description,
                            Price = service.Price,
                            DurationMinutes = service.DurationMinutes,
                            IsBookingRequired = service.IsBookingRequired,
                            IsActive = true,
                            CreatedAt = service.CreatedAt
                        },
                        Images = images?.Select(img => new ServiceImageModel
                        {
                            ImageId = img.ImageId,
                            ServiceId = img.ServiceId,
                            ImageUrl = img.ImageUrl,
                            IsPrimary = img.IsPrimary
                        }).ToList() ?? new List<ServiceImageModel>()
                    };

                    serviceList.Add(response);
                }

                return Ok(new
                {
                    resultId = 1,
                    data = serviceList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    resultId = -1,
                    message = "Error retrieving services",
                    error = ex.Message
                });
            }
        }
    }
}
