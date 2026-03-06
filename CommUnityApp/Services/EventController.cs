using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using CommUnityApp.InfrastructureLayer.Repositories;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private readonly IEventRepository _repository;
        private readonly IWebHostEnvironment _env;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public EventController(IEventRepository repository, IWebHostEnvironment env, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _repository = repository;
            _env = env;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;

        }

        [HttpPost("AddUpdateEvent")]
        public async Task<IActionResult> AddUpdateEvent(
     [FromForm] AddUpdateEventRequest model)
        {
            try
            {
                string imagePath = null;

                if (model.EventImage != null)
                {
                    var folder = Path.Combine(
                        _env.WebRootPath, "Content", "events");

                    Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() +
                        Path.GetExtension(model.EventImage.FileName);

                    var fullPath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await model.EventImage.CopyToAsync(stream);

                    imagePath = "/Content/events/" + fileName;
                }

                var result = await _repository
                    .AddUpdateEventAsync(model, imagePath);

                if (result?.ResultId > 0)
                {
                    long eventId = result.ResultId;

                    string registerUrl =
     $"{Request.Scheme}://{Request.Host}/Community/Event/EventDetails?eventId={eventId}";

                    var qrFolder = Path.Combine(
                        _env.WebRootPath,
                        "Content",
                        "eventqr",
                        eventId.ToString());

                    Directory.CreateDirectory(qrFolder);

                    string qrFileName = "event_qr.png";
                    string qrFullPath = Path.Combine(qrFolder, qrFileName);

                    CommUnityApp.ApplicationCore.Models.QRCodeHelper.GenerateQRCode(
                        registerUrl,
                        qrFullPath);

                    string qrDbPath =
                        "/Content/eventqr/" +
                        eventId + "/" + qrFileName;

                    await _repository.UpdateEventQRCodeAsync(
                        eventId,
                        qrDbPath);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public static string GenerateQRCode(
        string qrText,
        string folderPath,
        string fileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fullPath = Path.Combine(folderPath, fileName);

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(
                qrText,
                QRCodeGenerator.ECCLevel.Q);

            QRCode qrCode = new QRCode(data);

            using (Bitmap bitmap = qrCode.GetGraphic(20))
            {
                bitmap.Save(fullPath, ImageFormat.Png);
            }

            return fullPath;
        }

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _repository.GetCategoriesAsync();
            return Ok(categories);
        }


        [HttpPost("PostEvent")]
        public async Task<IActionResult> PostEventToGroups(
            [FromBody] PostEventToGroupsRequest model)
        {
            var result = await _repository.PostEventToGroupsAsync(model);
            return Ok(result);
        }
        [HttpGet("GetByCommunity/{communityId}")]
        public async Task<IActionResult> GetByCommunity(long communityId)
        {
            try
            {
                var events = await _repository.GetEventsByCommunityAsync(communityId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterEvent(
         [FromBody] EventRegistrationRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await _repository.RegisterEventAsync(model));
        }
        [HttpGet("GetEventById")]
        public async Task<IActionResult> GetEventById(int eventId)
        {
            var data = await _repository.GetEventByIdAsync(eventId);
            return Ok(data);
        }
        [HttpGet("GetEventDetails/{eventId}")]
        public async Task<IActionResult> GetEventDetails(int eventId)
        {
            var result = await _repository.GetEventByIdAsync(eventId);
            return Ok(result);
        }


        [HttpGet("GetRegistrationById/{id}")]
        public async Task<IActionResult> GetRegistrationById(long id)
        {
            var registration = await _repository.GetRegistrationByIdAsync(id);

            if (registration == null)
                return NotFound();

            return Ok(registration);
        }

        [HttpGet("GetRegistrationsByEvent/{eventId}")]
        public async Task<IActionResult> GetRegistrationsByEvent(long eventId)
        {
            var registrations = await _repository.GetRegistrationsByEventAsync(eventId);
            return Ok(registrations);
        }




        [HttpGet("Get_AllEvents")]
        public async Task<IActionResult> GetAllEvents()
        {
            var data = await _unitOfWork.Events.GetEvents();
            return Ok(data);
        }

        [HttpGet("Get_Top5Events")]
        public async Task<IActionResult> GetTopEvents()
        {
            var data = await _unitOfWork.Events.GetTop5Events();
            return Ok(data);
        }

        [HttpGet("GetEventsById")]
        public async Task<IActionResult> GetEventsById(int EventId)
        {
            var data = await _unitOfWork.Events.GetEventById(EventId);
            return Ok(data);
        }


    }
}
