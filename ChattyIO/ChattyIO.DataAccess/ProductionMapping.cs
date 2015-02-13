namespace ChattyIO.DataAccess
{

    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration;
 
    //Product
    internal class ProductConfiguration : EntityTypeConfiguration<Product>
    {
        public ProductConfiguration(string schema = "Production")
        {
            ToTable(schema + ".Product");
            HasKey(x => x.ProductId);

            Property(x => x.ProductId)
                .HasColumnName("ProductID")
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Name).HasColumnName("Name").IsRequired().HasMaxLength(50);
        }
    }

    // ProductCategory
    internal class ProductCategoryConfiguration : EntityTypeConfiguration<ProductCategory>
    {
        public ProductCategoryConfiguration(string schema = "Production")
        {
            ToTable(schema + ".ProductCategory");
            HasKey(x => x.ProductCategoryId);

            Property(x => x.ProductCategoryId)
                .HasColumnName("ProductCategoryID")
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Name).HasColumnName("Name").IsRequired().HasMaxLength(50);

        }
    }

    // ProductListPriceHistory
    internal class ProductListPriceHistoryConfiguration :
        EntityTypeConfiguration<ProductListPriceHistory>
    {
        public ProductListPriceHistoryConfiguration(string schema = "Production")
        {
            ToTable(schema + ".ProductListPriceHistory");
            HasKey(x => new { x.ProductId, x.StartDate });

            Property(x => x.ProductId)
                .HasColumnName("ProductID")
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(x => x.StartDate).HasColumnName("StartDate").IsRequired();
            Property(x => x.EndDate).HasColumnName("EndDate").IsOptional();
            Property(x => x.ListPrice).HasColumnName("ListPrice").IsRequired().HasPrecision(19, 4);
        }
    }

    // ProductSubcategory
    internal class ProductSubcategoryConfiguration : EntityTypeConfiguration<ProductSubcategory>
    {
        public ProductSubcategoryConfiguration(string schema = "Production")
        {
            ToTable(schema + ".ProductSubcategory");
            HasKey(x => x.ProductSubcategoryId);

            Property(x => x.ProductSubcategoryId).HasColumnName("ProductSubcategoryID").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ProductCategoryId).HasColumnName("ProductCategoryID").IsRequired();
            Property(x => x.Name).HasColumnName("Name").IsRequired().HasMaxLength(50);
        }
    }
}
