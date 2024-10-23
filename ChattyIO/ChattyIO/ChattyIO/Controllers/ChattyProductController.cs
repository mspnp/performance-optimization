using ChattyIO.DTOs;
using ChattyIO.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChattyIO.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChattyProductController(ILogger<ChattyProductController> logger, AdventureWorksProductContext context) : ControllerBase
    {

        [HttpGet("products/{subcategoryId}")]
        public async Task<IActionResult> GetProductsInSubCategoryAsync(int subcategoryId)
        {
            var productSubcategory = await context.ProductCategories
                      .Where(psc => psc.ProductCategoryID == subcategoryId)
                      .FirstOrDefaultAsync();

            if (productSubcategory == null)
            {
                // The subcategory was not found.
                return NotFound();
            }

            productSubcategory.Products = await context.Products
                  .Where(p => subcategoryId == p.ProductCategoryID)
                  .ToListAsync();

            foreach (var prod in productSubcategory.Products)
            {
                int productId = prod.ProductID;

                var salesOrderDetail = await context.SalesOrderDetails
                   .Where(sod => sod.ProductID == productId)
                   .ToListAsync();

                prod.SalesOrderDetails = salesOrderDetail;
            }

            var categories = new ProductCategoryDTO
            {
                ProductCategoryID = productSubcategory.ProductCategoryID,
                ParentProductCategoryID = productSubcategory.ParentProductCategoryID,
                Name = productSubcategory.Name,
                Rowguid = productSubcategory.Rowguid,
                ModifiedDate = productSubcategory.ModifiedDate,
                Products = productSubcategory.Products.Select(p => new ProductDTO
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    SalesOrderDetails = p.SalesOrderDetails.Select(od => new SalesOrderDetailDTO
                    {
                        SalesOrderDetailID = od.SalesOrderDetailID,
                        OrderQty = od.OrderQty,
                        ProductID = od.ProductID
                    }).ToList()
                })
                .ToList()
            };

            return Ok(categories);
        }
    }
}
