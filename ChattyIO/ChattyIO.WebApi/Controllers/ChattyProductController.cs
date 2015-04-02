// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ChattyIO.DataAccess;

namespace ChattyIO.WebApi.Controllers
{
    // We are using the db context directly here since the purpose of this is to illustrate perf anti-patterns.
    // Consider using the Repository pattern instead in a real app.

    public class ChattyProductController : ApiController
    {

        [HttpGet]
        [Route("chattyproduct/products/{subcategoryId}")]
        public async Task<IHttpActionResult> GetProductsInSubCategoryAsync(int subcategoryId)
        {
            using (var context = AdventureWorksProductContext.GetEagerContext())
            {
                var productSubcategory = await context.ProductSubcategories
                       .Where(psc => psc.ProductSubcategoryId == subcategoryId)
                       .FirstOrDefaultAsync();

                if (productSubcategory == null)
                {
                    // The subcategory was not found.
                    return NotFound();
                }

                productSubcategory.Product = await context.Products
                    .Where(p => subcategoryId == p.ProductSubcategoryId)
                    .ToListAsync();

                foreach (var prod in productSubcategory.Product)
                {
                    int productId = prod.ProductId;

                    var productListPriceHistory = await context.ProductListPriceHistory
                       .Where(pl => pl.ProductId == productId)
                       .ToListAsync();

                    prod.ProductListPriceHistory = productListPriceHistory;
                }

                return Ok(productSubcategory);
            }
        }
    }
}
