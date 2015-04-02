// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using NoCaching.Data.Models;

namespace NoCaching.Data
{
    public class CachedEmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeRepository _innerRepository;

        public CachedEmployeeRepository(EmployeeRepository innerRepository)
        {
            _innerRepository = innerRepository;
        }

        public async Task<Employee> GetAsync(int id)
        {
            return await CacheService.GetAsync("e:" + id, () => _innerRepository.GetAsync(id)).ConfigureAwait(false);
        }
    }
}
