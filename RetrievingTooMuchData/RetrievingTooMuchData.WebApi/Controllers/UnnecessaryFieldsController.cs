// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
        public async Task<IEnumerable<ProductInfo>> GetAllFieldsAsync()
        {
            using (var context = GetContext())
            {
                var products = await context.Products.ToListAsync(); // Execute query.

                return products.Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name }); // Project fields.
            }
        }

        [HttpGet]
        [Route("api/requiredfields")]
        public async Task<IEnumerable<ProductInfo>> GetRequiredFieldsAsync()
        {
            using (var context = GetContext())
            {
                return await context.Products
                                    .Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name }) // Project fields.
                                    .ToListAsync(); // Execute query.
            }
        }

        private AdventureWorksContext GetContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AdventureWorksContext"].ConnectionString;
            return new AdventureWorksContext(connectionString);
        }
    }
}