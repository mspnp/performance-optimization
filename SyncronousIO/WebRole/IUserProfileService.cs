using System.Threading.Tasks;
using WebRole.Models;

namespace WebRole
{
    public interface IUserProfileService
    {
        UserProfile GetUserProfile();
        Task<UserProfile> GetUserProfileAsync();
        Task<UserProfile> GetUserProfileWrappedAsync();
    }
}