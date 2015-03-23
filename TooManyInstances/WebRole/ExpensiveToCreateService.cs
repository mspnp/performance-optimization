// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using WebRole.Models;

namespace WebRole
{
    /// <summary>
    /// This ExpensiveToCreateService represents a class that is meant to be long lived. 
    /// The same instance is meant to be share by the classes that depend on it.
    /// </summary>
    public class ExpensiveToCreateService
    {
        public ExpensiveToCreateService()
        {
            //Simulate delay due to setup and configuration of ExpensiveToCreateService
            Thread.SpinWait(Int32.MaxValue / 100);
        }

        public Task<Product> GetProductByIdAsync(string productId)
        {
            var product = new Product {Name = "test"};
            return Task.FromResult(product);
        }
    }
}
