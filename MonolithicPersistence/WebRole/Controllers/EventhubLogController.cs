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
    public class EventhubLogController : ApiController
    {
        static string eventHubName = CloudConfigurationManager.GetSetting("EventHubName");
        static string eventHubNamespace = CloudConfigurationManager.GetSetting("EventHubNamespace");
        static string devicesSharedAccessPolicyName = CloudConfigurationManager.GetSetting("LogPolicyName");
        static string devicesSharedAccessPolicyKey = CloudConfigurationManager.GetSetting("LogPolicyKey");
        static string eventHubConnectionString = string.Format("Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2};TransportType=Amqp",
            eventHubNamespace, devicesSharedAccessPolicyName, devicesSharedAccessPolicyKey);
        static EventHubClient client = EventHubClient.CreateFromConnectionString(eventHubConnectionString, eventHubName);

        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            LogMessage logMessage = new LogMessage();
            await LogToEventhubAsync(logMessage).ConfigureAwait(false);
            return Ok();
        }
        private static async Task LogToEventhubAsync(LogMessage logMessage)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings();
            var serializedString = JsonConvert.SerializeObject(logMessage);
            var bytes = Encoding.UTF8.GetBytes(serializedString);
            using (EventData data = new EventData(bytes))
            {
                await client.SendAsync(data).ConfigureAwait(false);
            }
        }
    }
}
