using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data.Models
{
    public class SalesOrderHeader
    {
        public SalesOrderHeader()
        {
        }

        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public string SalesOrderNumber { get; set; }

        public decimal TotalDue { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual SalesPerson SalesPerson { get; set; }

        public int CustomerId { get; set; }
    }
}
