// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public async Task<Employee> GetAsync(int id)
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.Employees
                    .Where(e => e.Id == id)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}
