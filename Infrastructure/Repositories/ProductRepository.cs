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


    }
}
