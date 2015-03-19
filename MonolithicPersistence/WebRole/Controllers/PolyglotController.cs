// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class PolyglotController : ApiController
    {
        public async Task<IHttpActionResult> PostAsync([FromBody]int logCount)
        {
            for (int i = 0; i < logCount; i++)
            {
                var logMessage = new LogMessage();
                await DataAccess.LogToEventhubAsync(logMessage);
            }

            await DataAccess.SelectProductDescriptionAsync(321);
            await DataAccess.InsertToPurchaseOrderHeaderTableAsync();

            return Ok();
        }
    }
}
