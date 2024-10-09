using Microsoft.AspNetCore.Mvc;

namespace BusyDatabase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LessProcSqlController(ILogger<LessProcSqlController> _logger) : ControllerBase
    {

        [HttpGet("{id:int}")]
        [Produces("application/xml")]
        public async Task<IActionResult> Get(int id)
        {
            // Example data to return
            var exampleData = new
            {
                Id = id,
                Name = "Sample Name",
                Description = "Sample Description"
            };

            return Ok(exampleData);
        }
    }
}
