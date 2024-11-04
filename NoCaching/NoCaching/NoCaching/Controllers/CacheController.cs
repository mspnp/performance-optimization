using Microsoft.AspNetCore.Mvc;

namespace NoCaching.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheController(ICacheQueryService _cacheQueryService) : ControllerBase
    {
        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _cacheQueryService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("productCategories/{productSubcategoryId}")]
        public async Task<IActionResult> GetProductCategories(int productSubcategoryId)
        {
            var product = await _cacheQueryService.GetProductCategoryAsync(productSubcategoryId);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}
