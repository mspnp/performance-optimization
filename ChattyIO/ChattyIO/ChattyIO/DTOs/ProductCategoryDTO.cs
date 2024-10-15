namespace ChattyIO.DTOs
{
    public class ProductCategoryDTO
    {
        public int ProductCategoryID { get; set; }
        public int? ParentProductCategoryID { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }
        public ICollection<ProductDTO> Products { get; set; }
    }
}
