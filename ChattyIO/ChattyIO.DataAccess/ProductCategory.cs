namespace ChattyIO.DataAccess
{

    using System.Collections.Generic;
  
    public class ProductCategory
    {
        public ProductCategory()
        {
            ProductSubcategory = new List<ProductSubcategory>();
        }

        public int ProductCategoryId { get; set; }
        // ProductCategoryID (Primary key). Primary key for ProductCategory records.
        public string Name { get; set; } // Name. Category description.
        // Reverse navigation
        public virtual ICollection<ProductSubcategory> ProductSubcategory { get; set; }
    }
}
