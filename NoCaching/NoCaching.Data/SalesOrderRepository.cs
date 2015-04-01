// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
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
