using RetrievingTooMuchData.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Configuration;

namespace RetrievingTooMuchData.WebApi.Controllers
{
    public class UnnecessaryRowsController : ApiController
    {
        [HttpGet]
        [Route("unnecessaryrows/sales/total_aggregate_on_client")]
        public async Task<decimal> GetTotalSalesAggregateOnClientAsync()
        {
            decimal total = 0;

            using (var context = GetEagerLoadingContext())
            {
                var salesPersons = await context.SalesPersons
                                                .Include(sp => sp.SalesOrderHeaders) //This include here forces eager loading.
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                foreach (var salesPerson in salesPersons)
                {
                    var orderHeaders = salesPerson.SalesOrderHeaders;

                    total += orderHeaders.Sum(x => x.TotalDue);
                }
            }

            return total;
        }

        [HttpGet]
        [Route("unnecessaryrows/sales/total_aggregate_on_database")]
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
            var connectionString = ConfigurationManager.ConnectionStrings["AdventureWorksContext"].ConnectionString;
            return new AdventureWorksContext(connectionString);
        }

        private AdventureWorksContext GetEagerLoadingContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AdventureWorksContext"].ConnectionString;
            var context = new AdventureWorksContext(connectionString);
            // load eagerly
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context;
        }
    }
}
