using ImproperInstantiation.Models;

namespace ImproperInstantiation.Service
{
    public interface IExpensiveToCreateService
    {
        Task<Product> GetProductByIdAsync(string productId);
    }
}