// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class NewServiceInstancePerRequestController : ApiController
    {
        /// <summary>
        /// This method creates a new instance of ProductRepository and disposes it for every call to GetProductAsync.
        /// </summary>
        public async Task<Product> GetProductAsync(string id)
        {
            var expensiveToCreateService = new ExpensiveToCreateService();
            return await expensiveToCreateService.GetProductByIdAsync(id);
        }
    }
}
