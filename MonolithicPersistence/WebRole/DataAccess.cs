// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using WebRole.Models;

namespace WebRole
{
    public static class DataAccess
    {
        private static readonly EventHubClient EvtHubClient;
        private static readonly string SqlDbConnectionString;

        static DataAccess()
        {
            string eventHubName = CloudConfigurationManager.GetSetting("EventHubName");
            string eventHubNamespace = CloudConfigurationManager.GetSetting("EventHubNamespace");
            string devicesSharedAccessPolicyName = CloudConfigurationManager.GetSetting("LogPolicyName");
            string devicesSharedAccessPolicyKey = CloudConfigurationManager.GetSetting("LogPolicyKey");
            string eventHubConnectionString = string.Format("Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2};TransportType=Amqp",
                eventHubNamespace, devicesSharedAccessPolicyName, devicesSharedAccessPolicyKey);

            EvtHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, eventHubName);
            SqlDbConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
        }

        public static async Task InsertToPurchaseOrderHeaderTableAsync()
        {
            const string queryString =
                "INSERT INTO Purchasing.PurchaseOrderHeader " +
                "(RevisionNumber, Status, EmployeeID, VendorID, ShipMethodID, OrderDate, ShipDate, SubTotal, TaxAmt, Freight, ModifiedDate) " +
                "VALUES " +
                "(@RevisionNumber, @Status, @EmployeeID, @VendorID, @ShipMethodID, @OrderDate, @ShipDate, @SubTotal, @TaxAmt, @Freight, @ModifiedDate)";

            var dt = DateTime.UtcNow;

            using (var cn = new SqlConnection(SqlDbConnectionString))
            {
                using (var cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@RevisionNumber", 1);
                    cmd.Parameters.AddWithValue("@Status", 4);
                    cmd.Parameters.AddWithValue("@EmployeeID", 258);
                    cmd.Parameters.AddWithValue("@VendorID", 1580);
                    cmd.Parameters.AddWithValue("@ShipMethodID", 3);
                    cmd.Parameters.AddWithValue("@OrderDate", dt);
                    cmd.Parameters.AddWithValue("@ShipDate", dt);
                    cmd.Parameters.AddWithValue("@SubTotal", 123.40M);
                    cmd.Parameters.AddWithValue("@TaxAmt", 12.34M);
                    cmd.Parameters.AddWithValue("@Freight", 5.76M);
                    cmd.Parameters.AddWithValue("@ModifiedDate", dt);

                    await cn.OpenAsync().ConfigureAwait(false);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task<string> SelectProductDescriptionAsync(int id)
        {
            const string queryString = "SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=@inputId";

            using (var cn = new SqlConnection(SqlDbConnectionString))
            {
                using (var cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@inputId", id);

                    await cn.OpenAsync().ConfigureAwait(false);
                    return (string)await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task LogToSqldbAsync(LogMessage logMessage)
        {
            const string queryString = "INSERT INTO dbo.SqldbLog(Message, LogId, LogTime) VALUES(@Message, @LogId, @LogTime)";

            using (var cn = new SqlConnection(SqlDbConnectionString))
            {
                using (var cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@LogId", logMessage.LogId);
                    cmd.Parameters.AddWithValue("@Message", logMessage.Message);
                    cmd.Parameters.AddWithValue("@LogTime", logMessage.LogTime);

                    await cn.OpenAsync().ConfigureAwait(false);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task LogToEventhubAsync(LogMessage logMessage)
        {
            var json = JsonConvert.SerializeObject(logMessage);
            var bytes = Encoding.UTF8.GetBytes(json);

            using (var data = new EventData(bytes))
            {
                await EvtHubClient.SendAsync(data).ConfigureAwait(false);
            }
        }
    }
}