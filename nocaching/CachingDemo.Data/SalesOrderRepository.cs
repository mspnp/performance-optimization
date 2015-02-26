using CachingDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        public SalesOrderRepository()
        {
        }

        public async Task<ICollection<SalesOrderHeader>> GetTopTenSalesOrdersAsync()
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.SalesOrders
                    .Include(soh => soh.Customer.Person)
                    .Include(soh => soh.SalesPerson)
                    .OrderByDescending(soh => soh.TotalDue)
                    .Take(10)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}
