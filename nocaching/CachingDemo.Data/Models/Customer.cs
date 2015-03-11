namespace CachingDemo.Data.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string AccountNumber { get; set; }

        public virtual Person Person { get; set; }
    }
}
