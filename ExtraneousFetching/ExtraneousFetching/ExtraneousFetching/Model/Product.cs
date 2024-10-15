using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExtraneousFetching.Model
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? ProductID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [MaxLength(25)]
        public string ProductNumber { get; set; }

        [MaxLength(15)]
        public string? Color { get; set; }

        public decimal StandardCost { get; set; }

        public decimal ListPrice { get; set; }

        [MaxLength(5)]
        public string? Size { get; set; }

        public decimal? Weight { get; set; }

        public int? ProductCategoryID { get; set; }

        public int? ProductModelID { get; set; }

        public DateTime SellStartDate { get; set; }

        public DateTime? SellEndDate { get; set; }

        public DateTime? DiscontinuedDate { get; set; }

        public byte[] ThumbNailPhoto { get; set; }

        [MaxLength(50)]
        public string ThumbnailPhotoFileName { get; set; }

        public Guid Rowguid { get; set; }

        public DateTime ModifiedDate { get; set; }

        // Navigation property for related SalesOrderDetails
        public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; }

        // Navigation property to ProductCategory
        public ProductCategory ProductCategory { get; set; }
    }
}
