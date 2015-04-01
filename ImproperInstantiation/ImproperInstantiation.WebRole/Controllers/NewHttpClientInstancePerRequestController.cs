// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class NewHttpClientInstancePerRequestController : ApiController
    {
        /// <summary>
        /// This method creates a new instance of HttpClient and disposes it for every call to GetProductAsync.
        /// </summary>
        public async Task<Product> GetProductAsync(string id)
        {
            using (var httpClient = new HttpClient())
            {
                var hostName = HttpContext.Current.Request.Url.Host;
                var result = await httpClient.GetStringAsync(string.Format("http://{0}:8080/api/userprofile", hostName));

                return new Product { Name = result };
            }
        }
    }
}
