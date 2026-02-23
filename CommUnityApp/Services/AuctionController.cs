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

        [HttpPost("UploadAuction")]
        public async Task<IActionResult> Add_Auction_With_Images([FromBody] AuctionWithImagesRequest request)
        {
            if (request == null || request.Auction == null)
                return BadRequest("Invalid auction data");

            if (request.Auction.StartTime >= request.Auction.EndTime)
                return BadRequest("EndTime must be greater than StartTime");

            try
            {
                // 1️⃣ Save Auction
                var auctionResult = await _unitOfWork.Auction.SaveAuction(request.Auction);

                if (auctionResult == null || auctionResult.ResultId <= 0)
                    return BadRequest("Failed to create auction");

                int newAuctionId = auctionResult.ResultId;

                // 2️⃣ Save Images
                if (request.Images != null && request.Images.Any())
                {
                    string folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "Uploads",
                        "auctions"
                    );

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    // ✅ Ensure only one primary image
                    bool primaryExists = request.Images.Count(i => i.IsPrimary) > 1;
                    if (primaryExists)
                        return BadRequest("Only one primary image allowed");

                    foreach (var img in request.Images)
                    {
                        if (string.IsNullOrWhiteSpace(img.ImageBase64))
                            continue;

                        try
                        {
                            string base64Data = img.ImageBase64;

                            if (base64Data.Contains(","))
                                base64Data = base64Data.Split(',')[1];

                            byte[] imageBytes = Convert.FromBase64String(base64Data);

                            string fileName = Guid.NewGuid() + ".jpg";
                            string filePath = Path.Combine(folderPath, fileName);

                            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                            string relativePath = $"Uploads/auctions/{fileName}";

                            var auctionImage = new AuctionItemImage
                            {
                                AuctionId = newAuctionId,
                                ImageUrl = relativePath,
                                ImageName = fileName,
                                IsPrimary = img.IsPrimary,
                                CreatedDate = DateTime.UtcNow
                            };

                            await _unitOfWork.Auction.SaveAuctionItemImage(auctionImage);
                        }
                        catch
                        {
                            // Skip invalid image but don't fail whole auction
                            continue;
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    auctionId = newAuctionId,
                    message = "Auction created successfully with images"
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
        public async Task<IActionResult> AuctionByIdWithImages_Edit(int AuctionId)
        {
            // Step 1: Get auction(s)
            var auctions = await _unitOfWork.Auction.GetAuctionAuctionId(AuctionId);

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
                // Broadcast to users watching this auction
                await _hubContext.Clients
     .Group(entity.AuctionId.ToString())
     .SendAsync("ReceiveBid", new
     {
         userName = result.UserName,   // 🔥 add this
         bidAmount = entity.BidAmount
     });

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
    }
}
