using System.Collections.Generic;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public interface ISalesPersonRepository
    {
        Task<SalesPerson> GetAsync(int id);

        Task<ICollection<SalesPersonTotalSales>> GetTopTenSalesPeopleAsync();
    }
}
