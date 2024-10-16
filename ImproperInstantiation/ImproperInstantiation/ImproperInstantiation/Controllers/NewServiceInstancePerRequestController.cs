using ImproperInstantiation.Service;
using Microsoft.AspNetCore.Mvc;

namespace ImproperInstantiation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewServiceInstancePerRequestController() : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductAsync(string id)
        {
            var expensiveToCreateService = new ExpensiveToCreateService();
            return Ok(await expensiveToCreateService.GetProductByIdAsync(id));
        }
    }
}
