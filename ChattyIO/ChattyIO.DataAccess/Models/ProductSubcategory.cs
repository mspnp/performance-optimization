// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ChattyIO.DataAccess.Models
{
    public class ProductSubcategory
    {
        public ProductSubcategory()
        {
            Product = new List<Product>();
        }

        // ProductSubcategoryID (Primary key). Primary key for ProductSubcategory records.
        public int ProductSubcategoryId { get; set; }

        // ProductCategoryID. Product category identification number. Foreign key to ProductCategory.ProductCategoryID.
        public int ProductCategoryId { get; set; }

        // Name. Subcategory description.
        public string Name { get; set; }

        // Reverse navigation
        public virtual ICollection<Product> Product { get; set; }
    }
}