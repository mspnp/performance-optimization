using ImproperInstantiation.UserProfileService.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImproperInstantiation.UserProfileService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserProfileController : ControllerBase
    {
        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            //Simulate processing
            await Task.Delay(100);

            return Ok(new UserProfile() { FirstName = "Alton", LastName = "Hudgens" });
        }
    }
}
