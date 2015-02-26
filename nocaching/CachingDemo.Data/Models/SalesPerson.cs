using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data.Models
{
    public class SalesPerson : Employee
    {
        public SalesPerson()
        {
        }

        public decimal? SalesQuota { get; set; }

        public decimal Bonus { get; set; }

        public decimal CommissionPercentage { get; set; }
    }
}
