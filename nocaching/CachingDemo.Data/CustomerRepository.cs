using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class CustomerRepository : ICustomerRepository
    {
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
