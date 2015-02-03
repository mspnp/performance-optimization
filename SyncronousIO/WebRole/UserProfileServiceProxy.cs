using System.Threading;
using System.Threading.Tasks;
using WebRole.Models;

namespace WebRole
{
    public class UserProfileServiceProxy : IUserProfileService
    {
        public UserProfile GetUserProfile()
        {
            Thread.Sleep(2000);
            return new UserProfile() { FirstName = "Alton", LastName = "Hudgens" };
        }

        public async Task<UserProfile> GetUserProfileAsync()
        {
            await Task.Delay(2000).ConfigureAwait(false);
            return new UserProfile() { FirstName = "Alton", LastName = "Hudgens" };
        }

        public Task<UserProfile> GetUserProfileWrappedAsync()
        {
            return Task.Run(()=> GetUserProfile());
        }

    }
}
