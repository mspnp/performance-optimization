// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class CachedPersonRepository : IPersonRepository
    {
        private readonly PersonRepository _innerRepository;

        public CachedPersonRepository(PersonRepository innerRepository)
        {
            _innerRepository = innerRepository;
        }

        public async Task<Person> GetAsync(int id)
        {
            return await CacheService.GetAsync<Person>("p:" + id, () => _innerRepository.GetAsync(id)).ConfigureAwait(false);
        }
    }
}
