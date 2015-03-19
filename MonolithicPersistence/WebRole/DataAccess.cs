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
                    cmd.Parameters.Add("@RevisionNumber", SqlDbType.TinyInt).Value = 1;
                    cmd.Parameters.Add("@Status", SqlDbType.TinyInt).Value = 4;
                    cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = 258;
                    cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = 1580;
                    cmd.Parameters.Add("@ShipMethodID", SqlDbType.Int).Value = 3;
                    cmd.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = dt;
                    cmd.Parameters.Add("@ShipDate", SqlDbType.DateTime).Value = dt;
                    cmd.Parameters.Add("@SubTotal", SqlDbType.Money).Value = 123.40;
                    cmd.Parameters.Add("@TaxAmt", SqlDbType.Money).Value = 12.34;
                    cmd.Parameters.Add("@Freight", SqlDbType.Money).Value = 5.76;
                    cmd.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = dt;

                    await cn.OpenAsync();
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

                    await cn.OpenAsync();
                    return (string) await cmd.ExecuteScalarAsync().ConfigureAwait(false);
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
                    cmd.Parameters.Add("@LogId", SqlDbType.NChar, 32).Value = logMessage.LogId;
                    cmd.Parameters.Add("@Message", SqlDbType.NText).Value = logMessage.Message;
                    cmd.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = logMessage.LogTime;

                    await cn.OpenAsync();
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