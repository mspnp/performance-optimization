using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure;
using RetrievingTooMuchData.DataAccess;

namespace WebRole.Controllers
{
    public class TooManyRowsController : ApiController
    {
        [HttpGet]
        [Route("toomanyrows/sales/total_aggregate_on_client")]
        public async Task<decimal> GetTotalSalesAggregateOnClientAsync()
        {
            using (var context = GetContext())
            {
                var salesPersons = await context.SalesPersons
                                                .Include(sp => sp.SalesOrderHeaders) // This include forces eager loading.
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                decimal total = 0;
                foreach (var salesPerson in salesPersons)
                {
                    var orderHeaders = salesPerson.SalesOrderHeaders;

                    total += orderHeaders.Sum(x => x.TotalDue);
                }

                return total;
            }
        }

        [HttpGet]
        [Route("toomanyrows/sales/total_aggregate_on_database")]
        public async Task<decimal> GetTotalSalesAsync()
        {
            using (var context = GetContext())
            {
                var query = from sp in context.SalesPersons
                            from soh in sp.SalesOrderHeaders
                            select soh.TotalDue;

                return await query.DefaultIfEmpty(0)
                                  .SumAsync()
                                  .ConfigureAwait(false);
            }
        }

        private AdventureWorksContext GetContext()
        {
            var connectionString = CloudConfigurationManager.GetSetting("AdventureWorksContext");
            return new AdventureWorksContext(connectionString);
        }
    }
}
