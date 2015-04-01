// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NoCaching.Data.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string AccountNumber { get; set; }

        public virtual Person Person { get; set; }
    }
}
