// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using ChattyIO.DataAccess.Models;

namespace ChattyIO.DataAccess
{
    public class AdventureWorksProductContext : DbContext
    {
        static AdventureWorksProductContext()
        {
            Database.SetInitializer<AdventureWorksProductContext>(null);
        }

        public static AdventureWorksProductContext GetEagerContext()
        {
            var context = new AdventureWorksProductContext();
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context;
        }

        public AdventureWorksProductContext()
            : base("Name=AdventureWorksProductContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
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
