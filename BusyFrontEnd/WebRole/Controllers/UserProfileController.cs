using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class UserProfileController : ApiController
    {
        [HttpGet]
        [Route("api/userprofile/{id}")]
        public UserProfile Get(int id)
        {
            //Simulate processing
            return new UserProfile() {FirstName = "Alton", LastName = "Hudgens"};
        }
    }
}
