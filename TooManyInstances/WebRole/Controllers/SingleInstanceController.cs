// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class SingleInstanceController : ApiController
    {
        private static readonly IProductRepository ProductRepository;

        static SingleInstanceController()
        {
            ProductRepository = new ProductRepository();
        }

        /// <summary>
        /// This method uses the shared instance of IProductRepository for every call to GetProductAsync.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Product> GetProductAsync(string id)
        {
            return await ProductRepository.GetProductByIdAsync(id);
        }
    }
}
