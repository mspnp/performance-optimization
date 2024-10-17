using NoCaching.DTOs;

namespace NoCaching
{
    public interface ICacheQueryService
    {
        Task<ProductDTO> GetProductAsync(int id);
        Task<ProductCategoryDTO> GetProductCategoryAsync(int subcategoryId);
    }
}