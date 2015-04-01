// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using ImproperInstantiation.WebRole.Models;

namespace ImproperInstantiation.WebRole.Controllers
{
    public class NewServiceInstancePerRequestController : ApiController
    {
        /// <summary>
        /// This method creates a new instance of ExpensiveToCreateService and disposes it for every call to GetProductAsync.
        /// </summary>
        public async Task<Product> GetProductAsync(string id)
        {
            var expensiveToCreateService = new ExpensiveToCreateService();
            return await expensiveToCreateService.GetProductByIdAsync(id);
        }
    }
}
