using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class CachedPersonRepository : IPersonRepository
    {
        private PersonRepository innerRepository;

        public CachedPersonRepository(PersonRepository innerRepository)
        {
            this.innerRepository = innerRepository;
        }

        public async Task<Person> GetAsync(int id)
        {
            return await CacheService.GetAsync<Person>("p:" + id, () => this.innerRepository.GetAsync(id));
        }
    }
}
