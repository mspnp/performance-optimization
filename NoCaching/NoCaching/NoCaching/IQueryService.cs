using NoCaching.DTOs;

public interface IQueryService
{
    Task<ProductDTO> GetProductAsync(int id);
    Task<ProductCategoryDTO> GetProductCategoryAsync(int subcategoryId);
}