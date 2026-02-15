using CommUnityApp.BAL.Interfaces;
using CommUnityApp.Models;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;

namespace CommUnityApp.Services
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private readonly IEventRepository _repository;
        private readonly IWebHostEnvironment _env;

        public EventController(IEventRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
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

                    Models.QRCodeHelper.GenerateQRCode(
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


    }
}
