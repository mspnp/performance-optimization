// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using RetrievingTooMuchData.DataAccess;
using RetrievingTooMuchData.WebApi.Models;

namespace RetrievingTooMuchData.WebApi.Controllers
{
    public class UnnecessaryFieldsController : ApiController
    {
        [HttpGet]
        [Route("api/allfields")]
        public async Task<IHttpActionResult> GetAllFieldsAsync()
        {
            using (var context = GetContext())
            {
                var products = await context.Products.ToListAsync(); // Execute query.

                var result = products.Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name }); // Project fields.

                return Ok(result);
            }
        }

        [HttpGet]
        [Route("api/requiredfields")]
        public async Task<IHttpActionResult> GetRequiredFieldsAsync()
        {
            using (var context = GetContext())
            {
                var result = await context.Products
                    .Select(p => new ProductInfo {Id = p.ProductId, Name = p.Name}) // Project fields.
                    .ToListAsync(); // Execute query.

                return Ok(result);
            }
        }

        private AdventureWorksContext GetContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AdventureWorksContext"].ConnectionString;
            return new AdventureWorksContext(connectionString);
        }
    }
}