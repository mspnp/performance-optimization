// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using RetrievingTooMuchData.DataAccess;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace RetrievingTooMuchData.WebApi.Controllers
{
    public class UnnecessaryRowsController : ApiController
    {
        [HttpGet]
        [Route("api/aggregateonclient")]
        public async Task<decimal> AggregateOnClientAsync()
        {
            decimal total = 0;

            using (var context = GetEagerLoadingContext())
            {
                var salesPersons = await context.SalesPersons
                                                .Include(sp => sp.SalesOrderHeaders) // This include forces eager loading.
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
        [Route("api/aggregateondatabase")]
        public async Task<decimal> AggregateOnDatabaseAsync()
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