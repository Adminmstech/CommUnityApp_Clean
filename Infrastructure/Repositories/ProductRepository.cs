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

        public async Task<Product?> GetProductById(int productId)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ProductId", productId);

            return await connection.QueryFirstOrDefaultAsync<Product>(
                "Get_ProductById",
                parameters,
                commandType: CommandType.StoredProcedure
            );
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
    }
}
