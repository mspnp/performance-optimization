using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace RetrievingTooMuchData.DataAccess.Mapping
{
    //Product
    internal class ProductConfiguration : EntityTypeConfiguration<Product>
    {
        public ProductConfiguration(string schema = "Production")
        {
            ToTable(schema + ".Product");
            HasKey(x => x.ProductId);

            Property(x => x.ProductId).HasColumnName("ProductID").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Name).HasColumnName("Name").IsRequired().HasMaxLength(50);
            Property(x => x.ProductNumber).HasColumnName("ProductNumber").IsRequired().HasMaxLength(25);
            Property(x => x.MakeFlag).HasColumnName("MakeFlag").IsRequired();
            Property(x => x.FinishedGoodsFlag).HasColumnName("FinishedGoodsFlag").IsRequired();
            Property(x => x.Color).HasColumnName("Color").IsOptional().HasMaxLength(15);
            Property(x => x.SafetyStockLevel).HasColumnName("SafetyStockLevel").IsRequired();
            Property(x => x.ReorderPoint).HasColumnName("ReorderPoint").IsRequired();
            Property(x => x.StandardCost).HasColumnName("StandardCost").IsRequired().HasPrecision(19, 4);
            Property(x => x.ListPrice).HasColumnName("ListPrice").IsRequired().HasPrecision(19, 4);
            Property(x => x.Size).HasColumnName("Size").IsOptional().HasMaxLength(5);
            Property(x => x.SizeUnitMeasureCode).HasColumnName("SizeUnitMeasureCode").IsOptional().IsFixedLength().HasMaxLength(3);
            Property(x => x.WeightUnitMeasureCode).HasColumnName("WeightUnitMeasureCode").IsOptional().IsFixedLength().HasMaxLength(3);
            Property(x => x.Weight).HasColumnName("Weight").IsOptional().HasPrecision(8, 2);
            Property(x => x.DaysToManufacture).HasColumnName("DaysToManufacture").IsRequired();
            Property(x => x.ProductLine).HasColumnName("ProductLine").IsOptional().IsFixedLength().HasMaxLength(2);
            Property(x => x.Class).HasColumnName("Class").IsOptional().IsFixedLength().HasMaxLength(2);
            Property(x => x.Style).HasColumnName("Style").IsOptional().IsFixedLength().HasMaxLength(2);
            Property(x => x.ProductSubcategoryId).HasColumnName("ProductSubcategoryID").IsOptional();
            Property(x => x.ProductModelId).HasColumnName("ProductModelID").IsOptional();
            Property(x => x.SellStartDate).HasColumnName("SellStartDate").IsRequired();
            Property(x => x.SellEndDate).HasColumnName("SellEndDate").IsOptional();
            Property(x => x.DiscontinuedDate).HasColumnName("DiscontinuedDate").IsOptional();
            Property(x => x.Rowguid).HasColumnName("rowguid").IsRequired();
            Property(x => x.ModifiedDate).HasColumnName("ModifiedDate").IsRequired();
        }
    }
}