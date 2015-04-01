// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NoCaching.Data.Models;

namespace NoCaching.Data
{
    public class SalesPersonRepository : ISalesPersonRepository
    {
        public async Task<SalesPerson> GetAsync(int id)
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.SalesPeople
                    .Where(sp => sp.Id == id)
                    .FirstOrDefaultAsync()
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
