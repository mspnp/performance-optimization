using System.Collections;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class NewInstancePerRequestController : ApiController
    {
        public Product GetProduct(string id)
        {
            using (var productRepository = new ProductRepository())
            {
                return productRepository.GetProductById(id);
            }
        }
    }
}
