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
        [Route("chattyproduct/{categoryId}")]
        public async Task<ProductCategory> GetProductCategory(int categoryId)
        {
            using (var context = GetContext())
            {
                return await context.ProductCategories
                    .Where((pc) => pc.ProductCategoryId == categoryId)
                    .SingleOrDefaultAsync();
            }
        }

        [Route("chattyproduct/productsubcategories/{categoryId}")]
        public async Task<IEnumerable<ProductSubcategory>> GetProductSubCategoriesInCategory(int categoryId)
        {
            using (var context = GetContext())
            {
                return await context.ProductSubcategories
                     .Where((ps) => ps.ProductCategoryId == categoryId)
                     .ToListAsync();
            }
        }

        [Route("chattyproduct/products/{subcategoryId}")]
        public async Task<IEnumerable<Product>> GetProductsInSubCategory(int subcategoryId)
        {
            using (var context = GetContext())
            {
                return await context.Products
                    .Where((p) => p.ProductSubcategoryId == subcategoryId)
                    .ToListAsync();
            }
        }

        [Route("chattyproduct/productlistpricehistory/{productId}")]
        public async Task<IEnumerable<ProductListPriceHistory>> GetProductListPriceHistory(int productId)
        {
            using (var context = GetContext())
            {
                return await context.ProductListPriceHistory
                    .Where((plphist) => plphist.ProductId == productId)
                    .ToListAsync();
            }
        }

        private AdventureWorksProductContext GetContext()
        {
            var context = new AdventureWorksProductContext();
            // load eagerly
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context;
        }
    }
}
