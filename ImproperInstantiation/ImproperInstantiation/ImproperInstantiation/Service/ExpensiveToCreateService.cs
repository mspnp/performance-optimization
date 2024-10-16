using ImproperInstantiation.Models;

namespace ImproperInstantiation.Service
{
    public class ExpensiveToCreateService : IExpensiveToCreateService
    {
        public ExpensiveToCreateService()
        {
            //Simulate delay due to setup and configuration of ExpensiveToCreateService
            Thread.SpinWait(Int32.MaxValue / 100);
        }

        public Task<Product> GetProductByIdAsync(string productId)
        {
            var product = new Product { Name = "test" };
            return Task.FromResult(product);
        }
    }
}
