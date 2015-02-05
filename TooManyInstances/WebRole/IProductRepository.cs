using WebRole.Models;

namespace WebRole
{
    public interface IProductRepository
    {
        Product GetProductById(string productId);
    }
}