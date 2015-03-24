// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure;
using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{

    public class NoLogController : ApiController
    {
        private static readonly string ProductionDb = CloudConfigurationManager.GetSetting("ProductionSqlDbCnStr");

        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            string categoryName;
            string productDescription;

            categoryName = await DataAccess.SelectProductCategoryAsync(ProductionDb);

            productDescription = await DataAccess.SelectProductDescriptionAsync(ProductionDb);

            await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);

            await DataAccess.InsertPurchaseOrderDetailAsync(ProductionDb);

            await DataAccess.InsertPurchaseOrderDetailAsync(ProductionDb);

            return Ok();
        }
    }
}
