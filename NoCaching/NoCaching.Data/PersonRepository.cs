// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NoCaching.Data.Models;

namespace NoCaching.Data
{
    public class PersonRepository : IPersonRepository
    {
        public async Task<Person> GetAsync(int id)
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.People
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}
