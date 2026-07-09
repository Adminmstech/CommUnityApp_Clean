using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class ProductRepository: IProductRepository
    {
        private readonly IConfiguration _configuration;

        public ProductRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<BaseResponse> AddProduct(Product entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@ProductId", entity.ProductId);
            parameters.Add("@BusinessId", entity.BusinessId);
            parameters.Add("@CategoryId", entity.CategoryId);
            parameters.Add("@ProductName", entity.ProductName);
            parameters.Add("@Description", entity.Description);
            parameters.Add("@Price", entity.Price);
            parameters.Add("@DiscountPrice", entity.DiscountPrice);
            parameters.Add("@StartDate", entity.StartDate);
            parameters.Add("@EndDate", entity.EndDate);
            parameters.Add("@RedemptionCoins", entity.RedemptionCoins);
            parameters.Add("@ReferAFriend", entity.ReferAFriend);
            parameters.Add("@IsActive", entity.IsActive);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_Product",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<BaseResponse> AddProductImage(ProductImageUpload entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@ProductImageId", entity.ProductImageId);
            parameters.Add("@ProductId", entity.ProductId);
            parameters.Add("@ImagePath", entity.ImagePath);
            parameters.Add("@IsPrimary", entity.IsPrimary);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_ProductImage",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<List<Product>> GetAllProducts()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<Product>(
                "Get_Products",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<ProductDetails?> GetProductById(int productId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ProductId", productId);

            return await connection.QueryFirstOrDefaultAsync<ProductDetails>(
                "Get_ProductById",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<Product>> GetProductByBusinessId(int BusinessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@BusinessId", BusinessId);

            var result = await connection.QueryAsync<Product>(
                "Get_ProductByBusinessId",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }


        public async Task<List<ProductImage>> GetProductImageById(int productId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ProductId", productId);

            var result = await connection.QueryAsync<ProductImage>(
                "Get_ProductImages",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }


        public async Task<BaseResponse> AddProductCategory(ProductCategories entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ProductCategory", entity.ProductCategory);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_ProductCategory",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<List<ProductCategories>> GetProductCategories()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<ProductCategories>(
                "Get_ProductCategories",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }


        public async Task<FavoriteBusinessResult> AddFavouriteBusiness(FavBusineess entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@BusinessId", entity.BusinessId);
          
            var result = await connection.QueryAsync<FavoriteBusinessResult>(
                "Add_Favorite_Business",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }


        public async Task<BusinessDetailsDto> GetFavBusiness( Guid UserId )
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);

            var result = await connection.QueryAsync<BusinessDetailsDto>(
                "Get_UserFavBusiness",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }


        public async Task<BaseResponse> AddToCart(AddToCartRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@ProductId", entity.ProductId);
            parameters.Add("@quantity", entity.Quantity);

            var result = await connection.QueryAsync<BaseResponse>(
                "Add_ProductToCart",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }


        public async Task<BaseResponse> RemoveFromCart(AddToCartRequest entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", entity.UserId);
            parameters.Add("@ProductId", entity.ProductId);

            var result = await connection.QueryAsync<BaseResponse>(
                "Remove_ProductFromCart",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<List<CartItemResponse>> GetCartItems( Guid UserId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", UserId);


            var result = await connection.QueryAsync<CartItemResponse>(
                "Get_CartItems",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<List<AdminPromotionDto>> GetAdminPromotionsAsync()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var result = await connection.QueryAsync<AdminPromotionDto>(
                "Get_AdminPromotions",
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }


        /* public async Task<PromotionResult> AddUpdatePromotion(ProductPromotionModel entity)
         {
             using var connection = new SqlConnection(
                 _configuration.GetConnectionString("DefaultConnection")
             );

             await connection.OpenAsync();

             var parameters = new DynamicParameters();

             parameters.Add("@PromotionId", entity.PromotionId);

             parameters.Add("@ProductId", entity.ProductId);

             parameters.Add("@BusinessId", entity.BusinessId);

             parameters.Add("@PromotionType", entity.PromotionType);

             parameters.Add("@OtherPromotionText", entity.OtherPromotionText);

             parameters.Add("@PromoCode", entity.PromoCode);

             parameters.Add("@DiscountValue", entity.DiscountValue);

             parameters.Add("@CashbackValue", entity.CashbackValue);

             parameters.Add("@MinimumPurchaseAmount", entity.MinimumPurchaseAmount);

             parameters.Add("@MaxRedemptionLimit", entity.MaxRedemptionLimit);

             parameters.Add("@BuyGetDetails", entity.BuyGetDetails);

             parameters.Add("@ComboOfferDetails", entity.ComboOfferDetails);

             parameters.Add("@IsLimitedDeal", entity.IsLimitedDeal);

             parameters.Add("@PromotionImage", entity.PromotionImage);

             parameters.Add("@StartDate", entity.StartDate);

             parameters.Add("@EndDate", entity.EndDate);

             parameters.Add("@IsActive", entity.IsActive);

             var result = await connection.QueryAsync<PromotionResult>(
                 "InsertUpdate_ProductPromotion",
                 parameters,
                 commandType: CommandType.StoredProcedure
             );

             return result.FirstOrDefault();
         }*/

        public async Task<PromotionResult> AddUpdatePromotion(ProductPromotionModel entity)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

            parameters.Add("@PromotionId", entity.PromotionId);
            parameters.Add("@BusinessId", entity.BusinessId);
            parameters.Add("@FeaturedProducts", entity.FeaturedProducts);
            parameters.Add("@PromotionTitle", entity.PromotionTitle);
            parameters.Add("@OfferHeadline", entity.OfferHeadline);
            parameters.Add("@PromotionalPrice", entity.PromotionalPrice);
            parameters.Add("@ActualPrice", entity.ActualPrice);

            parameters.Add("@MaxCoinsRedemptionPercentage", entity.MaxCoinsRedemptionPercentage);
            parameters.Add("@IndoCoinsEarned", entity.IndoCoinsEarned);
            parameters.Add("@FriendRewardCoins", entity.FriendRewardCoins);
            parameters.Add("@PromotionImage", entity.PromotionImage);
            parameters.Add("@StartDate", entity.StartDate);
            parameters.Add("@EndDate", entity.EndDate);
            parameters.Add("@IsActive", entity.IsActive);

            return await connection.QueryFirstOrDefaultAsync<PromotionResult>(
                "InsertUpdate_ProductPromotion",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<PromotionListModel>> GetBusinessPromotionsAsync(int businessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@BusinessId", businessId);

            var result = await connection.QueryAsync<PromotionListModel>(
                "Get_BusinessPromotions",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<PromotionDetailsModel> GetPromotionDetailsAsync(int promotionId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@PromotionId", promotionId);

            return await connection.QueryFirstOrDefaultAsync<PromotionDetailsModel>(
                "Get_PromotionDetails",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PromotionDetailsModel> GetPromotionByTokenAsync(
            Guid promotionToken,
            Guid? userId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

            parameters.Add("@PromotionToken", promotionToken);
            parameters.Add("@UserId", userId);

            return await connection.QueryFirstOrDefaultAsync<PromotionDetailsModel>(
                "Get_PromotionByToken",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<BusinessPromotionModel>> GetPromotions(int businessId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = (await connection.QueryAsync<BusinessPromotionModel>(
                "Get_UserPromotions",
                new
                {
                    BusinessId = businessId
                },
                commandType: CommandType.StoredProcedure)).ToList();

            foreach (var item in result)
            {
                if (!string.IsNullOrEmpty(item.QRCodeImage) &&
                    !item.QRCodeImage.StartsWith("http"))
                {
                    item.QRCodeImage =
                        $"{_configuration["BaseUrl"]}/{item.QRCodeImage.TrimStart('/')}";
                }

                if (!string.IsNullOrEmpty(item.PromotionImage) &&
                    !item.PromotionImage.StartsWith("http"))
                {
                    item.PromotionImage =
                        $"{_configuration["BaseUrl"]}/{item.PromotionImage.TrimStart('/')}";
                }
            }

            return result;
        }

        public async Task<PromotionRedemptionSummary>GetPromotionRedemptionSummary(long promotionId,Guid userId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await connection.QueryFirstOrDefaultAsync<
                PromotionRedemptionSummary>(
                "SP_GetPromotionRedemptionSummary",
                new
                {
                    PromotionId = promotionId,
                    UserId = userId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PromotionRedemptionResult>RedeemPromotion(long promotionId,Guid userId)
        {
            using var connection = new SqlConnection( _configuration.GetConnectionString("DefaultConnection"));

            var result =
                await connection.QueryFirstOrDefaultAsync<
                    PromotionRedemptionResult>(
                "SP_RedeemPromotion",
                new
                {
                    PromotionId = promotionId,
                    UserId = userId
                },
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<List<PromotionRedemptionModel>>GetMyPromotionRedemptions(Guid userId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var result =
                await connection.QueryAsync<
                    PromotionRedemptionModel>(
                "SP_GetMyPromotionRedemptions",
                new
                {
                    UserId = userId
                },
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<VerifyPromotionResponse>VerifyPromotionRedemption(string redemptionCode)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return await connection.QueryFirstOrDefaultAsync<
                VerifyPromotionResponse>(
                "SP_VerifyPromotionRedemption",
                new
                {
                    RedemptionCode = redemptionCode
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<TopProductPromotionEntity>> GetTopFiveProductPromotions()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var result = await connection.QueryAsync<TopProductPromotionEntity>(
                "sp_GetTopFiveProductPromotions",
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<BaseResponse> DeletePromotion(int promotionId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await connection.QueryFirstOrDefaultAsync<BaseResponse>(
                "sp_DeletePromotion",
                new
                {
                    PromotionId = promotionId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<ProductPromotionModel> GetPromotionById(int promotionId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return await connection.QueryFirstOrDefaultAsync<ProductPromotionModel>(
                "sp_GetPromotionById",
                new
                {
                    PromotionId = promotionId
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}
