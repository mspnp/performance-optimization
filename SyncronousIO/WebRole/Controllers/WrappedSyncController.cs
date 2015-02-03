using System.Threading.Tasks;
using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class WrappedSyncController : ApiController
    {
        private readonly IUserProfileService _userProfileService;

        public WrappedSyncController()
        {
            _userProfileService = new UserProfileServiceProxy();
        }

        public async Task<UserProfile> GetUserProfileAsync()
        {
            return await _userProfileService.GetUserProfileWrappedAsync();
        }
    }
}
