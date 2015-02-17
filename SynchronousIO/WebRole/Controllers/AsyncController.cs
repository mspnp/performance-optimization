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

        /// <summary>
        /// This is an synchronous method that calls the Task based GetUserProfileAsync method.
        /// </summary>
        /// <returns>A UserProfile instance</returns>
        public Task<UserProfile> GetUserProfileAsync()
        {
            var userProfile = _userProfileService.GetUserProfileAsync();
            return userProfile;
        }
    }
}
