using CachingDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data
{
    public class CachedCustomerRepository : ICustomerRepository
    {
        private CustomerRepository innerRepository;

        public CachedCustomerRepository(CustomerRepository innerRepository)
        {
            this.innerRepository = innerRepository;
        }

        public async Task<Customer> GetAsync(int id)
        {
            return await CacheService.GetAsync<Customer>("c:" + id, () => this.innerRepository.GetAsync(id));
        }
    }
}
