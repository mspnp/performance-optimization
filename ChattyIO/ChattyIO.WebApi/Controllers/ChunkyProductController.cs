using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using ChattyIO.DataAccess;
using ChattyIO.DataAccess.Models;

namespace ChattyIO.WebApi.Controllers
{
    public class ChunkyProductController : ApiController
    {
        [HttpGet]
        [Route("chunkyproduct/products/{subCategoryId}")]
        public async Task<ProductSubcategory> GetProductCategoryDetailsAsync(int subCategoryId)
        {
            using (var context = GetContext())
            {
                var subCategory = await context.ProductSubcategories
                      .Where(psc => psc.ProductSubcategoryId == subCategoryId)
                      .Include("Product.ProductListPriceHistory")
                      .FirstOrDefaultAsync();
                return subCategory;
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

