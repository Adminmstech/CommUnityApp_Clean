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


        public async Task<BookingResponse> BookEventAsync(EventBookingRequest request)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.UserId);
                parameters.Add("@EventId", request.EventId);
                parameters.Add("@NoOfTickets", request.NoOfTickets);
                parameters.Add("@UseWallet", request.UseWallet);

                var result = await con.QueryFirstOrDefaultAsync<BookingResponse>(
                    "SP_BookEvent",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }


        public async Task<EventDetailsResponse> GetEventDetailsAsync(Guid userId, int eventId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@EventId", eventId);

                var result = await con.QueryFirstOrDefaultAsync<EventDetailsResponse>(
                    "SP_GetEventDetailsWithUserWallet",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }

        public async Task<IEnumerable<UserBookingResponse>> GetUserBookingsAsync(Guid userId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                var result = await con.QueryAsync<UserBookingResponse>(
                    "SP_GetUserBookings",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }

        public async Task<EventCheckoutResponse> GetEventCheckoutAsync(Guid transactionId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TransactionId", transactionId);

                var result = await con.QueryFirstOrDefaultAsync<EventCheckoutResponse>(
                    "SP_EventCheckoutSummary",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }

        public async Task<dynamic> AddEventPaymentAsync(EventPaymentRequest request)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.UserId);
                parameters.Add("@TransactionId", request.TransactionId);
                parameters.Add("@Amount", request.Amount);
                parameters.Add("@PaymentMethod", request.PaymentMethod);
                parameters.Add("@PaymentGatewayId", request.PaymentGatewayId);
                parameters.Add("@PaymentStatus", request.PaymentStatus);

                var result = await con.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_AddEventPaymentTransaction",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }
        public async Task<EventCheckoutSummaryResponse> GetEventCheckoutSummaryAsync(Guid userId,int eventId,int ticketTypeId,int quantity, bool useWallet)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var parameters = new DynamicParameters();

                parameters.Add("@UserId", userId);
                parameters.Add("@EventId", eventId);
                parameters.Add("@TicketTypeId", ticketTypeId);
                parameters.Add("@Quantity", quantity);
                parameters.Add("@UseWallet", useWallet);

                var result = await con.QueryFirstOrDefaultAsync<EventCheckoutSummaryResponse>(
                    "SP_EventCheckoutSummary_ByUser",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }
        public async Task<int> AddEventSponsor(EventSponsorModel model, string logoPath)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var id = await con.ExecuteScalarAsync<int>(
                    "sp_AddEventSponsor",
                   new
                   {
                       model.EventId,
                       model.SponsorName,
                       model.Mobile,
                       model.Email,
                       model.Amount,
                       model.SponsorType,
                       model.OtherInfo,
                       model.CommunityId,
                       LogoPath = logoPath
                   },
                    commandType: CommandType.StoredProcedure);

                return id;
            }
        }

        public async Task<List<EventSponsorModel>> GetEventSponsors(int eventId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var list = await con.QueryAsync<EventSponsorModel>(
                    "sp_GetEventSponsors",
                    new { EventId = eventId },
                    commandType: CommandType.StoredProcedure);

                return list.ToList();
            }
        }
        public async Task<List<EventSponsorListModel>> GetAllSponsors()
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var list = await con.QueryAsync<EventSponsorListModel>(
                    "sp_GetAllEventSponsors",
                    commandType: CommandType.StoredProcedure);

                return list.ToList();
            }
        }
        public async Task<List<EventSponsorModel>> GetSponsorsByCommunity(int communityId)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await con.QueryAsync<EventSponsorModel>(
                "sp_GetSponsorsByCommunity",
                new { CommunityId = communityId },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
        public async Task<List<Events>> GetEvents()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<Events>("Get_AllEvents", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }


        public async Task<List<TopEventDto>> GetTop5Events()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<TopEventDto>("Get_Top5Events", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<Events>> GetEventById(int EventId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@EventId", EventId);

            var result = await connection.QueryAsync<Events>("Get_EventById", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

       

        public async Task AttachSponsorToEvent(EventSponsorMappingModel model)
        {
            var sql = @"INSERT INTO EventSponsorMapping 
                (EventId, SponsorId, Amount, SponsorType, CreatedDate)
                VALUES (@EventId, @SponsorId, @Amount, @SponsorType, GETDATE())";

            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.ExecuteAsync(sql, model);
            }
        }
        public async Task<List<EventModel>> GetEventsByCommunity(int communityId)
        {
            var sql = @"SELECT EventId, EventName 
                FROM Events 
                WHERE CommunityId = @CommunityId
                ORDER BY EventId DESC";

            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<EventModel>(sql, new { CommunityId = communityId });
                return result.ToList();
            }
        }

        public async Task<EventDetailsModel> GetEventDetailsWithSponsors(int eventId)
        {
            EventDetailsModel eventData = null;

            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<EventDetailsModel, SponsorModel, EventDetailsModel>(
                    "sp_GetEventDetailsWithSponsors",
                    (eventObj, sponsorObj) =>
                    {
                        if (eventData == null)
                        {
                            eventData = eventObj;
                            eventData.Sponsors = new List<SponsorModel>();
                        }

                        if (sponsorObj != null && sponsorObj.SponsorId != 0)
                        {
                            eventData.Sponsors.Add(sponsorObj);
                        }

                        return eventObj;
                    },
                    new { EventId = eventId },
                    splitOn: "SponsorId",
                    commandType: CommandType.StoredProcedure
                );
            }

            return eventData;
        }

        public async Task<List<SponsorModel>> GetSponsorsByEvent(int eventId)
        {
            var sql = @"SELECT SponsorId, SponsorName, Amount, SponsorType, LogoPath
                FROM EventSponsors
                WHERE EventId = @EventId";

            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<SponsorModel>(sql, new { EventId = eventId });
                return result.ToList();
            }
        }

        public async Task<dynamic> PostEventToCommunityUsers(PostEventModel model)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_PostEventToCommunityUsers",
                    new
                    {
                        EventId = model.EventId,
                        CommunityId = model.CommunityId
                    },
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }
        public async Task<List<UserModel>> GetUsersByCommunityId(int communityId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<UserModel>(
                    "sp_GetUsersByCommunity",
                    new
                    {
                        CommunityId = communityId
                    },
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
        }

        public async Task<List<UserPostedEventModel>> GetPostedEventsByUser(Guid userId)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await con.QueryAsync<UserPostedEventModel>(
                    "sp_GetPostedEventsByUser",
                    new
                    {
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
        }

        public async Task<dynamic> AddBookEvent(BookEventRequest model)
        {

            using (var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();

                using (var transaction = con.BeginTransaction())
                {
                    try
                    {


                        DynamicParameters param = new DynamicParameters();

                        param.Add("@UserId", model.UserId);
                        param.Add("@EventId", model.EventId);
                        param.Add("@TicketTypeId", model.TicketTypeId);
                        param.Add("@Quantity", model.Quantity);
                        param.Add("@UseWallet", model.UseWallet);
                        param.Add("@PaymentMethod", model.PaymentMethod);
                        param.Add("@TransactionId", model.TransactionId);

                        var bookingResult = await con.QueryFirstOrDefaultAsync<dynamic>(
                            "SP_EventBookTickets",
                            param,
                            transaction: transaction,
                            commandType: CommandType.StoredProcedure);

                        if (bookingResult == null || bookingResult.Status == 0)
                        {
                            transaction.Rollback();

                            return new
                            {
                                status = 0,
                                message = "Booking failed"
                            };
                        }

                        int bookingId = bookingResult.BookingId;



                        var tickets = (await con.QueryAsync<dynamic>(
                            @"SELECT 
                        TicketId,
                        TicketCode
                      FROM EventTickets
                      WHERE BookingId=@BookingId",
                            new { BookingId = bookingId },
                            transaction)).ToList();



                        /*foreach (var ticket in tickets)
                        {
                            string qrPath = await GenerateQRCode(
                                ticket.TicketCode.ToString(),
                                Convert.ToInt32(ticket.TicketId));

                            await con.ExecuteAsync(
                                @"UPDATE EventTickets
                          SET QRCodePath=@QRCodePath
                          WHERE TicketId=@TicketId",
                                new
                                {
                                    QRCodePath = qrPath,
                                    TicketId = ticket.TicketId
                                },
                                transaction);
                        }*/
                        var ticket = tickets.FirstOrDefault();

                        if (ticket != null)
                        {
                            string qrPath = await GenerateQRCode(
                                ticket.TicketCode.ToString(),
                                Convert.ToInt32(ticket.TicketId));

                            await con.ExecuteAsync(
                                @"UPDATE EventTickets
          SET QRCodePath=@QRCodePath
          WHERE TicketId=@TicketId",
                                new
                                {
                                    QRCodePath = qrPath,
                                    TicketId = ticket.TicketId
                                },
                                transaction);
                        }

                        transaction.Commit();



                        return new
                        {
                            status = 1,
                            message = "Booking successful",
                            bookingId = bookingId,
                            payableAmount = bookingResult.PayableAmount,
                            rewardCoins = bookingResult.RewardCoins
                        };
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        return new
                        {
                            status = 0,
                            message = ex.Message
                        };
                    }
                }
            }
        }

        public async Task<UserTicketDetailsResponse> GetUserTicketDetails(int ticketId)
        {
            using (var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();

                DynamicParameters param = new DynamicParameters();

                param.Add("@TicketId", ticketId);

                using (var multi = await con.QueryMultipleAsync(
                    "SP_GetUserTicketDetails",
                    param,
                    commandType: CommandType.StoredProcedure))
                {
                    var ticket = await multi.ReadFirstOrDefaultAsync<TicketDetails>();

                    var sponsors = (await multi.ReadAsync<SponsorResponse>()).ToList();

                    return new UserTicketDetailsResponse
                    {
                        Status = 1,
                        TicketDetails = ticket,
                        Sponsors = sponsors
                    };
                }
            }
        }

        /*   public async Task<string> GenerateQRCode(string ticketCode, int ticketId)
           {
               try
               {
                   string rootPath = Path.Combine(
                       Directory.GetCurrentDirectory(),
                       "wwwroot");

                   string folderPath = Path.Combine(
                       rootPath,
                       "Uploads",
                       "EventTickets",
                       ticketId.ToString());

                   if (!Directory.Exists(folderPath))
                   {
                       Directory.CreateDirectory(folderPath);
                   }

                   string fileName = "qr.png";

                   string fullPath = Path.Combine(folderPath, fileName);

                   using (QRCodeGenerator qrGenerator =
                       new QRCodeGenerator())
                   {
                       // QR URL
                       string qrContent =
                           $"https://localhost:7176/api/Event/CheckInTicket?ticketCode={ticketCode}";

                       QRCodeData qrCodeData =
                           qrGenerator.CreateQrCode(
                               qrContent,
                               QRCodeGenerator.ECCLevel.Q);

                       using (QRCode qrCode =
                           new QRCode(qrCodeData))
                       {
                           using (Bitmap qrCodeImage =
                               qrCode.GetGraphic(20))
                           {
                               qrCodeImage.Save(
                                   fullPath,
                                   ImageFormat.Png);
                           }
                       }
                   }

                   return "/Uploads/EventTickets/"
                          + ticketId +
                          "/qr.png";
               }
               catch (Exception ex)
               {
                   throw new Exception(ex.Message);
               }
           }*/
        public async Task<string> GenerateQRCode(string ticketCode, int ticketId)
        {
            try
            {
                string rootPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");

                string folderPath = Path.Combine(
                    rootPath,
                    "Uploads",
                    "EventTickets",
                    ticketId.ToString());

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = "qr.png";

                string fullPath = Path.Combine(folderPath, fileName);

                using (QRCodeGenerator qrGenerator =
                    new QRCodeGenerator())
                {
                    string qrContent =
                        $"https://localhost:7176/api/Event/CheckInTicket?ticketCode={ticketCode}";

                    QRCodeData qrCodeData =
                        qrGenerator.CreateQrCode(
                            qrContent,
                            QRCodeGenerator.ECCLevel.Q);

                    using (QRCode qrCode =
                        new QRCode(qrCodeData))
                    {
                        using (Bitmap qrCodeImage =
                            qrCode.GetGraphic(20))
                        {
                            qrCodeImage.Save(
                                fullPath,
                                ImageFormat.Png);
                        }
                    }
                }

                return "/Uploads/EventTickets/"
                       + ticketId +
                       "/qr.png";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<SponsorDetailsResponse> GetSponsorDetailsById(int sponsorId)
        {
            using (var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();

                DynamicParameters param = new DynamicParameters();

                param.Add("@SponsorId", sponsorId);

                var result = await con.QueryFirstOrDefaultAsync<SponsorResponse>(
                    "SP_GetSponsorDetailsById",
                    param,
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                {
                    return new SponsorDetailsResponse
                    {
                        Status = 0,
                        Message = "Sponsor not found",
                        SponsorDetails = null
                    };
                }

                return new SponsorDetailsResponse
                {
                    Status = 1,
                    Message = "Success",
                    SponsorDetails = result
                };
            }
        }

        public async Task<dynamic> GetBookedTicketsByUserId(Guid userId)
        {
            using (var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();

                DynamicParameters param = new DynamicParameters();

                param.Add("@UserId", userId);

                var result = (await con.QueryAsync<dynamic>(
                    "SP_GetBookedTicketsByUserId",
                    param,
                    commandType: CommandType.StoredProcedure))
                    .ToList();

                return new
                {
                    status = 1,
                    tickets = result
                };
            }
        }

        public async Task<dynamic> CheckInTicket(string ticketCode)
        {
            using (var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                DynamicParameters param = new DynamicParameters();

                param.Add("@TicketCode", ticketCode);

                var result = await con.QueryFirstOrDefaultAsync(
                    "SP_EventTicketCheckIn",
                    param,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }
    }
}
