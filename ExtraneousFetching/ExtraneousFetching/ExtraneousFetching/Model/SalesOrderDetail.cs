using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExtraneousFetching.Model
{
    public class SalesOrderDetail
    {
        [Key, Column(Order = 0)]
        public int SalesOrderID { get; set; }

        [Key, Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SalesOrderDetailID { get; set; }

        public short OrderQty { get; set; }

        public int ProductID { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal UnitPriceDiscount { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal LineTotal { get; set; }

        public Guid Rowguid { get; set; }

        public DateTime ModifiedDate { get; set; }

        // Navigation property to Product
        public Product Product { get; set; }
    }
}
