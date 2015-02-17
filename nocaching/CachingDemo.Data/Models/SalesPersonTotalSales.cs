using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data.Models
{
    public class SalesPersonTotalSales
    {
        public SalesPersonTotalSales()
            : base()
        {
        }

        public SalesPerson SalesPerson { get; set; }

        public decimal TotalSales { get; set; }
    }
}
