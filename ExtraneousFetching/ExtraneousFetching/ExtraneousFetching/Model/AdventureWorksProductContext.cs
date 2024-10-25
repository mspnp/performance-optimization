using Microsoft.EntityFrameworkCore;

namespace ExtraneousFetching.Model
{
    public class AdventureWorksProductContext : DbContext
    {
        public AdventureWorksProductContext()
        {
        }

        public AdventureWorksProductContext(DbContextOptions<AdventureWorksProductContext> options)
            : base(options)
        {
        }

        // DbSet properties for each table in your database
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
        public DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; }

        // Configure model relationships using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductCategory>()
            .ToTable("ProductCategory", "SalesLT");

            modelBuilder.Entity<SalesOrderHeader>()
           .ToTable("SalesOrderHeader", "SalesLT");

            // Configuring SalesOrderDetail primary key (composite key)
            modelBuilder.Entity<SalesOrderDetail>()
                .ToTable("SalesOrderDetail", "SalesLT")
                .HasKey(sod => new { sod.SalesOrderID, sod.SalesOrderDetailID });

            // Configuring relationships between Product and ProductCategory
            modelBuilder.Entity<Product>()
                .ToTable("Product", "SalesLT")
                .HasOne(p => p.ProductCategory)
                .WithMany(pc => pc.Products)
                .HasForeignKey(p => p.ProductCategoryID);

            // Configuring relationships between Product and SalesOrderDetail
            modelBuilder.Entity<SalesOrderDetail>()
                .ToTable("SalesOrderDetail", "SalesLT")
                .HasOne(sod => sod.Product)
                .WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(sod => sod.ProductID);

            base.OnModelCreating(modelBuilder);
        }

    }
}
