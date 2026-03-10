using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;

namespace CommUnityApp.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public ProductController(ILogger<ProductController> logger, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _logger = logger;
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _config = config;
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
        public async Task<IActionResult> AddFavouriteBusiness(FavBusineess F )
        {
            var data = await _unitOfWork.Product.AddFavouriteBusiness(F);
           return Ok(data);
        }

        [HttpGet("Get_UserFavBusiness")]
        public async Task<IActionResult> GetFavBusiness(Guid UserId )
        {
            var data = await _unitOfWork.Product.GetFavBusiness(UserId);
            return Ok(data);
        }
    }
}
