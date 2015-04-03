// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ExtraneousFetching.DataAccess;

namespace ExtraneousFetching.WebApi.Controllers
{
    public class UnnecessaryRowsController : ApiController
    {
        [HttpGet]
        [Route("api/aggregateonclient")]
        public async Task<IHttpActionResult> AggregateOnClientAsync()
        {
            using (var context = new AdventureWorksContext())
            {
                // fetch all order totals from the database
                var orderAmounts = await context.SalesOrderHeaders.Select(soh => soh.TotalDue).ToListAsync();

                // sum the order totals here in the controller
                var total = orderAmounts.Sum();

                return Ok(total);
            }
        }

        [HttpGet]
        [Route("api/aggregateondatabase")]
        public async Task<IHttpActionResult> AggregateOnDatabaseAsync()
        {
            using (var context = new AdventureWorksContext())
            {
                // fetch the sum of all order totals, as computed on the database server
                var total = await context.SalesOrderHeaders.SumAsync(soh => soh.TotalDue);

                return Ok(total);
            }
        }
    }
}