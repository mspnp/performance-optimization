using System.Collections.Generic;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class CachedSalesPersonRepository : ISalesPersonRepository
    {
        private readonly SalesPersonRepository _innerRepository;

        public CachedSalesPersonRepository(SalesPersonRepository innerRepository)
        {
            _innerRepository = innerRepository;
        }

        public async Task<SalesPerson> GetAsync(int id)
        {
            return await CacheService.GetAsync<SalesPerson>("sp:" + id, () => _innerRepository.GetAsync(id)).ConfigureAwait(false);
        }

        public async Task<ICollection<SalesPersonTotalSales>> GetTopTenSalesPeopleAsync()
        {
            return await CacheService.GetAsync<ICollection<SalesPersonTotalSales>>(
                "sp:topTen",
                () => _innerRepository.GetTopTenSalesPeopleAsync()).ConfigureAwait(false);
        }
    }
}
