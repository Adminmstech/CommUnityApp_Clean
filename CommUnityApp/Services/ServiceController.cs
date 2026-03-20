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
    }
}
