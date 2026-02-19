using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public EventRepository(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<BaseResponse> AddUpdateEventAsync(
            AddUpdateEventRequest model,
            string imagePath)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstOrDefaultAsync<BaseResponse>(
                "sp_AddUpdateEvent",
                new
                {
                    model.EventId,
                    model.CommunityId,
                    model.CategoryId,
                    model.EventName,
                    EventImage = imagePath,
                    model.Description,
                    model.Location,
                    Latitude = model.Latitude ?? 0,
                    Longitude = model.Longitude ?? 0,
                    model.ContactName,
                    model.ContactEmail,
                    model.ContactPhone,
                    model.StartDate,
                    model.EndDate,
                    model.IsFundRaising
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<EventCategory>> GetCategoriesAsync()
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var sql = @"SELECT CategoryId, CategoryName
                FROM EventCategory
                WHERE IsActive = 1";

            return (await con.QueryAsync<EventCategory>(sql)).ToList();
        }

        public async Task<BaseResponse> PostEventToGroupsAsync(PostEventToGroupsRequest model)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstAsync<BaseResponse>(
                "sp_PostEventToGroups",
                model,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<EventListDto>> GetEventsByCommunityAsync(long communityId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return (await con.QueryAsync<EventListDto>(
                "sp_GetEventsByCommunity",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            )).ToList();
        }

        public async Task UpdateEventQRCodeAsync(
            long eventId,
            string qrPath)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await con.ExecuteAsync(
                "SP_UpdateEventQRCode",
                new
                {
                    EventID = eventId,
                    QRCodeImage = qrPath
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<BaseResponse> RegisterEventAsync(EventRegistrationRequest model)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var response = await con.QueryFirstOrDefaultAsync<BaseResponse>(
                "SP_RegisterEvent",
                new
                {
                    model.EventId,
                    model.Name,
                    model.Email,
                    model.Mobile,
                    model.NoOfAdults,
                    model.NoOfChildren,
                    model.SpecialRequest
                },
                commandType: CommandType.StoredProcedure
            );

            if (response == null || response.ResultId <= 0)
                return response;

            var request = _httpContextAccessor.HttpContext?.Request;
            var host = $"{request?.Scheme}://{request?.Host}";

            string registrationUrl =
                $"{host}/Community/Event/RegistrationDetails?id={response.ResultId}";

            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "EventQRCodes",
                response.ResultId.ToString()
            );

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(registrationUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(20);

            string filePath = Path.Combine(folderPath, "QRCode.png");
            await File.WriteAllBytesAsync(filePath, qrBytes);

            await con.ExecuteAsync(
                @"UPDATE EventRegistration 
          SET RegistrationQRCode = @QrValue
          WHERE RegistrationId = @Id",
                new { QrValue = response.ResultId.ToString(), Id = response.ResultId });

            string eventName = "Community Event"; 

            await _emailService.SendRegistrationEmailAsync(
                model.Email,
                model.Name,
                eventName,
                registrationUrl,
                filePath
            );

            return response;
        }

        public async Task<EventDto> GetEventByIdAsync(int eventId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstOrDefaultAsync<EventDto>(
                "sp_GetEventDetailsById",
                new { EventId = eventId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<EventRegistrationModel?> GetRegistrationByIdAsync(long id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var con = new SqlConnection(connectionString);

            return await con.QueryFirstOrDefaultAsync<EventRegistrationModel>(
                "SELECT * FROM EventRegistration WHERE RegistrationId = @Id",
                new { Id = id });
        }

        public async Task<IEnumerable<EventRegistrationModel>> GetRegistrationsByEventAsync(long eventId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryAsync<EventRegistrationModel>(
                @"SELECT RegistrationId, EventId, Name, Email, Mobile,
                 NoOfAdults, NoOfChildren, TotalTickets, TotalAmount,
                 PaymentStatus, BookingStatus, IsCheckedIn
          FROM EventRegistration
          WHERE EventId = @EventId
          ORDER BY CreatedDate DESC",
                new { EventId = eventId });
        }
    }
}
