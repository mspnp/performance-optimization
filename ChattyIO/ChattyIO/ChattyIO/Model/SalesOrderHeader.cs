using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ChattyIO.Model
{
    public class SalesOrderHeader
    {
        [Key]
        public int SalesOrderID { get; set; }

        public byte RevisionNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ShipDate { get; set; }

        public byte Status { get; set; }

        public bool OnlineOrderFlag { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string SalesOrderNumber { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public string AccountNumber { get; set; }

        public int CustomerID { get; set; }

        public int? ShipToAddressID { get; set; }

        public int? BillToAddressID { get; set; }

        [MaxLength(50)]
        public string ShipMethod { get; set; }

        [MaxLength(15)]
        public string CreditCardApprovalCode { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TaxAmt { get; set; }

        public decimal Freight { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalDue { get; set; }

        public string Comment { get; set; }

        public Guid Rowguid { get; set; }

        public DateTime ModifiedDate { get; set; }
    }

}
