// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ChattyIO.DataAccess.Models
{
    public class ProductCategory
    {
        public ProductCategory()
        {
            ProductSubcategory = new List<ProductSubcategory>();
        }

        // ProductCategoryID (Primary key). Primary key for ProductCategory records.
        public int ProductCategoryId { get; set; }

        // Name. Category description.
        public string Name { get; set; }

        // Reverse navigation
        public virtual ICollection<ProductSubcategory> ProductSubcategory { get; set; }
    }
}
