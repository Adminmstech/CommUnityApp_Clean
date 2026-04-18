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
        Task<ProductDetails?> GetProductById(int productId);
        Task<BaseResponse> AddProductCategory(ProductCategories entity);
        Task<List<ProductCategories>> GetProductCategories();
        Task<BaseResponse> AddProductImage(ProductImageUpload entity);
        Task<List<ProductImage>> GetProductImageById(int productId);
        Task<List<Product>> GetProductByBusinessId(int BusinessId);
        Task<FavoriteBusinessResult> AddFavouriteBusiness(FavBusineess entity);
        Task<BusinessDetailsDto> GetFavBusiness(Guid UserId);
        Task<BaseResponse> AddToCart(AddToCartRequest entity);
        Task<BaseResponse> RemoveFromCart(AddToCartRequest entity);
        Task<List<CartItemResponse>> GetCartItems(Guid UserId);
    }
}

