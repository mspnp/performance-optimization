// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using SynchronousIO.WebRole.Models;

namespace SynchronousIO.WebRole.Controllers
{
    public class AsyncController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public AsyncController()
        {
            _userProfileService = new FakeUserProfileService();
        }

        /// <summary>
        /// This is an asynchronous method that calls the Task based GetUserProfileAsync method.
        /// </summary>
        public async Task<UserProfile> GetUserProfileAsync()
        {
            return await _userProfileService.GetUserProfileAsync();
        }
    }
}
