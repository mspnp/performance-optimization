// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class MonolithicController : ApiController
    {
        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            int logCount = Convert.ToInt32(value);
            for (int i = 0; i < logCount; i++)
            {
                LogMessage logMessage = new LogMessage();
                await DataAccess.LogToSqldbAsync(logMessage);
            }
            await DataAccess.SelectProductDescriptionAsync(321);
            await DataAccess.InsertToPurchaseOrderHeaderTableAsync();
            return Ok();
        }

    }
}
