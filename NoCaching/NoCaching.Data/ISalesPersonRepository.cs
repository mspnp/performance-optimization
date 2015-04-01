// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using NoCaching.Data.Models;

namespace NoCaching.Data
{
    public interface ISalesPersonRepository
    {
        Task<SalesPerson> GetAsync(int id);

        Task<ICollection<SalesPersonTotalSales>> GetTopTenSalesPeopleAsync();
    }
}
