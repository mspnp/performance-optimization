// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using NoCaching.Data.Models;

namespace NoCaching.Data
{
    public class CachedCustomerRepository : ICustomerRepository
    {
        private readonly CustomerRepository _innerRepository;

        public CachedCustomerRepository(CustomerRepository innerRepository)
        {
            _innerRepository = innerRepository;
        }

        public async Task<Customer> GetAsync(int id)
        {
            return await CacheService.GetAsync("c:" + id, () => _innerRepository.GetAsync(id)).ConfigureAwait(false);
        }
    }
}
