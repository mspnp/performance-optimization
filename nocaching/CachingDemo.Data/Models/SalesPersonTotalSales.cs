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
