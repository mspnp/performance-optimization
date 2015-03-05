using RetrievingTooMuchData.DataAccess;
using RetrievingTooMuchData.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Configuration;

namespace RetrievingTooMuchData.WebApi.Controllers
{
    public class UnnecessaryFieldsController : ApiController
    {
        [HttpGet]
        [Route("unnecessaryfields/products/project_all_fields")]
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
        [Route("unnecessaryfields/products/project_only_required_fields")]
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
            var connectionString = ConfigurationManager.ConnectionStrings["AdventureWorksContext"].ConnectionString;
            return new AdventureWorksContext(connectionString);
        }
    }
}
