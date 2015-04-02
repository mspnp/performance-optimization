// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ChattyIO.DataAccess;

namespace ChattyIO.WebApi.Controllers
{
    public class ChunkyProductController : ApiController
    {
        [HttpGet]
        [Route("chunkyproduct/products/{subCategoryId}")]
        public async Task<IHttpActionResult> GetProductCategoryDetailsAsync(int subCategoryId)
        {
            using (var context = AdventureWorksProductContext.GetEagerContext())
            {
                var subCategory = await context.ProductSubcategories
                      .Where(psc => psc.ProductSubcategoryId == subCategoryId)
                      .Include("Product.ProductListPriceHistory")
                      .FirstOrDefaultAsync();

                if (subCategory == null)
                    return NotFound();
                
                return Ok(subCategory);
            }
        }
    }
}

