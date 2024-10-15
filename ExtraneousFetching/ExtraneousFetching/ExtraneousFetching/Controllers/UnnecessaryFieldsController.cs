using ExtraneousFetching.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExtraneousFetching.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UnnecessaryFieldsController(AdventureWorksProductContext context) : ControllerBase
    {
        [HttpGet]
        [Route("api/allfields")]
        public async Task<IActionResult> GetAllFieldsAsync()
        {
            // execute the query
            var products = await context.Products.ToListAsync();

            // project fields from the query results
            var result = products.Select(p => new DTOs.ProductInfo { Id = p.ProductID, Name = p.Name });

            return Ok(result);
        }

        [HttpGet]
        [Route("api/requiredfields")]
        public async Task<IActionResult> GetRequiredFieldsAsync()
        {
            // project fields as part of the query itself
            var result = await context.Products
                .Select(p => new DTOs.ProductInfo { Id = p.ProductID, Name = p.Name })
                .ToListAsync();

            return Ok(result);
        }
    }
}
