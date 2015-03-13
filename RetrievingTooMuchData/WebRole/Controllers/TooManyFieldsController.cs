using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure;
using RetrievingTooMuchData.DataAccess;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class TooManyFieldsController : ApiController
    {
        [HttpGet]
        [Route("toomanyfields/products/project_all_fields")]
        public async Task<IEnumerable<ProductInfo>> GetProductsProjectAllFieldsAsync()
        {
            using (var context = GetContext())
            {
                var products = await context.Products
                                            .ToListAsync() //Execute query.
                                            .ConfigureAwait(false);

                return products.Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name }); //Project fields.;            
            }
        }

        [HttpGet]
        [Route("toomanyfields/products/project_only_required_fields")]
        public async Task<IEnumerable<ProductInfo>> GetProductsProjectOnlyRequiredFieldsAsync()
        {
            using (var context = GetContext())
            {
                return await context.Products
                                    .Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name }) //Project fields.
                                    .ToListAsync() //Execute query.
                                    .ConfigureAwait(false);
            }
        }

        private AdventureWorksContext GetContext()
        {
            var connectionString = CloudConfigurationManager.GetSetting("AdventureWorksContext");
            return new AdventureWorksContext(connectionString);
        }
    }
}
