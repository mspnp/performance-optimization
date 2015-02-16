namespace ChattyIO.Api.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Data.Entity;

    using ChattyIO.DataAccess;

    public class ChunkyProductController : ApiController
    {
        [Route("chunkyproduct/{categoryId}")]
        public async Task<ProductCategory> GetProductCategoryDetailsAsync(int categoryId)
        {
            using (var context = GetContext())
            {
                var category = await context.ProductCategories
                      .Where((pc) => pc.ProductCategoryId == categoryId)
                      .Include("ProductSubcategory.Product.ProductListPriceHistory")
                      .SingleOrDefaultAsync();
                return category;
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

