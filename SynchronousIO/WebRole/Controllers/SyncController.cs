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

        /// <summary>
        /// This is a synchronous method that calls the synchronous GetUserProfile method.
        /// </summary>
        /// <returns>A UserProfile instance</returns>
        public UserProfile GetUserProfile()
        {
            var userProfile = _userProfileService.GetUserProfile();
            return userProfile;
        }
    }
}
