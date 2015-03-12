using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ChattyIO.DataAccess;
using ChattyIO.DataAccess.Models;

namespace ChattyIO.WebApi.Controllers
{
    // We are using the db context directly here since the purpose of this is to illustrate perf anti-patterns.
    // Consider using the Repository pattern instead in a real app.

    public class ChattyProductController : ApiController
    {

        [HttpGet]
        [Route("chattyproduct/products/{subcategoryId}")]
        public async Task<ProductSubcategory> GetProductsInSubCategoryAsync(int subcategoryId)
        {
            using (var context = GetContext())
            {
                var productSubcategory = await context.ProductSubcategories
                       .Where(psc => psc.ProductSubcategoryId == subcategoryId)
                       .FirstOrDefaultAsync();

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

                return productSubcategory;
            }
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
