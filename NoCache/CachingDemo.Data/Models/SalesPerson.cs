// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CachingDemo.Data.Models
{
    public class SalesPerson : Employee
    {
        public decimal? SalesQuota { get; set; }

        public decimal Bonus { get; set; }

        public decimal CommissionPercentage { get; set; }
    }
}
