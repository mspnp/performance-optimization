// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace ExtraneousFetching.DataAccess
{
    public class Product
    {
        public int ProductId { get; set; } // ProductID (Primary key)

        public string Name { get; set; } // Name

        public string ProductNumber { get; set; } // ProductNumber

        public bool MakeFlag { get; set; } // MakeFlag

        public bool FinishedGoodsFlag { get; set; } // FinishedGoodsFlag

        public string Color { get; set; } // Color

        public short SafetyStockLevel { get; set; } // SafetyStockLevel

        public short ReorderPoint { get; set; } // ReorderPoint

        public decimal StandardCost { get; set; } // StandardCost

        public decimal ListPrice { get; set; } // ListPrice

        public string Size { get; set; } // Size

        public string SizeUnitMeasureCode { get; set; } // SizeUnitMeasureCode

        public string WeightUnitMeasureCode { get; set; } // WeightUnitMeasureCode

        public decimal? Weight { get; set; } // Weight

        public int DaysToManufacture { get; set; } // DaysToManufacture

        public string ProductLine { get; set; } // ProductLine

        public string Class { get; set; } // Class

        public string Style { get; set; } // Style

        public int? ProductSubcategoryId { get; set; } // ProductSubcategoryID

        public int? ProductModelId { get; set; } // ProductModelID

        public DateTime SellStartDate { get; set; } // SellStartDate

        public DateTime? SellEndDate { get; set; } // SellEndDate

        public DateTime? DiscontinuedDate { get; set; } // DiscontinuedDate

        public Guid Rowguid { get; set; } // rowguid

        public DateTime ModifiedDate { get; set; } // ModifiedDate
    }
}