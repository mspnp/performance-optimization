// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using SynchronousIO.WebRole.Models;

namespace SynchronousIO.WebRole.Controllers
{
    public class WrappedSyncController : ApiController
    {
        private readonly IUserProfileService _userProfileService;

        public WrappedSyncController()
        {
            _userProfileService = new FakeUserProfileService();
        }

        /// <summary>
        /// This is an asynchronous method that calls the Task based GetUserProfileWrappedAsync method.
        /// Even though this method is async, the result is similar to the SyncController in that threads
        /// are tied up by the synchronous GetUserProfile method in the Task.Run. Under significant load
        /// new threads will need to be created.
        /// </summary>
        public async Task<UserProfile> GetUserProfileAsync()
        {
            return await _userProfileService.GetUserProfileWrappedAsync();
        }
    }
}
