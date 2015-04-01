// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using SynchronousIO.WebRole.Models;

namespace SynchronousIO.WebRole
{
    public interface IUserProfileService
    {
        UserProfile GetUserProfile();
        Task<UserProfile> GetUserProfileAsync();
        Task<UserProfile> GetUserProfileWrappedAsync();
    }
}