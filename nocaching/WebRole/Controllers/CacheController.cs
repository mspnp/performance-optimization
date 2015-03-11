using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using CachingDemo.Data;
using CachingDemo.Data.Models;

namespace WebRole.Controllers
{
    public class CacheController : ApiController
    {
        public async Task<Person> GetPerson(int id)
        {
            IPersonRepository repository = new CachedPersonRepository(new PersonRepository());
            return await repository.GetAsync(id);
        }

        public async Task<Customer> GetCustomer(int id)
        {
            ICustomerRepository repository = new CachedCustomerRepository(new CustomerRepository());
            return await repository.GetAsync(id);
        }

        public async Task<Employee> GetEmployee(int id)
        {
            IEmployeeRepository repository = new CachedEmployeeRepository(new EmployeeRepository());
            return await repository.GetAsync(id);
        }

        public async Task<ICollection<SalesOrderHeader>> GetTopTenSalesOrders()
        {
            ISalesOrderRepository repository = new CachedSalesOrderRepository(new SalesOrderRepository());
            return await repository.GetTopTenSalesOrdersAsync();
        }

        public async Task<ICollection<SalesPersonTotalSales>> GetTopTenSalesPeople()
        {
            ISalesPersonRepository repository = new CachedSalesPersonRepository(new SalesPersonRepository());
            return await repository.GetTopTenSalesPeopleAsync();
        }

        [HttpGet]
        public async Task<IHttpActionResult> Flush()
        {
            await CacheService.FlushAsync();
            return this.Ok();
        }
    }
}
