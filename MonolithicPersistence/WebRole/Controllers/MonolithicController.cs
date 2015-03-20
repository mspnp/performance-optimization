// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure;
using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{

    public class MonolithicController : ApiController
    {
        private static readonly string ProductionDb;

        static MonolithicController()
        {
            ProductionDb = CloudConfigurationManager.GetSetting("ProductionSqlDbCnStr");
        }

        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);

            await DataAccess.LogAsync(ProductionDb);

            return Ok();
        }
    }
}
