using CommUnityApp.BAL.Interfaces;
using CommUnityApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using QRCoder;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;

namespace CommUnityApp.DAL
{
    public class EventRepository : IEventRepository
    {
        private readonly IConfiguration _configuration;

        public EventRepository(IConfiguration configuration)
        {
            _configuration = configuration;
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
        public async Task<BaseResponse> RegisterEventAsync(
    EventRegistrationRequest model)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await con.QueryFirstOrDefaultAsync<BaseResponse>(
                "SP_RegisterEvent",
                new
                {
                    model.EventId,
                    model.Name,
                    model.Email,
                    model.Mobile,
                    model.NoOfAdults,
                    model.NoOfChildren
                },
                commandType: CommandType.StoredProcedure
            );
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

    }
}
