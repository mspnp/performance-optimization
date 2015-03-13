using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class NewInstancePerRequestController : ApiController
    {
        /// <summary>
        /// This method creates a new instance of ProductRepository and disposes it for every call to GetProductAsync.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Product> GetProductAsync(string id)
        {
            using (var productRepository = new ProductRepository())
            {
                return await productRepository.GetProductByIdAsync(id).ConfigureAwait(false);
            }
        }
    }
}
