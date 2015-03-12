using System.Data.Entity;

namespace ChattyIO.DataAccess
{
    public class AdventureWorksProductContext : DbContext
    {
        static AdventureWorksProductContext()
        {
            Database.SetInitializer<AdventureWorksProductContext>(null);
        }

        public AdventureWorksProductContext()
            : base("Name=AdventureWorksProductContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new ProductConfiguration());
            modelBuilder.Configurations.Add(new ProductCategoryConfiguration());
            modelBuilder.Configurations.Add(new ProductListPriceHistoryConfiguration());
            modelBuilder.Configurations.Add(new ProductSubcategoryConfiguration());
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductListPriceHistory> ProductListPriceHistory { get; set; }
        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }
    }
}
