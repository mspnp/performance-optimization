using ImproperInstantiation.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImproperInstantiation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewHttpClientInstancePerRequestController(IConfiguration configuration) : ControllerBase
    {

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductAsync(string id)
        {
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.GetStringAsync($"{configuration["api-usuario"]}/UserProfile");

                return Ok(new Product { Name = result });
            }
        }
    }
}
