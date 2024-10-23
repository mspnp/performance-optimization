using ChattyIO.DTOs;
using ChattyIO.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChattyIO.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChunkyProductController(ILogger<ChunkyProductController> logger, AdventureWorksProductContext context) : ControllerBase
    {

        [HttpGet("products/{subcategoryId}")]
        public async Task<IActionResult> GetProductCategoryDetailsAsync(int subcategoryId)
        {
            var categories = await context.ProductCategories
                            .Include(x => x.Products) // Include related Products
                            .ThenInclude(p => p.SalesOrderDetails) // Then include SalesOrderDetails for each Product
                            .Where(x => x.ProductCategoryID == subcategoryId)
                            .Select(x => new ProductCategoryDTO
                            {
                                ProductCategoryID = x.ProductCategoryID,
                                ParentProductCategoryID = x.ParentProductCategoryID,
                                Name = x.Name,
                                Rowguid = x.Rowguid,
                                ModifiedDate = x.ModifiedDate,
                                Products = x.Products.Select(p => new ProductDTO
                                {
                                    ProductID = p.ProductID,
                                    Name = p.Name,
                                    SalesOrderDetails = p.SalesOrderDetails.Select(od => new SalesOrderDetailDTO
                                    {
                                        SalesOrderDetailID = od.SalesOrderDetailID,
                                        OrderQty = od.OrderQty,
                                        ProductID = od.ProductID
                                    }).ToList()
                                }).ToList()
                            })
                            .FirstOrDefaultAsync();

            return Ok(categories);
        }
    }
}
