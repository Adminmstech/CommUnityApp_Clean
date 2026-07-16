using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Bcpg;
using QRCoder;
using Dapper;
using NuGet.Protocol.Core.Types;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public ProductController(
            ILogger<ProductController> logger,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _logger = logger;

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));

            _configuration = configuration;
        }

        //[HttpPost("Add_Product")]
        //public async Task<IActionResult> AddProduct(Product entity)
        //{
        //    var data = await _unitOfWork.Product.AddProduct(entity);
        //    return Ok(data);
        //}

        //[HttpGet("Get_Products")]
        //public async Task<IActionResult> GetProducts()
        //{
        //    var data = await _unitOfWork.Product.GetAllProducts();
        //    return Ok(data);
        //}

        //[HttpGet("Get_ProductById")]
        //public async Task<IActionResult> GetProductById(int productId)
        //{
        //    var data = await _unitOfWork.Product.GetProductById(productId);
        //    return Ok(data);
        //}

        [HttpPost("Add_ProductCategory")]
        public async Task<IActionResult> AddProductCategory(ProductCategories entity)
        {
            var data = await _unitOfWork.Product.AddProductCategory(entity);
            return Ok(data);
        }

        [HttpGet("Get_ProductCategories")]
        public async Task<IActionResult> GetProductCategories()
        {
            var data = await _unitOfWork.Product.GetProductCategories();
            return Ok(data);
        }

        [HttpPost("UploadProduct")]
        public async Task<IActionResult> Add_Product_With_Images([FromBody] ProductWithImagesModel request)
        {
            if (request == null || request.Product == null)
                return BadRequest("Invalid product data");

            try
            {
                // Save Product
                var productResult = await _unitOfWork.Product.AddProduct(request.Product);

                if (productResult == null || productResult.ResultId <= 0)
                    return BadRequest("Failed to create product");

                int newProductId = productResult.ResultId;

                // Save Images
                if (request.Images != null && request.Images.Any())
                {
                    string folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "Uploads",
                        "products"
                    );

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    if (request.Images.Count(i => i.IsPrimary) > 1)
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

                            string relativePath = $"Uploads/products/{fileName}";

                            var productImage = new ProductImageUpload
                            {
                                ProductId = newProductId,
                                ImagePath = relativePath,
                                IsPrimary = img.IsPrimary
                            };

                            await _unitOfWork.Product.AddProductImage(productImage);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    productId = newProductId,
                    message = "Product created successfully with images"
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



        [HttpGet("Get_Products")]
        public async Task<IActionResult> GetProductsWithImages()
        {
            try
            {
                var products = await _unitOfWork.Product.GetAllProducts();

                var productList = new List<ProductWithImagesModel>();

                foreach (var product in products)
                {
                    var images = await _unitOfWork.Product.GetProductImageById(product.ProductId);

                    var response = new ProductWithImagesModel
                    {
                        Product = product
                    };

                    if (images != null && images.Count > 0)
                    {
                        foreach (var image in images)
                        {
                            response.Images.Add(new ProductImageUpload
                            {
                                ProductImageId = image.ProductImageId,
                                ProductId = image.ProductId,
                                ImagePath = image.ImagePath,
                                IsPrimary = image.IsPrimary
                            });
                        }
                    }

                    productList.Add(response);
                }

                return Ok(productList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error retrieving products",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("Get_ProductById")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var product = await _unitOfWork.Product.GetProductById(productId);

                if (product == null)
                    return NotFound("Product not found");

                var images = await _unitOfWork.Product.GetProductImageById(productId);

                var response = new ProductFullResponse
                {
                    ProductDetails = new ProductDetails
                    {
                        ProductId = product.ProductId,
                        BusinessId = product.BusinessId,
                        CategoryId = product.CategoryId,
                        ProductCategory = product.ProductCategory,
                        ProductName = product.ProductName,
                        Description = product.Description,
                        Price = product.Price,
                        DiscountPrice = product.DiscountPrice,
                        StartDate = product.StartDate,
                        EndDate = product.EndDate,
                        RedemptionCoins = product.RedemptionCoins,
                        ReferAFriend = product.ReferAFriend,
                        IsActive = product.IsActive,
                        CreatedAt = product.CreatedAt
                    },

                    ProductImages = images ?? new List<ProductImage>(),

                    BusinessDetails = new BusinessDetails
                    {
                        BusinessName = product.BusinessName,
                        OwnerName = product.OwnerName,
                        Email = product.Email,
                        Phone = product.Phone,
                        Address = product.Address,
                        City = product.City,
                        State = product.State,
                        Country = product.Country,
                        Logo = product.Logo,
                        Latitude = product.Latitude,
                        Longitude = product.Longitude
                    }
                };

                return Ok(new List<ProductFullResponse> { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error retrieving product",
                    Error = ex.Message
                });
            }
        }


        [HttpGet("Get_ProductByBusinessId")]
        public async Task<IActionResult> GetProductByBusinessId(int BusinessId)
        {
            try
            {
                var products = await _unitOfWork.Product.GetProductByBusinessId(BusinessId);

                var productList = new List<ProductWithImagesModel>();

                foreach (var product in products)
                {
                    var images = await _unitOfWork.Product.GetProductImageById(product.ProductId);

                    var response = new ProductWithImagesModel
                    {
                        Product = product
                    };

                    if (images != null)
                    {
                        foreach (var image in images)
                        {
                            response.Images.Add(new ProductImageUpload
                            {
                                ProductImageId = image.ProductImageId,
                                ProductId = image.ProductId,
                                ImagePath = image.ImagePath,
                                IsPrimary = image.IsPrimary
                            });
                        }
                    }

                    productList.Add(response);
                }

                return Ok(productList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error retrieving products",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("Add_FavouriteBusiness")]
        public async Task<IActionResult> AddFavouriteBusiness(FavBusineess F)
        {
            var data = await _unitOfWork.Product.AddFavouriteBusiness(F);
            return Ok(data);
        }

        [HttpGet("Get_UserFavBusiness")]
        public async Task<IActionResult> GetFavBusiness(Guid UserId)
        {
            var data = await _unitOfWork.Product.GetFavBusiness(UserId);
            return Ok(data);
        }

        [HttpPost("Add_ToCart")]
        public async Task<IActionResult> AddToCart(AddToCartRequest F)
        {
            var data = await _unitOfWork.Product.AddToCart(F);
            return Ok(data);
        }

        [HttpPost("Remove_fromCart")]
        public async Task<IActionResult> Removefromcart(AddToCartRequest F)
        {
            var data = await _unitOfWork.Product.RemoveFromCart(F);
            return Ok(data);
        }

        [HttpGet("Get_CartItems")]
        public async Task<IActionResult> GetCart(Guid UserId)
        {
            try
            {
                var cartItems = await _unitOfWork.Product.GetCartItems(UserId);

                var cartList = new List<CartWithImagesModel>();

                foreach (var cart in cartItems)
                {
                    var images = await _unitOfWork.Product.GetProductImageById(cart.ProductId);

                    var response = new CartWithImagesModel
                    {
                        Cart = cart
                    };

                    if (images != null)
                    {
                        foreach (var image in images)
                        {
                            response.Images.Add(new ProductImageUpload
                            {
                                ProductImageId = image.ProductImageId,
                                ProductId = image.ProductId,
                                ImagePath = image.ImagePath,
                                IsPrimary = image.IsPrimary
                            });
                        }
                    }

                    cartList.Add(response);
                }

                return Ok(cartList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error retrieving cart items",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("Get_AdminPromotions")]
        public async Task<IActionResult> Get_AdminPromotions()
        {
            var data = await _unitOfWork.Product.GetAdminPromotionsAsync();
            return Ok(data);
        }


        /* [HttpPost("UploadPromotion")]
         public async Task<IActionResult> UploadPromotion(
     [FromBody] PromotionWithImageModel request)
         {
             if (request == null || request.Promotion == null)
                 return BadRequest("Invalid promotion data");

             try
             {
                 string promotionImagePath = null;
                 string qrImagePath = null;

                 // =========================
                 // SAVE PROMOTION IMAGE
                 // =========================

                 if (!string.IsNullOrWhiteSpace(request.PromotionImageBase64))
                 {
                     string promotionFolder = Path.Combine(
                         Directory.GetCurrentDirectory(),
                         "wwwroot",
                         "Uploads",
                         "Promotions"
                     );

                     if (!Directory.Exists(promotionFolder))
                         Directory.CreateDirectory(promotionFolder);

                     string base64Data = request.PromotionImageBase64;

                     if (base64Data.Contains(","))
                         base64Data = base64Data.Split(',')[1];

                     byte[] imageBytes = Convert.FromBase64String(base64Data);

                     string fileName = Guid.NewGuid() + ".jpg";

                     string filePath = Path.Combine(promotionFolder, fileName);

                     await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                     promotionImagePath = $"Uploads/Promotions/{fileName}";
                 }

                 request.Promotion.PromotionImage = promotionImagePath;

                 // =========================
                 // SAVE PROMOTION
                 // =========================

                 var promotionResult = await _unitOfWork.Product
                     .AddUpdatePromotion(request.Promotion);

                 if (promotionResult == null || promotionResult.PromotionId <= 0)
                 {
                     return BadRequest(new
                     {
                         success = false,
                         message = "Failed to create promotion"
                     });
                 }

                 // =========================
                 // GENERATE QR CODE
                 // =========================

                 if (!string.IsNullOrWhiteSpace(promotionResult.PromotionUrl))
                 {
                     string qrFolder = Path.Combine(
                         Directory.GetCurrentDirectory(),
                         "wwwroot",
                         "Uploads",
                         "PromotionQR"
                     );

                     if (!Directory.Exists(qrFolder))
                         Directory.CreateDirectory(qrFolder);

                     using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                     {
                         QRCodeData qrCodeData = qrGenerator.CreateQrCode(
                             promotionResult.PromotionUrl,
                             QRCodeGenerator.ECCLevel.Q
                         );

                         PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

                         byte[] qrCodeBytes = qrCode.GetGraphic(20);

                         string qrFileName = Guid.NewGuid() + ".png";

                         string qrFilePath = Path.Combine(qrFolder, qrFileName);

                         await System.IO.File.WriteAllBytesAsync(
                             qrFilePath,
                             qrCodeBytes
                         );

                         qrImagePath = $"Uploads/PromotionQR/{qrFileName}";
                     }

                     // =========================
                     // UPDATE QR IMAGE PATH
                     // =========================

                     string fullQrUrl =
                         $"{Request.Scheme}://{Request.Host}/Uploads/PromotionQR/{Path.GetFileName(qrImagePath)}";

                     using var connection = new SqlConnection(
                         _configuration.GetConnectionString("DefaultConnection")
                     );

                     await connection.OpenAsync();

                     await connection.ExecuteAsync(
                         @"UPDATE ProductPromotions
                         SET QRCodeImage = @QRCodeImage
                         WHERE PromotionId = @PromotionId",
                         new
                         {
                             QRCodeImage = fullQrUrl,
                             PromotionId = promotionResult.PromotionId
                         }
                     );

                     qrImagePath = fullQrUrl;
                 }

                 return Ok(new
                 {
                     success = true,
                     promotionId = promotionResult.PromotionId,
                     promotionUrl = promotionResult.PromotionUrl,
                     qrCodeImage = qrImagePath,
                     message = "Promotion created successfully with QR Code"
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
         }*/


        [HttpPost("UploadPromotion")]
        public async Task<IActionResult> UploadPromotion(
    [FromBody] PromotionWithImageModel request)
        {
            if (request == null || request.Promotion == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid promotion data"
                });
            }

            try
            {
                string promotionImagePath = null;
                string qrImagePath = null;

                // =========================
                // SAVE PROMOTION IMAGE
                // =========================

                if (!string.IsNullOrWhiteSpace(request.PromotionImageBase64))
                {
                    string promotionFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "Uploads",
                        "Promotions"
                    );

                    if (!Directory.Exists(promotionFolder))
                        Directory.CreateDirectory(promotionFolder);

                    string base64Data = request.PromotionImageBase64;

                    if (base64Data.Contains(","))
                        base64Data = base64Data.Split(',')[1];

                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    string fileName = $"{Guid.NewGuid()}.jpg";

                    string filePath = Path.Combine(
                        promotionFolder,
                        fileName
                    );

                    await System.IO.File.WriteAllBytesAsync(
                        filePath,
                        imageBytes
                    );

                    promotionImagePath = $"Uploads/Promotions/{fileName}";
                }

                // Preserve old image during update
                if (!string.IsNullOrEmpty(promotionImagePath))
                {
                    request.Promotion.PromotionImage = promotionImagePath;
                }

                // =========================
                // SAVE PROMOTION
                // =========================

                var promotionResult =
                    await _unitOfWork.Product.AddUpdatePromotion(
                        request.Promotion
                    );

                if (promotionResult == null ||
                    promotionResult.PromotionId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to save promotion"
                    });
                }

                // =========================
                // GENERATE QR CODE (NEW ONLY)
                // =========================

                if (request.Promotion.PromotionId == 0 &&
                    !string.IsNullOrWhiteSpace(
                        promotionResult.PromotionUrl))
                {
                    string qrFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "Uploads",
                        "PromotionQR"
                    );

                    if (!Directory.Exists(qrFolder))
                        Directory.CreateDirectory(qrFolder);

                    using (QRCodeGenerator qrGenerator =
                           new QRCodeGenerator())
                    {
                        QRCodeData qrCodeData =
                            qrGenerator.CreateQrCode(
                                promotionResult.PromotionUrl,
                                QRCodeGenerator.ECCLevel.Q
                            );

                        PngByteQRCode qrCode =
                            new PngByteQRCode(qrCodeData);

                        byte[] qrBytes =
                            qrCode.GetGraphic(20);

                        string qrFileName =
                            $"{Guid.NewGuid()}.png";

                        string qrFilePath =
                            Path.Combine(
                                qrFolder,
                                qrFileName
                            );

                        await System.IO.File.WriteAllBytesAsync(
                            qrFilePath,
                            qrBytes
                        );

                        qrImagePath =
                            $"{Request.Scheme}://{Request.Host}/Uploads/PromotionQR/{qrFileName}";

                        using var connection = new SqlConnection(
                            _configuration.GetConnectionString(
                                "DefaultConnection"));

                        await connection.ExecuteAsync(
                            @"UPDATE ProductPromotions
                      SET QRCodeImage = @QRCodeImage
                      WHERE PromotionId = @PromotionId",
                            new
                            {
                                QRCodeImage = qrImagePath,
                                PromotionId =
                                    promotionResult.PromotionId
                            });
                    }
                }

                return Ok(new
                {
                    success = true,
                    promotionId = promotionResult.PromotionId,
                    promotionUrl = promotionResult.PromotionUrl,
                    qrCodeImage = qrImagePath,
                    message = request.Promotion.PromotionId == 0
                        ? "Promotion created successfully"
                        : "Promotion updated successfully"
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


        // =====================================
        // Business Promotions
        // =====================================

        [HttpGet("Get_BusinessPromotions")]
        public async Task<IActionResult> GetBusinessPromotions(int businessId)
        {
            try
            {
                var result =
                    await _unitOfWork.Product
                        .GetBusinessPromotionsAsync(businessId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

        // =====================================
        // Promotion Details
        // =====================================

        [HttpGet("Get_PromotionDetails")]
        public async Task<IActionResult> GetPromotionDetails(int promotionId)
        {
            try
            {
                var result =
                    await _unitOfWork.Product
                        .GetPromotionDetailsAsync(promotionId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

        // =====================================
        // Scan QR / Token
        // =====================================

        [HttpPost("Get_PromotionByToken")]
        public async Task<IActionResult> GetPromotionByToken(
            [FromBody] GetPromotionByTokenRequest request)
        {
            try
            {
                var result =
                    await _unitOfWork.Product
                        .GetPromotionByTokenAsync(
                            request.PromotionToken,
                            request.UserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ResultId = -1,
                    ResultMessage = ex.Message
                });
            }
        }

        [HttpGet("GetPromotionsList")]
        public async Task<IActionResult> GetPromotions(int businessId)
        {
            var data = await _unitOfWork.Product.GetPromotions(businessId);

            return Ok(data);
        }

        [HttpPost("GetPromotionRedemptionSummary")]
        public async Task<IActionResult>GetPromotionRedemptionSummary([FromBody] PromotionRedemptionRequest request)
        {
            var data = await _unitOfWork.Product
                .GetPromotionRedemptionSummary(
                    request.PromotionId,
                    request.UserId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpPost("RedeemPromotion")]
        public async Task<IActionResult> RedeemPromotion([FromBody] PromotionRedemptionRequest request)
        {
            try
            {
                var result = await _unitOfWork.Product
                    .RedeemPromotion(
                        request.PromotionId,
                        request.UserId);

                if (result == null)
                {
                    return Ok(new
                    {
                        ResultId = 0,
                        ResultMessage = "Something went wrong.",
                        Status = false
                    });
                }

                if (!result.Status || result.RedemptionId == null || result.RedemptionId <= 0)
                {
                    return Ok(new
                    {
                        ResultId = result.ResultId,
                        ResultMessage = result.ResultMessage,
                        Status = result.Status
                    });
                }

                string qrImageUrl = "";

                string qrFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "Uploads",
                    "PromotionRedemptions");

                if (!Directory.Exists(qrFolder))
                    Directory.CreateDirectory(qrFolder);

                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    var qrPayload = new
                    {
                        RedemptionId = result.RedemptionId,
                        RedemptionCode = result.RedemptionCode
                    };

                    string qrContent =
                        System.Text.Json.JsonSerializer.Serialize(qrPayload);

                    QRCodeData qrCodeData =
                        qrGenerator.CreateQrCode(
                            qrContent,
                            QRCodeGenerator.ECCLevel.Q);

                    PngByteQRCode qrCode =
                        new PngByteQRCode(qrCodeData);

                    byte[] qrBytes = qrCode.GetGraphic(20);

                    string fileName = Guid.NewGuid() + ".png";

                    string filePath = Path.Combine(qrFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, qrBytes);

                    qrImageUrl =
                        $"{Request.Scheme}://{Request.Host}/Uploads/PromotionRedemptions/{fileName}";
                }

                using (var connection = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    await connection.ExecuteAsync(
                        @"UPDATE PromotionRedemptions
                  SET QRCodeImage = @QRCodeImage
                  WHERE RedemptionId = @RedemptionId",
                        new
                        {
                            QRCodeImage = qrImageUrl,
                            RedemptionId = result.RedemptionId
                        });
                }

                result.QRCodeImage = qrImageUrl;

                return Ok(new
                {
                    ResultId = result.ResultId,
                    ResultMessage = result.ResultMessage,
                    Status = result.Status,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ResultId = 0,
                    ResultMessage = ex.Message,
                    Status = false
                });
            }
        }
        [HttpGet("GetMyPromotionRedemptions")]
        public async Task<IActionResult>GetMyPromotionRedemptions(Guid userId)
        {
            var data = await _unitOfWork.Product
                .GetMyPromotionRedemptions(
                    userId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }


        [HttpPost("VerifyPromotionRedemption")]
        public async Task<IActionResult>VerifyPromotionRedemption([FromBody] VerifyPromotionRequest request)
        {
            var data = await _unitOfWork.Product
                .VerifyPromotionRedemption(
                    request.RedemptionCode);

            return Ok(new
            {
                ResultId = data.Status,
                ResultMessage = data.Message,
                Status = data.Status == 1,
                Data = data
            });
        }
        [HttpGet("GetTopFiveProductPromotions")]
        public async Task<IActionResult> GetTopFiveProductPromotions()
        {
            var data = await _unitOfWork.Product.GetTopFiveProductPromotions();

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Status = true,
                Data = data
            });
        }

        [HttpDelete("DeletePromotion")]
        public async Task<IActionResult> DeletePromotion(int promotionId)
        {
            var result =
                await _unitOfWork.Product.DeletePromotion(promotionId);

            return Ok(result);
        }

        [HttpGet("GetPromotionById")]
        public async Task<IActionResult> GetPromotionById(int promotionId)
        {
            var data =
                await _unitOfWork.Product.GetPromotionById(promotionId);

            return Ok(new
            {
                ResultId = 1,
                ResultMessage = "Success",
                Data = data
            });
        }
    }
}

