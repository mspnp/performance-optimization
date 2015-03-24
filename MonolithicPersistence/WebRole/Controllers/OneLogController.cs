// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure;
using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class OneLogController : ApiController
    {
        private static readonly string LogDb = CloudConfigurationManager.GetSetting("LogSqlDbCnStr");
        public const string LogTableName = "OneLog";

        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            await DataAccess.LogAsync(LogDb, LogTableName);

            return Ok();
        }
    }
}