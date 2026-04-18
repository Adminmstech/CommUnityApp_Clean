using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using CommUnityApp.InfrastructureLayer.Repositories;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QRCoder;
using System.Drawing;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("BookEvent")]
        public async Task<IActionResult> BookEvent([FromBody] EventBookingRequest request)
        {
            var result = await _repository.BookEventAsync(request);

            if (result.Status == 1)
            {
                return Ok(new
                {
                    status = result.Status,
                    message = result.Message,
                    transactionId = result.TransactionId,  
                    userId = result.UserId
                });
            }

            return BadRequest(new
            {
                status = result.Status,
                message = result.Message
            });
        }
        [HttpGet("GetEventDetails")]
        public async Task<IActionResult> GetEventDetails(Guid userId, int eventId)
        {
            var result = await _repository.GetEventDetailsAsync(userId, eventId);

            if (result == null)
            {
                return NotFound(new
                {
                    status = 0,
                    message = "Event not found"
                });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            if (!string.IsNullOrEmpty(result.EventImage))
            {
                result.EventImage = baseUrl + result.EventImage;
            }

            return Ok(new
            {
                status = 1,
                data = result
            });
        }

        [HttpGet("GetUserEventBookings")]
        public async Task<IActionResult> GetUserBookings(Guid userId)
        {
            var result = await _repository.GetUserBookingsAsync(userId);

            if (result == null || !result.Any())
            {
                return NotFound(new
                {
                    status = 0,
                    message = "No bookings found"
                });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var item in result)
            {
                if (!string.IsNullOrEmpty(item.EventImage))
                {
                    item.EventImage = baseUrl + item.EventImage;
                }
            }

            return Ok(new
            {
                status = 1,
                data = result
            });
        }
        [HttpGet("GetEventCheckoutSummaryByTransaction")]
        public async Task<IActionResult> GetEventCheckoutSummaryByTransaction(Guid transactionId)
        {
            var result = await _repository.GetEventCheckoutAsync(transactionId);

            if (result == null || result.Status == 0)
            {
                return NotFound(new
                {
                    status = 0,
                    message = "Invalid TransactionId"
                });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            if (!string.IsNullOrEmpty(result.EventImage))
            {
                result.EventImage = baseUrl + result.EventImage;
            }

            return Ok(new
            {
                status = 1,
                data = result
            });
        }

        [HttpPost("AddEventPayment")]
        public async Task<IActionResult> AddEventPayment([FromBody] EventPaymentRequest request)
        {
            var result = await _repository.AddEventPaymentAsync(request);

            if (result.Status == 1)
            {
                return Ok(new
                {
                    status = result.Status,
                    message = result.Message,
                    bookingId = result.BookingId,
                    transactionId = result.TransactionId
                });
            }

            return BadRequest(new
            {
                status = result.Status,
                message = result.Message
            });
        }

        [HttpGet("GetEventCheckoutSummaryByUser")]
        public async Task<IActionResult> GetEventCheckoutSummary(Guid userId)
        {
            var result = await _repository.GetEventCheckoutSummaryAsync(userId);

            if (result == null)
            {
                return NotFound(new
                {
                    status = 0,
                    message = "No booking found"
                });
            }

            return Ok(new
            {
                status = 1,
                data = result
            });
        }
        [HttpPost("AddSponsor")]
        public async Task<IActionResult> AddSponsor([FromForm] EventSponsorModel model)
        {
            try
            {
                var communityId = HttpContext.Session.GetString("CommunityId");

                if (string.IsNullOrEmpty(communityId))
                    return Unauthorized("CommunityId not found in session");

                model.CommunityId = Convert.ToInt32(communityId);

                string logoPath = "";

                if (model.LogoFile != null)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/sponsors");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(stream);
                    }

                    logoPath = "/uploads/sponsors/" + fileName;
                }

                var id = await _repository.AddEventSponsor(model, logoPath);

                return Ok(new { message = "Sponsor Added Successfully", sponsorId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetSponsors")]
        public async Task<IActionResult> GetSponsors(int eventId)
        {
            var data = await _repository.GetEventSponsors(eventId);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.LogoPath))
                    item.LogoPath = baseUrl + item.LogoPath;
            }

            return Ok(data);
        }

        [HttpGet("GetAllEventSponsors")]
        public async Task<IActionResult> GetAllSponsors()
        {
            var data = await _repository.GetAllSponsors();

            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.LogoPath))
                    item.LogoPath = baseUrl + item.LogoPath;
                else
                    item.LogoPath = baseUrl + "/images/noimage.png";
            }

            return Ok(data);
        }
        [HttpGet("GetSponsorsByCommunity")]
        public async Task<IActionResult> GetSponsorsByCommunity()
        {
            try
            {
                var communityId = HttpContext.Session.GetString("CommunityId");

                if (string.IsNullOrEmpty(communityId))
                    return Unauthorized("Community not logged in");

                int cid = Convert.ToInt32(communityId);

                var sponsors = await _repository.GetSponsorsByCommunity(cid);

                return Ok(sponsors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("AttachSponsorToEvent")]
        public async Task<IActionResult> AttachSponsorToEvent([FromBody] EventSponsorMappingModel model)
        {
            try
            {
                if (model.EventId == 0 || model.SponsorId == 0)
                    return BadRequest("Invalid data");

                await _repository.AttachSponsorToEvent(model);

                return Ok("Sponsor linked successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetEventsByCommunity")]
        public async Task<IActionResult> GetEventsByCommunity(int communityId)
        {
            var events = await _repository.GetEventsByCommunity(communityId);
            return Ok(events);
        }

        [HttpGet("GetEventDetailsWithSponsors")]
        public async Task<IActionResult> GetEventDetailsWithSponsors(int eventId)
        {
            var data = await _repository.GetEventDetailsWithSponsors(eventId);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Event Image
            if (!string.IsNullOrEmpty(data.EventImage))
            {
                data.EventImage = baseUrl + "/uploads/Events/" + data.EventImage;
            }

            // Sponsor Logos
            foreach (var s in data.Sponsors)
            {
                if (!string.IsNullOrEmpty(s.LogoPath))
                {
                    s.LogoPath = baseUrl + "/uploads/sponsors/" + s.LogoPath;
                }
            }

            return Ok(data);
        }

        [HttpGet("GetSponsorsByEvent")]
        public async Task<IActionResult> GetSponsorsByEvent(int eventId)
        {
            try
            {
                var data = await _repository.GetSponsorsByEvent(eventId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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

        [HttpGet("Get_EventDetails")]
        public async Task<IActionResult> GetEventsById(int EventId)
        {
            var data = await _unitOfWork.Events.GetEventById(EventId);
            return Ok(data);
        }


    }
    }

