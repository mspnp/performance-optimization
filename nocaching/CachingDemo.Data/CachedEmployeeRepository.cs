using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
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
            return await CacheService.GetAsync<Employee>("e:" + id, () => _innerRepository.GetAsync(id)).ConfigureAwait(false);
        }
    }
}
