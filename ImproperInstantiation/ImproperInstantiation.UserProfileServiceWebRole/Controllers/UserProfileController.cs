// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Web.Http;
using UserProfileServiceWebRole.Models;

namespace UserProfileServiceWebRole.Controllers
{
    public class UserProfileController : ApiController
    {
        public async Task<UserProfile> Get()
        {
            //Simulate processing
            await Task.Delay(100);

            return new UserProfile() { FirstName = "Alton", LastName = "Hudgens" };
        }
    }
}
