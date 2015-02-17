using CachingDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public EmployeeRepository()
        {
        }

        public async Task<Employee> GetAsync(int id)
        {
            using (var context = new AdventureWorksContext())
            {
                return await context.Employees
                    .Where(e => e.Id == id)
                    .SingleOrDefaultAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}
