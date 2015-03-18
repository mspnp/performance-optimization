// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using WebRole.Models;

namespace WebRole
{
    public interface IProductRepository
    {
        Task<Product> GetProductByIdAsync(string productId);
    }
}