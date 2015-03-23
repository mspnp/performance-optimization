// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class SingleServiceInstanceController : ApiController
    {
        private static readonly ExpensiveToCreateService ExpensiveToCreateService;

        static SingleServiceInstanceController()
        {
            ExpensiveToCreateService = new ExpensiveToCreateService();
        }

        /// <summary>
        /// This method uses the shared instance of ExpensiveToCreateService for every call to GetProductAsync.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Product> GetProductAsync(string id)
        {
            return await ExpensiveToCreateService.GetProductByIdAsync(id);
        }
    }
}
