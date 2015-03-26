// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Web.Http;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml.Linq;
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

                    var firstRow = true;

                    var doc = new XDocument();
                    var order = new XElement("Order");
                    var customer = new XElement("Customer");
                    var lineItems = new XElement("OrderLineItems");

                    doc.Add(order);
                    order.Add(customer, lineItems);

                    while (await reader.ReadAsync())
                    {
                        if (firstRow)
                        {
                            firstRow = false;

                            var orderDate = (DateTime)reader["OrderDate"];

                            var totalDue = (Decimal)reader["TotalDue"];
                            var reviewRequired = totalDue > 5000
                                ? 'Y'
                                : 'N';

                            order.Add(
                                new XAttribute("OrderNumber", reader["OrderNumber"]),
                                new XAttribute("Status", reader["Status"]),
                                new XAttribute("ShipDate", reader["ShipDate"]),
                                new XAttribute("OrderDateYear", orderDate.Year),
                                new XAttribute("OrderDateMonth", orderDate.Month),
                                new XAttribute("DueDate", reader["DueDate"]),
                                new XAttribute("SubTotal", reader["SubTotal"]),
                                new XAttribute("TaxAmt", reader["TaxAmt"]),
                                new XAttribute("TotalDue", totalDue),
                                new XAttribute("ReviewRequired", reviewRequired));

                            var fullName = string.Join(" ",
                                reader["CustomerTitle"],
                                reader["CustomerFirstName"],
                                reader["CustomerMiddleName"],
                                reader["CustomerLastName"],
                                reader["CustomerSuffix"]
                                )
                                .Replace("  "," ") //remove double spaces
                                .Trim()
                                .ToUpper();

                            customer.Add(
                                new XAttribute("AccountNumber", reader["AccountNumber"]),
                                new XAttribute("FullName", fullName));
                        }

                        var productId = (int)reader["ProductID"];
                        var quantity = (short) reader["Quantity"];
                  
                        var inventoryCheckRequired = (productId > 710 && productId < 720 && quantity > 5)
                            ? 'Y'
                            : 'N';

                        lineItems.Add(
                            new XElement("LineItem",
                                new XAttribute("Quantity", quantity),
                                new XAttribute("UnitPrice", reader["UnitPrice"]),
                                new XAttribute("LineTotal", reader["LineTotal"]),
                                new XAttribute("ProductID", productId),
                                new XAttribute("InventoryCheckRequired", inventoryCheckRequired)
                                ));
                    }

                    return ResponseMessage(TooMuchProcSqlController.CreateMessageFrom(doc.ToString()));
                }
            }
        }
    }
}
