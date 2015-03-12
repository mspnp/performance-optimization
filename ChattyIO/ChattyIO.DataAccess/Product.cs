using System.Collections.Generic;

namespace ChattyIO.DataAccess
{
    public class Product
    {
        public Product()
        {
            ProductListPriceHistory = new List<ProductListPriceHistory>();
        }

        // ProductID (Primary key). Primary key for Product records.
        public int ProductId { get; set; }

        // Name. Name of the product.
        public string Name { get; set; }

        // ProductNumber. Unique product identification number.
        public string ProductNumber { get; set; }

        // ListPrice. Selling price.
        public decimal ListPrice { get; set; }

        // ProductSubcategoryID. Product is a member of this product subcategory. Foreign key to ProductSubCategory.ProductSubCategoryID.
        public int? ProductSubcategoryId { get; set; }

        public virtual ICollection<ProductListPriceHistory> ProductListPriceHistory { get; set; }
    }
}