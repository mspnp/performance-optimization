using ImproperInstantiation.Service;
using Microsoft.AspNetCore.Mvc;

namespace ImproperInstantiation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SingleServiceInstanceController(IExpensiveToCreateService expensiveToCreateService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductAsync(string id)
        {
            return Ok(await expensiveToCreateService.GetProductByIdAsync(id));
        }
    }
}
