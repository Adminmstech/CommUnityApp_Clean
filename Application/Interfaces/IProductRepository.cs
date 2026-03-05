using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IProductRepository
    {
        Task<BaseResponse> AddProduct(Product entity);
        Task<List<Product>> GetAllProducts();
        Task<Product?> GetProductById(int productId);
        Task<BaseResponse> AddProductCategory(ProductCategories entity);
        Task<List<ProductCategories>> GetProductCategories();
        Task<BaseResponse> AddProductImage(ProductImageUpload entity);
    }
}

