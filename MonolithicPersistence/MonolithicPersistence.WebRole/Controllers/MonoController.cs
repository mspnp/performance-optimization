// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure;

namespace MonolithicPersistence.WebRole.Controllers
{
    public class MonoController : ApiController
    {
        private static readonly string ProductionDb = CloudConfigurationManager.GetSetting("ProductionSqlDbCnStr");
        public const string LogTableName = "MonoLog";

        public async Task<IHttpActionResult> PostAsync()
        {
            await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);

            await DataAccess.LogAsync(ProductionDb, LogTableName);

            return Ok();
        }
    }
}
