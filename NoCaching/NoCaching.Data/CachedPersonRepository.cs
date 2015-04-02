// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using NoCaching.Data.Models;

namespace NoCaching.Data
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
            return await CacheService.GetAsync("p:" + id, () => _innerRepository.GetAsync(id)).ConfigureAwait(false);
        }
    }
}
