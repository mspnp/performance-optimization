using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
public class UserProfileController : ApiController
{
    public UserProfile Get()
    {
        return new UserProfile() { FirstName = "Alton", LastName = "Hudgens" };
    }
}
}