using System.Collections.Generic;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class CachedSalesOrderRepository : ISalesOrderRepository
    {
        private readonly SalesOrderRepository innerRepository;

        public CachedSalesOrderRepository(SalesOrderRepository innerRepository)
        {
            this.innerRepository = innerRepository;
        }

        public async Task<ICollection<SalesOrderHeader>> GetTopTenSalesOrdersAsync()
        {
            return await CacheService.GetAsync<ICollection<SalesOrderHeader>>(
                "soh:topTen",
                () => this.innerRepository.GetTopTenSalesOrdersAsync()).ConfigureAwait(false);
        }
    }
}
