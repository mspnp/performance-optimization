using BusyDatabase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Xml.Serialization;

namespace BusyDatabase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LessProcSqlController(ILogger<LessProcSqlController> _logger, IQueryService _queryService, IConfiguration _configuration) : ControllerBase
    {
        private static string RoundAndFormat(object value)
        {
            var currency = Math.Round((decimal)value, 2);
            return currency.ToString("C");
        }

        [HttpGet("{id}")]
        [Produces("application/xml")]
        public async Task<IActionResult> Get(int id)
        {
            var query = await _queryService.GetAsync("LessProcSql.sql");

            var connectionString = _configuration["connectionString"];
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", id);

                    var orders = new List<Order>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var lastOrderNumber = string.Empty;
                        Order order = new Order();
                        while (await reader.ReadAsync())
                        {
                            var orderNumber = reader["OrderNumber"].ToString();
                            if (orderNumber != lastOrderNumber)
                            {
                                lastOrderNumber = orderNumber;
                                var orderDate = reader["OrderDate"].ToString();
                                var orderDateTime = DateTime.Parse(orderDate);
                                var totalDue = Double.Parse(reader["TotalDue"].ToString());
                                order = new Order
                                {
                                    OrderNumber = orderNumber,
                                    Status = reader["Status"].ToString(),
                                    ShipDate = reader["ShipDate"].ToString(),
                                    OrderDateYear = orderDateTime.Year,
                                    OrderDateMonth = orderDateTime.Month,
                                    DueDate = reader["DueDate"].ToString(),
                                    SubTotal = reader["SubTotal"].ToString(),
                                    TaxAmt = reader["TaxAmt"].ToString(),
                                    TotalDue = reader["TotalDue"].ToString(),
                                    ReviewRequired = (totalDue > 5000) ? "Y" : "N",
                                    OrderLineItems = new List<LineItem>()
                                };
                                order.Customer = new Customer
                                {
                                    CompanyName = reader["CompanyName"].ToString(),
                                    FullName = $"{reader["City"]} {reader["CountryRegion"]} {reader["PostalCode"]} {reader["StateProvince"]}".ToUpper(),
                                };
                                orders.Add(order);
                            }
                            var productId = (int)reader["ProductID"];
                            var quantity = (short)reader["Quantity"];

                            var inventoryCheckRequired = (productId >= 710 && productId <= 720 && quantity >= 5) ? 'Y' : 'N';

                            var lineItem = new LineItem
                            {
                                Quantity = quantity,
                                LineTotal = RoundAndFormat(reader["LineTotal"]),
                                ProductId = productId,
                                UnitPrice = ((Decimal)reader["UnitPrice"]).ToString("C"),
                                InventoryCheckRequired = inventoryCheckRequired
                            };
                            order.OrderLineItems.Add(lineItem);

                        }
                    }
                    var orderList = new OrderList { Orders = orders };
                    return Ok(orderList);
                }
            }
        }
    }

    public class Order
    {
        [XmlAttribute("OrderNumber")]
        public string OrderNumber { get; set; }

        [XmlAttribute("Status")]
        public string Status { get; set; }

        [XmlAttribute("ShipDate")]
        public string ShipDate { get; set; }

        [XmlAttribute("OrderDateYear")]
        public int OrderDateYear { get; set; }

        [XmlAttribute("OrderDateMonth")]
        public int OrderDateMonth { get; set; }

        [XmlAttribute("DueDate")]
        public string DueDate { get; set; }

        [XmlAttribute("SubTotal")]
        public string SubTotal { get; set; }

        [XmlAttribute("TaxAmt")]
        public string TaxAmt { get; set; }

        [XmlAttribute("TotalDue")]
        public string TotalDue { get; set; }

        [XmlAttribute("ReviewRequired")]
        public string ReviewRequired { get; set; }

        public Customer Customer { get; set; }

        public List<LineItem> OrderLineItems { get; set; }

    }

    [XmlRoot("orders")]
    public class OrderList
    {
        [XmlElement("Order")]
        public List<Order> Orders { get; set; }
    }

    public class Customer
    {
        [XmlAttribute("CompanyName")]
        public string CompanyName { get; set; }

        [XmlAttribute("FullName")]
        public string FullName { get; set; }
    }

    public class LineItem
    {
        [XmlAttribute("Quantity")]
        public short Quantity { get; set; }

        [XmlAttribute("UnitPrice")]
        public string UnitPrice { get; set; }

        [XmlAttribute("LineTotal")]
        public string LineTotal { get; set; }

        [XmlAttribute("ProductId")]
        public int ProductId { get; set; }

        [XmlAttribute("InventoryCheckRequired")]
        public char InventoryCheckRequired { get; set; }
    }
}
