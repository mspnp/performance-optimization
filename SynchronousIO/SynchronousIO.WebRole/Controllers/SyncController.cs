// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Http;
using SynchronousIO.WebRole.Models;

namespace SynchronousIO.WebRole.Controllers
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
