// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebRole;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;
using System.Text;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class PolyglotController : ApiController
    {

        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            int logCount = Convert.ToInt32(value);
            for (int i = 0; i < logCount; i++)
            {
                LogMessage logMessage = new LogMessage();
                await DataAccess.LogToEventhubAsync(logMessage);
            }
            await DataAccess.SelectProductDescriptionAsync(321);
            await DataAccess.InsertToPurchaseOrderHeaderTableAsync();
            return Ok();
        }
    }
}
