using System;

namespace ChattyIO.DataAccess.Models
{
    public class ProductListPriceHistory
    {
        // ProductID (Primary key). Product identification number. Foreign key to Product.ProductID
        public int ProductId { get; set; }

        // StartDate (Primary key). List price start date.
        public DateTime StartDate { get; set; }

        // EndDate. List price end date
        public DateTime? EndDate { get; set; }

        // ListPrice. Product list price.
        public decimal ListPrice { get; set; }
    }
}
