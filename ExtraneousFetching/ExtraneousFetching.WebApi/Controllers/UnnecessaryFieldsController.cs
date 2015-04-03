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
                // execute the query
                var products = await context.Products.ToListAsync();

                // project fields from the query results
                var result = products.Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name });

                return Ok(result);
            }
        }

        [HttpGet]
        [Route("api/requiredfields")]
        public async Task<IHttpActionResult> GetRequiredFieldsAsync()
        {
            using (var context = new AdventureWorksContext())
            {
                // project fields as part of the query itself
                var result = await context.Products
                    .Select(p => new ProductInfo {Id = p.ProductId, Name = p.Name})
                    .ToListAsync();

                return Ok(result);
            }
        }
    }
}