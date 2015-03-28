// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class SingleHttpClientInstanceController : ApiController
    {
        private static readonly HttpClient HttpClient;

        static SingleHttpClientInstanceController()
        {
            HttpClient = new HttpClient();
        }

        /// <summary>
        /// This method uses the shared instance of HttpClient for every call to GetProductAsync.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Product> GetProductAsync(string id)
        {
            var hostName = HttpContext.Current.Request.Url.Host;
            var result = await HttpClient.GetStringAsync(string.Format("http://{0}:8080/api/userprofile", hostName));

            return new Product { Name = result };
        }
    }
}
