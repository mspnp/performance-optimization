using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public class CachedEmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeRepository innerRepository;

        public CachedEmployeeRepository(EmployeeRepository innerRepository)
        {
            this.innerRepository = innerRepository;
        }

        public async Task<Employee> GetAsync(int id)
        {
            return await CacheService.GetAsync<Employee>("e:" + id, () => this.innerRepository.GetAsync(id)).ConfigureAwait(false);
        }
    }
}
