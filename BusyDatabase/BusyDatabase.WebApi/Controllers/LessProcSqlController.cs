// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Web.Http;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using BusyDatabase.Support;
using Microsoft.Azure;

namespace BusyDatabase.WebApi.Controllers
{
    public class LessProcSqlController : ApiController
    {
        private static readonly string SqlConnectionString = CloudConfigurationManager.GetSetting("connectionString");
        private static readonly string Query = Queries.Get("LessSql");

        public async Task<IHttpActionResult> Get(int id)
        {
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(Query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", id);

                    var reader = await command.ExecuteReaderAsync();

                    var doc = new XmlDocument();
                    var order = (XmlElement)doc.AppendChild(doc.CreateElement("Order"));

                    while (await reader.ReadAsync())
                    {
                        var shipDate = (DateTime)reader["ShipDate"];
                        // we only expected a single row in the first result set
                        order.SetAttribute("OrderNumber", reader["OrderNumber"].ToString());
                        order.SetAttribute("Status", reader["Status"].ToString());
                        order.SetAttribute("ShipDate", shipDate.ToString("s"));
                        order.SetAttribute("SubTotal", reader["SubTotal"].ToString());
                        order.SetAttribute("TaxAmt", reader["TaxAmt"].ToString());
                        order.SetAttribute("TotalDue", reader["TotalDue"].ToString());
                        order.SetAttribute("AccountNumber", reader["AccountNumber"].ToString());
                    }

                    await reader.NextResultAsync();

                    while (reader.Read())
                    {
                        var lineItem = (XmlElement) order.AppendChild(doc.CreateElement("LineItem"));
                        lineItem.SetAttribute("Quantity", reader["Quantity"].ToString());
                        lineItem.SetAttribute("UnitPrice", reader["UnitPrice"].ToString());
                        lineItem.SetAttribute("LineTotal", reader["LineTotal"].ToString());
                        lineItem.SetAttribute("ProductID", reader["ProductID"].ToString());
                    }

                    return BuildResponseForRawXml(doc.OuterXml);
                }
            }
        }

        private IHttpActionResult BuildResponseForRawXml(string result)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var mediaType = new MediaTypeHeaderValue("application/xml") { CharSet = "utf-8" };
            response.Content = new StringContent(result);
            response.Content.Headers.ContentType = mediaType;

            return ResponseMessage(response);
        }
    }
}
