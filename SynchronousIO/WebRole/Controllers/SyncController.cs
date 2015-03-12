using System.Web.Http;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class SyncController : ApiController
    {
        private readonly IUserProfileService _userProfileService;

        public SyncController()
        {
            _userProfileService = new FakeUserProfileService();
        }

        /// <summary>
        /// This is a synchronous method that calls the synchronous GetUserProfile method.
        /// </summary>
        public UserProfile GetUserProfile()
        {
            return _userProfileService.GetUserProfile();
        }
    }
}
