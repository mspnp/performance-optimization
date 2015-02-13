namespace ChattyIO.DataAccess
{

    using System.Data.Entity;
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

        public AdventureWorksProductContext(string connectionString)
            : base(connectionString)
        {
        }

        public AdventureWorksProductContext(string connectionString,
            System.Data.Entity.Infrastructure.DbCompiledModel model)
            : base(connectionString, model)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new ProductConfiguration());
            modelBuilder.Configurations.Add(new ProductCategoryConfiguration());
            modelBuilder.Configurations.Add(
                new ProductListPriceHistoryConfiguration());
            modelBuilder.Configurations.Add(
                new ProductSubcategoryConfiguration());
        }

        public static DbModelBuilder CreateModel(DbModelBuilder modelBuilder, string schema)
        {
            modelBuilder.Configurations.Add(new ProductConfiguration(schema));
            modelBuilder.Configurations.Add(new ProductCategoryConfiguration(schema));
            modelBuilder.Configurations.Add(
                new ProductListPriceHistoryConfiguration());
            modelBuilder.Configurations.Add(
                new ProductSubcategoryConfiguration(schema));
            return modelBuilder;
        }

        public DbSet<Product> Products { get; set; } // Product
        public DbSet<ProductCategory> ProductCategories { get; set; } // ProductCategory
        public DbSet<ProductListPriceHistory> ProductListPriceHistory { get; set; }
        public DbSet<ProductSubcategory> ProductSubcategories { get; set; } // ProductSubcategory
    }
}
