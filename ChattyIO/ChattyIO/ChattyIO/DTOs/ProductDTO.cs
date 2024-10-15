namespace ChattyIO.DTOs
{
    public class ProductDTO
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public ICollection<SalesOrderDetailDTO> SalesOrderDetails { get; set; }
    }
}
