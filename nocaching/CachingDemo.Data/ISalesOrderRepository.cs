using System.Collections.Generic;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public interface ISalesOrderRepository
    {
        Task<ICollection<SalesOrderHeader>> GetTopTenSalesOrdersAsync();
    }
}
