// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure;
using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class PolyController : ApiController
    {
        private static readonly string ProductionDb = CloudConfigurationManager.GetSetting("ProductionSqlDbCnStr");
        private static readonly string LogDb = CloudConfigurationManager.GetSetting("LogSqlDbCnStr");
        public const string LogTableName = "PolyglotLog";

        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            string categoryName;
            string productDescription;

            categoryName = await DataAccess.SelectProductCategoryAsync(ProductionDb);
            await DataAccess.LogAsync(LogDb, LogTableName);

            productDescription = await DataAccess.SelectProductDescriptionAsync(ProductionDb);
            await DataAccess.LogAsync(LogDb, LogTableName);

            await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);
            await DataAccess.LogAsync(LogDb, LogTableName);

            await DataAccess.InsertPurchaseOrderDetailAsync(ProductionDb);
            await DataAccess.LogAsync(LogDb, LogTableName);

            await DataAccess.InsertPurchaseOrderDetailAsync(ProductionDb);
            await DataAccess.LogAsync(LogDb, LogTableName);

            return Ok();
        }

    }
}
