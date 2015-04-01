// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace CachingDemo.Data.Models
{
    public class Employee : Person
    {
        public string JobTitle { get; set; }

        public DateTime HireDate { get; set; }
    }
}
