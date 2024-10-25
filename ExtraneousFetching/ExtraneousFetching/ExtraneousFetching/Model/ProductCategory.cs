using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExtraneousFetching.Model
{
    public class ProductCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductCategoryID { get; set; }

        public int? ParentProductCategoryID { get; set; }

        [Required]
        public string Name { get; set; }

        public Guid Rowguid { get; set; }

        public DateTime ModifiedDate { get; set; }

        // Navigation property for related Products
        public ICollection<Product> Products { get; set; }
    }
}
