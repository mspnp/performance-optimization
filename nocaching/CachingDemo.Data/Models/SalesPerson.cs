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
