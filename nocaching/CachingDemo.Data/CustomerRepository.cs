using CachingDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data
{
    public class CustomerRepository : ICustomerRepository
    {
        public CustomerRepository()
        {
        }

        public async Task<Customer> GetAsync(int id)
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.Customers
                    .Include(c => c.Person)
                    .Where(c => c.CustomerId == id)
                    .SingleOrDefaultAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}
