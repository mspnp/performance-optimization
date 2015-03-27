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

        private static string RoundAndFormat(object value)
        {
            // the rounding is redundant, but this experiment is about
            // relocating the work not making the work meaningful :-)
            var currency = Math.Round((Decimal)value, 2);
            return currency.ToString("C");
        }

        public async Task<IHttpActionResult> Get(int id)
        {
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(Query, connection))
                {
                    command.Parameters.AddWithValue("@TerritoryId", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var lastOrderNumber = string.Empty;

                        var doc = new XDocument();
                        var orders = new XElement("Orders");

                        doc.Add(orders);
                        XElement lineItems = null;

                        while (await reader.ReadAsync())
                        {
                            var orderNumber = reader["OrderNumber"].ToString();

                            if (orderNumber != lastOrderNumber)
                            {
                                lastOrderNumber = orderNumber;

                                var order = new XElement("Order");
                                orders.Add(order);
                                var customer = new XElement("Customer");
                                lineItems = new XElement("OrderLineItems");
                                order.Add(customer, lineItems);

                                var orderDate = (DateTime)reader["OrderDate"];

                                var totalDue = (Decimal)reader["TotalDue"];
                                var reviewRequired = totalDue > 5000
                                    ? 'Y'
                                    : 'N';

                                order.Add(
                                    new XAttribute("OrderNumber", orderNumber),
                                    new XAttribute("Status", reader["Status"]),
                                    new XAttribute("ShipDate", reader["ShipDate"]),
                                    new XAttribute("OrderDateYear", orderDate.Year),
                                    new XAttribute("OrderDateMonth", orderDate.Month),
                                    new XAttribute("DueDate", reader["DueDate"]),
                                    new XAttribute("SubTotal", RoundAndFormat(reader["SubTotal"])),
                                    new XAttribute("TaxAmt", RoundAndFormat(reader["TaxAmt"])),
                                    new XAttribute("TotalDue", RoundAndFormat(totalDue)),
                                    new XAttribute("ReviewRequired", reviewRequired));

                                var fullName = string.Join(" ",
                                    reader["CustomerTitle"],
                                    reader["CustomerFirstName"],
                                    reader["CustomerMiddleName"],
                                    reader["CustomerLastName"],
                                    reader["CustomerSuffix"]
                                    )
                                    .Replace("  ", " ") //remove double spaces
                                    .Trim()
                                    .ToUpper();

                                customer.Add(
                                    new XAttribute("AccountNumber", reader["AccountNumber"]),
                                    new XAttribute("FullName", fullName));
                            }

                            var productId = (int)reader["ProductID"];
                            var quantity = (short)reader["Quantity"];

                            var inventoryCheckRequired = (productId > 710 && productId < 720 && quantity > 5)
                                ? 'Y'
                                : 'N';

                            lineItems.Add(
                                new XElement("LineItem",
                                    new XAttribute("Quantity", quantity),
                                    new XAttribute("UnitPrice", ((Decimal)reader["UnitPrice"]).ToString("C")),
                                    new XAttribute("LineTotal", RoundAndFormat(reader["LineTotal"])),
                                    new XAttribute("ProductID", productId),
                                    new XAttribute("InventoryCheckRequired", inventoryCheckRequired)
                                    ));
                        }

                        return ResponseMessage(HttpResponseHelper.CreateMessageFrom(doc.ToString()));
                    }
                }
            }
        }

    }
}