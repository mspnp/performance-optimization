using System;

namespace CachingDemo.Data.Models
{
    public class SalesOrderHeader
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public string SalesOrderNumber { get; set; }

        public decimal TotalDue { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual SalesPerson SalesPerson { get; set; }

        public int CustomerId { get; set; }
    }
}
