using System.Collections.Generic;

namespace ChattyIO.DataAccess
{
    public class ProductSubcategory
    {
        public ProductSubcategory()
        {
            Product = new List<Product>();
        }

        public int ProductSubcategoryId { get; set; }
        // ProductSubcategoryID (Primary key). Primary key for ProductSubcategory records.
        public int ProductCategoryId { get; set; }
        // ProductCategoryID. Product category identification number. Foreign key to ProductCategory.ProductCategoryID.
        public string Name { get; set; } // Name. Subcategory description.
        // Reverse navigation
        public virtual ICollection<Product> Product { get; set; }
    }
}