using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingDemo.Data.Models
{
    public class Employee : Person
    {
        public Employee()
            : base()
        {
        }

        public string JobTitle { get; set; }

        public DateTime HireDate { get; set; }
    }
}
