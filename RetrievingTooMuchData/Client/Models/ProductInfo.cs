using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class ProductInfo
    {
        public string ProductName { get; set; }
        public string SubCategory { get; set; }
        public string Color { get; set; }
        public decimal ListPrice { get; set; }
        public string Size { get; set; }
    }
}
