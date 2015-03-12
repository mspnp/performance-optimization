using System.Collections.Generic;

namespace ChattyIO.DataAccess
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
