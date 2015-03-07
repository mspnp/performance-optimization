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

        /// <summary>
        /// This is an asynchronous method that calls the Task based GetUserProfileWrappedAsync method.
        /// Even though this method is async, the result is similar to the SyncController in that threads
        /// are tied up by the synchronous GetUserProfile method in the Task.Run. Under significant load
        /// new threads will need to be created.
        /// </summary>
        /// <returns>A UserProfile instance</returns>
        public Task<UserProfile> GetUserProfileAsync()
        {
            return _userProfileService.GetUserProfileWrappedAsync();
        }
    }
}
