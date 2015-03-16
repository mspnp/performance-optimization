using RetrievingTooMuchData.DataAccess.Mapping;
using System.Data.Entity;

namespace RetrievingTooMuchData.DataAccess
{
    public class AdventureWorksContext : DbContext
    {
        static AdventureWorksContext()
        {
            Database.SetInitializer<AdventureWorksContext>(null);
        }

        public AdventureWorksContext()
            : base("Name=AdventureWorksContext")
        {
        }

        public AdventureWorksContext(string connectionString)
            : base(connectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new ProductConfiguration());
            modelBuilder.Configurations.Add(new SalesPersonConfiguration());
            modelBuilder.Configurations.Add(new SalesOrderHeaderConfiguration());
        }

        public static DbModelBuilder CreateModel(DbModelBuilder modelBuilder, string schema)
        {
            modelBuilder.Configurations.Add(new ProductConfiguration());
            modelBuilder.Configurations.Add(new SalesPersonConfiguration());
            modelBuilder.Configurations.Add(new SalesOrderHeaderConfiguration());

            return modelBuilder;
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<SalesPerson> SalesPersons { get; set; }
    }
}