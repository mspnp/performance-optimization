using CachingDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data
{
    public class CachedEmployeeRepository : IEmployeeRepository
    {
        private EmployeeRepository innerRepository;

        public CachedEmployeeRepository(EmployeeRepository innerRepository)
        {
            this.innerRepository = innerRepository;
        }

        public async Task<Employee> GetAsync(int id)
        {
            return await CacheService.GetAsync<Employee>("e:" + id, () => this.innerRepository.GetAsync(id));
        }
    }
}
