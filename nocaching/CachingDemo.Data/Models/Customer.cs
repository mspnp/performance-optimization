using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data.Models
{
    public class Customer
    {
        public Customer()
        {
        }

        public int CustomerId { get; set; }

        public string AccountNumber { get; set; }

        public virtual Person Person { get; set; }
    }
}
