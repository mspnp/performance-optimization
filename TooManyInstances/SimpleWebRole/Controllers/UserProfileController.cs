// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Web.Http;

namespace SimpleWebRole.Controllers
{
    public class UserProfileController : ApiController
    {
        public UserProfile Get()
        {
            //Simulate processing
            Thread.Sleep(100);

            return new UserProfile() { FirstName = "Alton", LastName = "Hudgens" };
        }
    }

    public class UserProfile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
