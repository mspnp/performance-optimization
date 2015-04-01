// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Http;
using BusyFrontEnd.WebRole.Models;

namespace BusyFrontEnd.WebRole.Controllers
{
    public class UserProfileController : ApiController
    {
        [HttpGet]
        [Route("api/userprofile/{id}")]
        public UserProfile Get(int id)
        {
            //Simulate processing
            return new UserProfile() {FirstName = "Alton", LastName = "Hudgens"};
        }
    }
}
