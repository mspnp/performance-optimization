using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetAsync(int id);
    }
}
