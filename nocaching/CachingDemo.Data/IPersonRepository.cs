using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public interface IPersonRepository
    {
        Task<Person> GetAsync(int id);
    }
}
