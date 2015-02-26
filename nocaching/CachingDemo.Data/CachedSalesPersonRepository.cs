using CachingDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data
{
    public class CachedSalesPersonRepository : ISalesPersonRepository
    {
        private SalesPersonRepository innerRepository;

        public CachedSalesPersonRepository(SalesPersonRepository innerRepository)
        {
            this.innerRepository = innerRepository;
        }

        public async Task<SalesPerson> GetAsync(int id)
        {
            return await CacheService.GetAsync<SalesPerson>("sp:" + id, () => this.innerRepository.GetAsync(id));
        }

        public async Task<ICollection<SalesPersonTotalSales>> GetTopTenSalesPeopleAsync()
        {
            return await CacheService.GetAsync<ICollection<SalesPersonTotalSales>>(
                "sp:topTen",
                () => this.innerRepository.GetTopTenSalesPeopleAsync());
        }
    }
}
