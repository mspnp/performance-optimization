using System.Threading.Tasks;
using WebRole.Models;

namespace WebRole
{
    public interface IProductRepository
    {
        Task<Product> GetProductByIdAsync(string productId);
    }
}