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
        /// This is an asynchronous method that calls the Task based GetUserProfileAsync method.
        /// </summary>
        public Task<UserProfile> GetUserProfileAsync()
        {
            return _userProfileService.GetUserProfileAsync();
        }
    }
}
