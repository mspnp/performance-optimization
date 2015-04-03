// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ExtraneousFetching.DataAccess;
using ExtraneousFetching.WebApi.Models;

namespace ExtraneousFetching.WebApi.Controllers
{
    public class UnnecessaryFieldsController : ApiController
    {
        [HttpGet]
        [Route("api/allfields")]
        public async Task<IHttpActionResult> GetAllFieldsAsync()
        {
            using (var context = new AdventureWorksContext())
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
            using (var context = new AdventureWorksContext())
            {
                var result = await context.Products
                    .Select(p => new ProductInfo {Id = p.ProductId, Name = p.Name}) // Project fields.
                    .ToListAsync(); // Execute query.

                return Ok(result);
            }
        }
    }
}