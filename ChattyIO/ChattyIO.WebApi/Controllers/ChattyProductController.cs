namespace ChattyIO.Api.Web.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using ChattyIO.DataAccess;

    //We are using the context directly here since the purpose of this is to illustrate perf anti-patterns
    //consider using the Repository pattern instead in a real app.
    public class ChattyProductController : ApiController
    {

        [HttpGet]
        [Route("chattyproduct/products/{subcategoryId}")]
        public async Task<ProductSubcategory> GetProductsInSubCategoryAsync(int subcategoryId)
        {
            ProductSubcategory productSubcategory = null;
     
            using (var context = GetContext())
            {
               productSubcategory = await context.ProductSubcategories
                      .Where((psc) => psc.ProductSubcategoryId == subcategoryId)
                      .SingleOrDefaultAsync();
                productSubcategory.Product = await context.Products
                    .Where((p) => subcategoryId == p.ProductSubcategoryId)
                    .ToListAsync();

                foreach (var prod in productSubcategory.Product)
                {
                    var productListPriceHistory = await context.ProductListPriceHistory
                       .Where((pl) => pl.ProductId == prod.ProductId)
                       .ToListAsync();
                    prod.ProductListPriceHistory = productListPriceHistory;
                }

            }
            return productSubcategory;
        }


        private AdventureWorksProductContext GetContext()
        {
            var context = new AdventureWorksProductContext();
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context;
        }
    }
}
