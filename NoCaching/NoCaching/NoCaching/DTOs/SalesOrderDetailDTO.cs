﻿namespace NoCaching.DTOs
{
    [Serializable]
    public class SalesOrderDetailDTO
    {
        public int SalesOrderDetailID { get; set; }
        public int OrderQty { get; set; }
        public int ProductID { get; set; }
    }
}
