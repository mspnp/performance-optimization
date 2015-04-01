// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using NoCaching.Data.Models;

namespace NoCaching.Data
{
    public class CachedSalesOrderRepository : ISalesOrderRepository
    {
        private readonly SalesOrderRepository _innerRepository;

        public CachedSalesOrderRepository(SalesOrderRepository innerRepository)
        {
            _innerRepository = innerRepository;
        }

        public async Task<ICollection<SalesOrderHeader>> GetTopTenSalesOrdersAsync()
        {
            return await CacheService.GetAsync<ICollection<SalesOrderHeader>>(
                "soh:topTen",
                () => _innerRepository.GetTopTenSalesOrdersAsync()).ConfigureAwait(false);
        }
    }
}
