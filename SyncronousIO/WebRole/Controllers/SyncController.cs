using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class SyncController : ApiController
    {
        private readonly IUserProfileService _userProfileService;

        public SyncController()
        {
            _userProfileService = new UserProfileServiceProxy();
        }

        public UserProfile GetUserProfileAsync()
        {
            return _userProfileService.GetUserProfile();
        }
    }
}
