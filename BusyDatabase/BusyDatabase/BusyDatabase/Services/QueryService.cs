using Microsoft.Extensions.Caching.Memory;

namespace BusyDatabase.Services
{
    public class QueryService(IMemoryCache _memoryCache) : IQueryService
    {
        public async Task<string> GetAsync(string key)
        {
            return await _memoryCache.GetOrCreateAsync(key, async entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), key);
                var sqlContent = await File.ReadAllTextAsync(filePath);
                return sqlContent;
            });
        }
    }
}
