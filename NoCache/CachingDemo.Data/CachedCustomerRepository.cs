// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
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
            return await CacheService.GetAsync<Customer>("c:" + id, () => _innerRepository.GetAsync(id)).ConfigureAwait(false);
        }
    }
}
