using Microsoft.Extensions.Caching.Memory;
using NoCaching.DTOs;

namespace NoCaching
{
    public class CacheQueryService(IMemoryCache _memoryCache, IQueryService _queryService) : ICacheQueryService
    {
        public async Task<ProductDTO> GetProductAsync(int id)
        {
            return await _memoryCache.GetOrCreateAsync($"product-{id}", async entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
                return await _queryService.GetProductAsync(id);
            });
        }

        public async Task<ProductCategoryDTO> GetProductCategoryAsync(int subcategoryId)
        {
            return await _memoryCache.GetOrCreateAsync($"productCategory-{subcategoryId}", async entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
                return await _queryService.GetProductCategoryAsync(subcategoryId);
            });
        }
    }
}
