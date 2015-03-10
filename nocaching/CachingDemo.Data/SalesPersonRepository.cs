using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class SalesPersonRepository : ISalesPersonRepository
    {
        public SalesPersonRepository()
        {
        }

        public async Task<SalesPerson> GetAsync(int id)
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.SalesPeople
                    .Where(sp => sp.Id == id)
                    .SingleOrDefaultAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<ICollection<SalesPersonTotalSales>> GetTopTenSalesPeopleAsync()
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.SalesOrders
                    .Where(soh => soh.SalesPerson != null)
                    .GroupBy(soh => soh.SalesPerson)
                    .OrderByDescending(g => g.Sum(soh => soh.TotalDue))
                    .Take(10)
                    .Select(g => new SalesPersonTotalSales()
                    {
                        SalesPerson = g.Key,
                        TotalSales = g.Sum(soh => soh.TotalDue)
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}
