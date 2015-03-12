using System;

namespace ChattyIO.DataAccess
{
    public class ProductListPriceHistory
    {
        public int ProductId { get; set; }
        // ProductID (Primary key). Product identification number. Foreign key to Product.ProductID
        public DateTime StartDate { get; set; } // StartDate (Primary key). List price start date.
        public DateTime? EndDate { get; set; } // EndDate. List price end date
        public decimal ListPrice { get; set; } // ListPrice. Product list price.
    }
}
