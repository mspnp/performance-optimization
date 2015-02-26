using CachingDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data
{
    public class CachedSalesOrderRepository : ISalesOrderRepository
    {
        private SalesOrderRepository innerRepository;

        public CachedSalesOrderRepository(SalesOrderRepository innerRepository)
        {
            this.innerRepository = innerRepository;
        }

        public async Task<ICollection<SalesOrderHeader>> GetTopTenSalesOrdersAsync()
        {
            return await CacheService.GetAsync<ICollection<SalesOrderHeader>>(
                "soh:topTen",
                () => this.innerRepository.GetTopTenSalesOrdersAsync());
        }
    }
}
