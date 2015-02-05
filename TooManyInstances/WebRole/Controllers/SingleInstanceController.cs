using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class SingleInstanceController : ApiController
    {
        private static readonly IProductRepository ProductRepository;

        static SingleInstanceController()
        {
            ProductRepository = new ProductRepository();
        }

        public Product GetProduct(string id)
        {
            return ProductRepository.GetProductById(id);
        }
    }
}
