using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;
using Stripe.Checkout;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly ILogger<AuctionController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IHubContext<AuctionHub> _hubContext;

        public AuctionController(ILogger<AuctionController> logger, IUnitOfWork unitOfWork, IConfiguration config, IHubContext<AuctionHub> hubContext)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;
            _hubContext = hubContext;
        }

        //[HttpPost("Add_Auction")]
        //public async Task<IActionResult> AddAuction(Auction entity)
        //{
        //    var data = await _unitOfWork.Auction.SaveAuction(entity);
        //    return Ok(data);
        //}

        [HttpGet("Get_ItemType")]
        public async Task<IActionResult> GetItemItemtype()
        {
            var data = await _unitOfWork.Auction.GetItemType();
            return Ok(data);
        }

        //[ProducesResponseType(400)]
        //[ProducesResponseType(500)]
        //[ProducesResponseType(typeof(int), 200)]
        //[HttpPost("Add_AuctionItemImages")]
        //public async Task<IActionResult> AddAuctionItemImages(AuctionItemImage C)
        //{
        //    var data = await _unitOfWork.Auction.SaveAuctionItemImage(C);
        //    return Ok(data);
        //}

        //[HttpGet("Get_Auctions")]
        //public async Task<IActionResult> GetAuctions()
        //{
        //    var data = await _unitOfWork.Auction.GetAuctions();
        //    return Ok(data);
        //}
        //[HttpGet("Get_AuctionById")]
        //public async Task<IActionResult> GetAuctionById(int AuctionId)
        //{
        //    var data = await _unitOfWork.Auction.GetAuctionAuctionId(AuctionId);
        //    return Ok(data);
        //}

        //[HttpGet("Get_AuctionItemTypeById")]
        //public async Task<IActionResult> GetAuctionByItemTypeId(int ItemTypeId)
        //{
        //    var data = await _unitOfWork.Auction.GetAuctionByItemTypeId(ItemTypeId);
        //    return Ok(data);
        //}


        //[HttpGet("Get_AuctionImagesByAuctionId")]
        //public async Task<IActionResult> GetAuctionImages( int AuctionId )
        //{
        //    var data = await _unitOfWork.Auction.GetAuctionImages(AuctionId);
        //    return Ok(data);
        //}

        [HttpPost("UploadAndUpdateAuction")]
        public async Task<IActionResult> Add_Auction_With_Images([FromBody] AuctionWithImagesRequest request)
        {
            if (request == null || request.Auction == null)
                return BadRequest("Invalid auction data");

            if (request.Auction.StartTime >= request.Auction.EndTime)
                return BadRequest("EndTime must be greater than StartTime");

            try
            {
                bool isUpdate = request.Auction.AuctionId > 0;

                // Save Auction (Insert/Update)
                var auctionResult = await _unitOfWork.Auction.SaveAuction(request.Auction);

                if (auctionResult == null || auctionResult.ResultId <= 0)
                    return BadRequest(isUpdate
                        ? "Failed to update auction"
                        : "Failed to create auction");

                int auctionId = isUpdate
                    ? request.Auction.AuctionId
                    : auctionResult.ResultId;

                // Process images only if images are supplied
                if (request.Images != null && request.Images.Any())
                {
                    // Allow only one primary image
                    if (request.Images.Count(x => x.IsPrimary) > 1)
                        return BadRequest("Only one primary image is allowed.");

                    string folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "Uploads",
                        "auctions");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    // Update: Remove old images from database
                    if (isUpdate)
                    {
                        var oldImages = await _unitOfWork.Auction
                            .GetAuctionImagesByAuctionId(auctionId);

                        if (oldImages != null)
                        {
                            foreach (var oldImage in oldImages)
                            {
                                try
                                {
                                    string physicalPath = Path.Combine(
                                        Directory.GetCurrentDirectory(),
                                        "wwwroot",
                                        oldImage.ImageUrl.Replace("/", "\\"));

                                    if (System.IO.File.Exists(physicalPath))
                                    {
                                        System.IO.File.Delete(physicalPath);
                                    }
                                }
                                catch
                                {
                                    // Ignore file delete errors
                                }
                            }
                        }

                        await _unitOfWork.Auction.DeleteAuctionImages(auctionId);
                    }

                    // Save new images
                    foreach (var img in request.Images)
                    {
                        if (string.IsNullOrWhiteSpace(img.ImageBase64))
                            continue;

                        try
                        {
                            string base64 = img.ImageBase64;

                            if (base64.Contains(","))
                                base64 = base64.Split(',')[1];

                            byte[] bytes = Convert.FromBase64String(base64);

                            string fileName = Guid.NewGuid() + ".jpg";

                            string filePath = Path.Combine(folderPath, fileName);

                            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                            string relativePath = $"Uploads/auctions/{fileName}";

                            var auctionImage = new AuctionItemImage
                            {
                                AuctionId = auctionId,
                                ImageUrl = relativePath,
                                ImageName = fileName,
                                IsPrimary = img.IsPrimary,
                                CreatedDate = DateTime.UtcNow
                            };

                            await _unitOfWork.Auction
                                .SaveAuctionItemImage(auctionImage);
                        }
                        catch
                        {
                            // Skip invalid image
                            continue;
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    auctionId = auctionId,
                    message = isUpdate
                        ? "Auction updated successfully."
                        : "Auction created successfully."
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

        [HttpGet("Get_Top5Auctions")]
        public async Task<IActionResult> GetAuctions()
        {
            // Step 1: Get all auctions
            var auctions = await _unitOfWork.Auction.GetTop5Auctions();

            // Step 2: Create list
            var combinedAuctions = new List<AuctionWithImagesModel>();

            // Step 3: Loop auctions
            foreach (var auction in auctions)
            {
                var images = await _unitOfWork.Auction.GetAuctionImages(auction.AuctionId);

                var auctionResponse = new AuctionWithImagesModel
                {
                    AuctionId = auction.AuctionId,
                    BusinessId = auction.BusinessId,
                    UserId = auction.UserId,
                    User = auction.User,
                    ItemTypeId = auction.ItemTypeId,
                    ItemTitle = auction.ItemTitle,
                    ItemDescription = auction.ItemDescription,
                    ItemCondition = auction.ItemCondition,
                    PriceIncrement = auction.PriceIncrement,
                    ReservePrice = auction.ReservePrice,
                    MinDeposite = auction.MinDeposite,
                    StartTime = auction.StartTime,
                    EndTime = auction.EndTime,
                    ItemLocation = auction.ItemLocation,
                    DeleveryMethodId = auction.DeleveryMethodId,
                    AuctionStatus = auction.AuctionStatus,
                    CreatedBy = auction.CreatedBy,
                    RegistrationRequired=auction.RegistrationRequired,
                    CreatedAt = auction.CreatedAt,

                    // ✅ FIXED HERE
                    Images = images
                };

                combinedAuctions.Add(auctionResponse);
            }

            return Ok(combinedAuctions);
        }


        [HttpGet("Get_Auctions")]
        public async Task<IActionResult> GetAuctionsWithImages()
        {
            // Step 1: Get all auctions
            var auctions = await _unitOfWork.Auction.GetAuctions();

            // Step 2: Create list
            var combinedAuctions = new List<AuctionWithImagesModel>();

            // Step 3: Loop auctions
            foreach (var auction in auctions)
            {
                var images = await _unitOfWork.Auction.GetAuctionImages(auction.AuctionId);

                var auctionResponse = new AuctionWithImagesModel
                {
                    AuctionId = auction.AuctionId,
                    BusinessId = auction.BusinessId,
                    UserId = auction.UserId,
                    User = auction.User,
                    ItemTypeId = auction.ItemTypeId,
                    ItemTitle = auction.ItemTitle,
                    ItemDescription = auction.ItemDescription,
                    ItemCondition = auction.ItemCondition,
                    PriceIncrement = auction.PriceIncrement,
                    ReservePrice = auction.ReservePrice,
                    MinDeposite = auction.MinDeposite,
                    StartTime = auction.StartTime,
                    EndTime = auction.EndTime,
                    ItemLocation = auction.ItemLocation,
                    DeleveryMethodId = auction.DeleveryMethodId,
                    AuctionStatus = auction.AuctionStatus,
                    CreatedBy = auction.CreatedBy,
                    CreatedAt = auction.CreatedAt,

                    // ✅ FIXED HERE
                    Images = images
                };

                combinedAuctions.Add(auctionResponse);
            }

            return Ok(combinedAuctions);
        }


        [HttpGet("Get_AuctionByAuctionId")]
        public async Task<IActionResult> AuctionByIdWithImages_Edit(int AuctionId, Guid? userId = null)
        {
            // Step 1: Get auction(s)
            var auctions = await _unitOfWork.Auction.GetAuctionAuctionId(AuctionId, userId = null);

            var combinedAuctions = new List<AuctionWithImagesModel>();

            foreach (var auction in auctions)
            {
                // Step 2: Get images
                var images = await _unitOfWork.Auction.GetAuctionImages(auction.AuctionId);

                // Step 3: Combine
                var auctionDetails = new AuctionWithImagesModel
                {
                    AuctionId = auction.AuctionId,
                    BusinessId = auction.BusinessId,
                    UserId = auction.UserId,
                    User = auction.User,
                    ItemTypeId = auction.ItemTypeId,
                    ItemTitle = auction.ItemTitle,
                    ItemDescription = auction.ItemDescription,
                    ItemCondition = auction.ItemCondition,
                    PriceIncrement = auction.PriceIncrement,
                    ReservePrice = auction.ReservePrice,
                    MinDeposite = auction.MinDeposite,
                    StartTime = auction.StartTime,
                    EndTime = auction.EndTime,
                    ItemLocation = auction.ItemLocation,
                    DeleveryMethodId = auction.DeleveryMethodId,
                    AuctionStatus = auction.AuctionStatus,
                    CreatedBy = auction.CreatedBy,
                    IsRegistered = auction.IsRegistered,
                    CreatedAt = auction.CreatedAt,

                    // ✅ FIXED HERE
                    Images = images
                };

                combinedAuctions.Add(auctionDetails);
            }

            return Ok(combinedAuctions);
        }

        [HttpGet("Get_AuctionByCategoryId")]
        public async Task<IActionResult> AuctionByCategoryIdWithImages(int itemTypeId)
        {
            if (itemTypeId <= 0)
                return BadRequest("Invalid ItemTypeId");

            // Step 1: Get auctions by category
            var auctions = await _unitOfWork.Auction.GetAuctionByItemTypeId(itemTypeId);

            if (auctions == null || !auctions.Any())
                return NotFound("No auctions found for this category");

            var combinedAuctions = new List<AuctionWithImagesModel>();

            // Step 2: Loop auctions
            foreach (var auction in auctions)
            {
                // Step 3: Get images for each auction
                var images = await _unitOfWork.Auction.GetAuctionImages(auction.AuctionId);

                // Step 4: Combine auction + images
                combinedAuctions.Add(new AuctionWithImagesModel
                {
                    AuctionId = auction.AuctionId,
                    BusinessId = auction.BusinessId,
                    UserId = auction.UserId,
                    User = auction.User,
                    ItemTypeId = auction.ItemTypeId,
                    ItemTitle = auction.ItemTitle,
                    ItemDescription = auction.ItemDescription,
                    ItemCondition = auction.ItemCondition,
                    PriceIncrement = auction.PriceIncrement,
                    ReservePrice = auction.ReservePrice,
                    MinDeposite = auction.MinDeposite,
                    StartTime = auction.StartTime,
                    EndTime = auction.EndTime,
                    ItemLocation = auction.ItemLocation,
                    DeleveryMethodId = auction.DeleveryMethodId,
                    AuctionStatus = auction.AuctionStatus,
                    CreatedBy = auction.CreatedBy,
                    CreatedAt = auction.CreatedAt,
                    Images = images
                });
            }

            return Ok(combinedAuctions);
        }

        [HttpPost("PlaceBid")]
        public async Task<IActionResult> PlaceBid(PlaceBidRequest entity)
        {
            var result = await _unitOfWork.Auction.PlaceBid(entity);

            if (result.ResultId == 1)
            {
                // Send latest bid info
                await _hubContext.Clients
                    .Group(entity.AuctionId.ToString())
                    .SendAsync("ReceiveBid", new
                    {
                        userName = result.UserName,
                        bidAmount = entity.BidAmount
                    });

                // Send updated recent bids list
                var recentBids = await _unitOfWork.Auction
                    .GetRecentBids(entity.AuctionId);

                await _hubContext.Clients
                    .Group(entity.AuctionId.ToString())
                    .SendAsync(
                        "RecentBidsUpdated",
                        recentBids);
            }

            return Ok(result);
        }

        [HttpGet("GetRecentBids")]
        public async Task<IActionResult> GetRecentBids(int auctionId)
        {
            var data = await _unitOfWork.Auction.GetRecentBids(auctionId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Data = data
            });
        }


        [HttpPost("Add_BidRegistration")]
        public async Task<IActionResult> BidRegistration([FromBody] BidRegistration entity)
        {
            var data = await _unitOfWork.Auction.SaveBidRegistration(entity);
            return Ok(data);
        }

        //[HttpPost("create-checkout-session-dynamic")]
        //public async Task<IActionResult> CreateCheckoutSessionDynamic([FromBody] CheckoutRequestModel model)
        //{
        //    if (model == null || model.Price <= 0)
        //        return BadRequest("Invalid checkout request.");

        //    var options = new SessionCreateOptions
        //    {
        //        PaymentMethodTypes = new List<string> { "card" },

        //        LineItems = new List<SessionLineItemOptions>
        //{
        //    new SessionLineItemOptions
        //    {
        //        PriceData = new SessionLineItemPriceDataOptions
        //        {
        //            Currency = "aud",
        //            ProductData = new SessionLineItemPriceDataProductDataOptions
        //            {
        //                Name = "Auction Bid Registration",
        //            },
        //            UnitAmount = (long)(model.Price * 100),
        //        },
        //        Quantity = 1,
        //    }
        //},

        //        Mode = "payment",

        //        SuccessUrl = "https://indocommunity.com/business/home/auctions",
        //        CancelUrl = "https://indocommunity.com/business/home/auctions",

        //        Metadata = new Dictionary<string, string>
        //{
        //    { "user_id", model.UserId },
        //    { "auction_id", model.AuctionId.ToString() }
        //}
        //    };

        //    var service = new SessionService();
        //    var session = await service.CreateAsync(options);

        //    return Ok(new { url = session.Url });
        //}
        [HttpPost("create-checkout-session-dynamic")]
        public async Task<IActionResult> CreateCheckoutSessionDynamic([FromBody] CheckoutRequestModel model)
        {
            if (model == null || model.Price <= 0)
                return BadRequest("Invalid checkout request.");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },

                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "aud",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Auction Bid Registration",
                    },
                    UnitAmount = (long)(model.Price * 100),
                },
                Quantity = 1,
            }
        },

                Mode = "payment",

                SuccessUrl = "https://indocommunity.com/business/home/auctions",
                CancelUrl = "https://indocommunity.com/business/home/auctions",

                Metadata = new Dictionary<string, string>
        {
            { "user_id", model.UserId },
            { "auction_id", model.AuctionId.ToString() }
        }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // ✅ SAVE PENDING REGISTRATION
            var entity = new BidRegistration
            {
                BidRegistrationId = Guid.NewGuid(),
                AuctionId = model.AuctionId,
                UserId = Guid.Parse(model.UserId),
                PaymentId = session.Id,   // Save Stripe SessionId
                PaymentStatusId = 1       // 0 = Pending,1= paid for now , will update to 1 when webhook received
            };

            await _unitOfWork.Auction.SaveBidRegistration(entity);

            return Ok(new { url = session.Url });
        }

        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            HttpContext.Request.EnableBuffering();

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var endpointSecret = _config["Stripe:WebhookSecret"];

            if (string.IsNullOrEmpty(stripeSignature))
                return BadRequest("Missing Stripe signature.");

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    endpointSecret,
                    throwOnApiVersionMismatch: false
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe webhook validation failed.");
                return BadRequest("Invalid webhook.");
            }

            // =====================================================
            // HANDLE CHECKOUT COMPLETED
            // =====================================================
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;

                if (session == null || session.PaymentStatus != "paid")
                    return Ok(); // Not paid

                Guid userId = Guid.Parse(session.Metadata["user_id"]);
                int auctionId = int.Parse(session.Metadata["auction_id"]);

                var entity = new BidRegistration
                {
                    BidRegistrationId = Guid.NewGuid(),
                    AuctionId = auctionId,
                    UserId = userId,
                    PaymentId = session.PaymentIntentId,
                    PaymentStatusId = 1 // 1 = Paid
                };

                var result = await _unitOfWork.Auction.SaveBidRegistration(entity);

                _logger.LogInformation(
                    $"Bid registration processed. Auction:{auctionId} User:{userId}");

                return Ok(result);
            }

            return Ok(); // Prevent Stripe retries
        }

        [HttpGet("Get_AdminLiveAuctions")]
        public async Task<IActionResult> Get_AdminLiveAuctions()
        {
            var data = await _unitOfWork.Auction.GetAdminLiveAuctionsAsync();
            return Ok(data);
        }

        [HttpGet("Get_AuctionWinnerSellerDetails")]
        public async Task<IActionResult> GetAuctionWinnerSellerDetails( Guid userId)
        {
            try
            {
                var result = await _unitOfWork.Auction.GetAuctionWinnerSellerDetailsAsync( userId);

                if (result == null)
                {
                    return Ok(new BaseResponse
                    {
                        ResultId = 0,
                        ResultMessage = "No details found."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpDelete("DeleteAuction")]
        public async Task<IActionResult> DeleteAuction(int auctionId)
        {
            try
            {
                var result = await _unitOfWork.Auction.DeleteAuction(auctionId);

                return Ok(new
                {
                    ResultId = result.ResultId,
                    ResultMessage = result.ResultMessage,
                    Status = result.ResultId > 0
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }

        [HttpGet("Get_LiveAuctions")]
        public async Task<IActionResult> GetLiveAuctions(Guid? UserId=null)
        {
            _logger.LogInformation($"UserId Received: {UserId}");
            var auctions = await _unitOfWork.Auction.GetLiveAuctions(UserId);

            var combinedAuctions = new List<AuctionWithImagesModel>();

            foreach (var auction in auctions)
            {
                var images = await _unitOfWork.Auction.GetAuctionImages(auction.AuctionId);

                var auctionResponse = new AuctionWithImagesModel
                {
                    AuctionId = auction.AuctionId,
                    BusinessId = auction.BusinessId,
                    UserId = auction.UserId,
                    User = auction.User,
                    ItemTypeId = auction.ItemTypeId,
                    ItemTitle = auction.ItemTitle,
                    ItemDescription = auction.ItemDescription,
                    ItemCondition = auction.ItemCondition,
                    PriceIncrement = auction.PriceIncrement,
                    ReservePrice = auction.ReservePrice,
                    MinDeposite = auction.MinDeposite,
                    StartTime = auction.StartTime,
                    EndTime = auction.EndTime,
                    ItemLocation = auction.ItemLocation,
                    DeleveryMethodId = auction.DeleveryMethodId,
                    AuctionStatus = auction.AuctionStatus,
                    CreatedBy = auction.CreatedBy,
                    IsRegistered = auction.IsRegistered,
                    CreatedAt = auction.CreatedAt,
                    Images = images
                };

                combinedAuctions.Add(auctionResponse);
            }

            return Ok(combinedAuctions);
        }

        [HttpGet("Get_AdminAuctionDetails")]
        public async Task<IActionResult> Get_AdminAuctionDetails(int auctionId)
        {
            try
            {
                // Existing API logic
                var auctions = await _unitOfWork.Auction
                    .GetAuctionAuctionId(auctionId);

                if (auctions == null || !auctions.Any())
                {
                    return Ok(new
                    {
                        ResultId = 0,
                        ResultMessage = "Auction not found"
                    });
                }

                var auction = auctions.FirstOrDefault();

                var images = await _unitOfWork.Auction
                    .GetAuctionImages(auctionId);

                var registeredUsers = await _unitOfWork.Auction
                    .GetAuctionRegisteredUsers(auctionId);

                var bids = await _unitOfWork.Auction
                    .GetRecentBids(auctionId);

                var highestBid = bids
                    .OrderByDescending(x => x.BidAmount)
                    .FirstOrDefault();

                return Ok(new
                {
                    ResultId = 1,
                    ResultMessage = "Success",

                    Auction = new
                    {
                        auction.AuctionId,
                        auction.BusinessId,
                        auction.UserId,
                        auction.User,
                        auction.ItemTypeId,
                        auction.ItemTitle,
                        auction.ItemDescription,
                        auction.ItemCondition,
                        auction.PriceIncrement,
                        auction.ReservePrice,
                        auction.MinDeposite,
                        auction.StartTime,
                        auction.EndTime,
                        auction.ItemLocation,
                        auction.DeleveryMethodId,
                        auction.AuctionStatus,
                        auction.CreatedBy,
                        auction.IsRegistered,
                        auction.CreatedAt,
                        Images = images
                    },

                    TotalRegisteredUsers = registeredUsers.Count,
                    RegisteredUsers = registeredUsers,

                    TotalBids = bids.Count,
                    HighestBid = highestBid?.BidAmount ?? 0,
                    HighestBidder = highestBid?.UserName ?? "",

                    BidHistory = bids
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message
                });
            }
        }
    }
}