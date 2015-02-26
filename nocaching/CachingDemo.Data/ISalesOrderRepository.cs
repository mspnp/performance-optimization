using CachingDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data
{
    public interface ISalesOrderRepository
    {
        Task<ICollection<SalesOrderHeader>> GetTopTenSalesOrdersAsync();
    }
}
