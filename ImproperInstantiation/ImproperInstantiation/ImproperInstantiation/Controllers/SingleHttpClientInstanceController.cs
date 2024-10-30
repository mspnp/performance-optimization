using ImproperInstantiation.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImproperInstantiation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SingleHttpClientInstanceController(IHttpClientFactory httpClientFactory) : ControllerBase
    {

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductAsync(string id)
        {
            var httpClient = httpClientFactory.CreateClient("api-usuario");

            var result = await httpClient.GetStringAsync("/UserProfile");

            return Ok(new Product { Name = result });
        }
    }
}
