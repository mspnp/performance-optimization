// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure;
using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class PolyglotController : ApiController
    {
        private static readonly string ProductionDb;
        private static readonly string LogDb;

        static PolyglotController()
        {
            ProductionDb = CloudConfigurationManager.GetSetting("ProductionSqlDbCnStr");

            LogDb = CloudConfigurationManager.GetSetting("LogSqlDbCnStr");
        }

        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);

            await DataAccess.LogAsync(LogDb);

            return Ok();
        }

    }
}
