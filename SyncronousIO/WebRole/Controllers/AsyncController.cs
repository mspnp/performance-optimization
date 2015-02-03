using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class AsyncController : ApiController
    {
        private readonly IUserProfileService _userProfileService;

        public AsyncController()
        {
            _userProfileService = new UserProfileServiceProxy();
        }

        public async Task<UserProfile> GetUserProfileAsync()
        {
            return await _userProfileService.GetUserProfileAsync();
        }
    }
}
