// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using NoCaching.Data;

namespace NoCaching.WebRole.Controllers
{
    public class NoCacheController : ApiController
    {
        public async Task<IHttpActionResult> GetPerson(int id)
        {
            var repository = new PersonRepository();
            var person = await repository.GetAsync(id);

            if (person == null) return NotFound();
            return Ok(person);
        }

        public async Task<IHttpActionResult> GetCustomer(int id)
        {
            var repository = new CustomerRepository();
            var customer = await repository.GetAsync(id);

            if (customer == null) return NotFound();
            return Ok(customer);
        }

        public async Task<IHttpActionResult> GetEmployee(int id)
        {
            var repository = new EmployeeRepository();
            var employee = await repository.GetAsync(id);

            if (employee == null) return NotFound();
            return Ok(employee);
        }

        public async Task<IHttpActionResult> GetTopTenSalesOrders()
        {
            var repository = new SalesOrderRepository();
            var results = await repository.GetTopTenSalesOrdersAsync();

            if (results == null) return NotFound();
            return Ok(results);
        }

        public async Task<IHttpActionResult> GetTopTenSalesPeople()
        {
            var repository = new SalesPersonRepository();
            var results = await repository.GetTopTenSalesPeopleAsync();

            if (results == null) return NotFound();
            return Ok(results);
        }
    }
}
