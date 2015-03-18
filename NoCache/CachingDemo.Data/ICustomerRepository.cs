using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public interface ICustomerRepository
    {
        Task<Customer> GetAsync(int id);
    }
}
