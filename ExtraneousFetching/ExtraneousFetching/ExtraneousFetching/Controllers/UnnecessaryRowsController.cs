using ExtraneousFetching.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExtraneousFetching.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UnnecessaryRowsController(AdventureWorksProductContext context) : Controller
    {
        [HttpGet]
        [Route("api/aggregateonclient")]
        public async Task<IActionResult> AggregateOnClientAsync()
        {
            // fetch all order totals from the database
            var orderAmounts = await context.SalesOrderHeaders.Select(soh => soh.TotalDue).ToListAsync();

            // sum the order totals here in the controller
            var total = orderAmounts.Sum();

            return Ok(total);
        }

        [HttpGet]
        [Route("api/aggregateondatabase")]
        public async Task<IActionResult> AggregateOnDatabaseAsync()
        {
            // fetch the sum of all order totals, as computed on the database server
            var total = await context.SalesOrderHeaders.SumAsync(soh => soh.TotalDue);

            return Ok(total);
        }
    }
}
