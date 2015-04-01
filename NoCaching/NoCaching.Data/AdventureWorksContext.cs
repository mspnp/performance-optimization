// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Entity;
using System.Data.Entity.SqlServer;
using Microsoft.Azure;
using NoCaching.Data.Models;

namespace NoCaching.Data
{
    public class AdventureWorksContext : DbContext
    {
        static AdventureWorksContext()
        {
            Database.SetInitializer<AdventureWorksContext>(null);
        }

        public AdventureWorksContext()
            : this(CloudConfigurationManager.GetSetting("AdventureWorksConnectionString"))
        {
        }

        public AdventureWorksContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .ToTable("Person", "Person")
                .HasKey(p => p.Id)
                .Property(p => p.Id)
                .HasColumnName("BusinessEntityID");

            modelBuilder.Entity<Employee>()
                .ToTable("Employee", "HumanResources");

            modelBuilder.Entity<SalesPerson>()
                .ToTable("SalesPerson", "Sales")
                .Property(sp => sp.CommissionPercentage)
                .HasColumnName("CommissionPct");

            modelBuilder.Entity<Customer>()
                .ToTable("Customer", "Sales")
                .HasKey(c => c.CustomerId)
                .Property(c => c.CustomerId)
                .HasColumnName("CustomerID");

            modelBuilder.Entity<Customer>()
                .HasRequired(c => c.Person)
                .WithMany()
                .Map(m => m.MapKey("PersonID"));

            modelBuilder.Entity<SalesOrderHeader>()
                .ToTable("SalesOrderHeader", "Sales")
                .HasKey(soh => soh.Id)
                .Property(soh => soh.Id)
                .HasColumnName("SalesOrderID");

            modelBuilder.Entity<SalesOrderHeader>()
                .HasRequired(soh => soh.Customer)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SalesOrderHeader>()
                .HasOptional(soh => soh.SalesPerson)
                .WithMany()
                .Map(m => m.MapKey("SalesPersonID"))
                .WillCascadeOnDelete(false);
        }

        public virtual DbSet<Person> People { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<Employee> Employees { get; set; }

        public virtual DbSet<SalesPerson> SalesPeople { get; set; }

        public virtual DbSet<SalesOrderHeader> SalesOrders { get; set; }
    }

    public class AdventureWorksConfiguration : DbConfiguration
    {
        public AdventureWorksConfiguration()
        {
            this.SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);
            this.SetExecutionStrategy(SqlProviderServices.ProviderInvariantName, () => new SqlAzureExecutionStrategy());
        }
    }
}
