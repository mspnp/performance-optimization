// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using CachingDemo.Data.Models;

namespace CachingDemo.Data
{
    public interface ISalesOrderRepository
    {
        Task<ICollection<SalesOrderHeader>> GetTopTenSalesOrdersAsync();
    }
}
