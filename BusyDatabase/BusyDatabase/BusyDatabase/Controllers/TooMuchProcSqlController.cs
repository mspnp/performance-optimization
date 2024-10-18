using BusyDatabase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BusyDatabase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TooMuchProcSqlController(ILogger<TooMuchProcSqlController> _logger, IQueryService _queryService, IConfiguration _configuration) : ControllerBase
    {

        [HttpGet("{id:int}")]
        [Produces("application/xml")]
        public async Task<IActionResult> Get(int id)
        {
            var query = await _queryService.GetAsync("TooMuchProcSql.sql");

            var connectionString = _configuration["connectionString"];
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var xmlResult = reader.GetString(0);
                            return Content(xmlResult, "application/xml");
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
        }
    }
}
