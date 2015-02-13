namespace ChattyIO.DataAccess
{

    using System.Collections.Generic;
 
    public class Product
    {
        public Product()
        {
            ProductListPriceHistory = new List<ProductListPriceHistory>();
        }

        public int ProductId { get; set; } // ProductID (Primary key). Primary key for Product records.
        public string Name { get; set; } // Name. Name of the product.
        public string ProductNumber { get; set; } // ProductNumber. Unique product identification number.
        public decimal ListPrice { get; set; } // ListPrice. Selling price.
        public int? ProductSubcategoryId { get; set; }
        // ProductSubcategoryID. Product is a member of this product subcategory. Foreign key to ProductSubCategory.ProductSubCategoryID.
        public virtual ICollection<ProductListPriceHistory> ProductListPriceHistory { get; set; }
    }
}