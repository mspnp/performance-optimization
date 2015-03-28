// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using ExtraneousFetching.DataAccess.Mapping;

namespace ExtraneousFetching.DataAccess
{
    public class AdventureWorksContext : DbContext
    {
        static AdventureWorksContext()
        {
            Database.SetInitializer<AdventureWorksContext>(null);
            DbInterception.Add(new ConnectionInterceptor());
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