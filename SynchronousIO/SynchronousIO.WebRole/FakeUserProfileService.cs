// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using WebRole.Models;

namespace WebRole
{
    public class FakeUserProfileService : IUserProfileService
    {
        /// <summary>
        /// This method simulates a synchronous IO call that blocks the current thread
        /// while processing the request for a user profile instance.
        /// </summary>
        public UserProfile GetUserProfile()
        {
            Thread.Sleep(2000);
            return new UserProfile { FirstName = "Alton", LastName = "Hudgens" };
        }

        /// <summary>
        /// This method simulates a Task based asynchronous IO call that does not block the current thread
        /// while processing the request for a user profile instance. The await keyword allows the current thread
        /// to be returned to the thread pool while the IO request waits to be completed. The ConfigureAwait(false)
        /// allows a different thread to resume the execution of the method after the IO request completes.
        /// </summary>
        public async Task<UserProfile> GetUserProfileAsync()
        {
            await Task.Delay(2000).ConfigureAwait(false);
            return new UserProfile { FirstName = "Alton", LastName = "Hudgens" };
        }

        /// <summary>
        /// This method shows how to create a Tasked based asynchronous method that wraps a synchronous method.
        /// This should be avoided except when current thread resources are highly valuable and it is desirable 
        /// to offload processing to a different thread, (e.g. the UI thread of a client application).
        /// This method will have similar impact on scalability as the synchronous GetUserProfile method.
        /// </summary>
        public Task<UserProfile> GetUserProfileWrappedAsync()
        {
            return Task.Run(() => GetUserProfile());
        }
    }
}
