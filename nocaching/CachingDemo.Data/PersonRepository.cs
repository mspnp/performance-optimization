using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class PersonRepository : IPersonRepository
    {
        public PersonRepository()
        {
        }

        public async Task<Person> GetAsync(int id)
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.People
                    .Where(p => p.Id == id)
                    .SingleOrDefaultAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}
