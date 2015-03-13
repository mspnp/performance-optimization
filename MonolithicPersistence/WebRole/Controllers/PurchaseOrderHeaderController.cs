using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebRole;

namespace WebRole.Controllers
{
    public class PurchaseOrderHeaderController : ApiController
    {
        private string sqlServerConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
        public void Post([FromBody]string value)
        {
            try
            {
                string queryString =
                    "INSERT INTO Purchasing.PurchaseOrderHeader(" +
                    " RevisionNumber, Status, EmployeeID, VendorID, ShipMethodID, OrderDate, ShipDate, SubTotal, TaxAmt, Freight, ModifiedDate)" +
                    " VALUES(" +
                    "@RevisionNumber,@Status,@EmployeeID,@VendorID,@ShipMethodID,@OrderDate,@ShipDate,@SubTotal,@TaxAmt,@Freight,@ModifiedDate)";
                var dt = DateTime.Now;
                using (SqlConnection cn = new SqlConnection(sqlServerConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(queryString, cn))
                    {
                        MonoglotEventSource.Log.WriteDataStart();

                        cmd.Parameters.Add("@RevisionNumber", SqlDbType.TinyInt).Value = 1;
                        cmd.Parameters.Add("@Status", SqlDbType.TinyInt).Value = 4;
                        cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = 258;
                        cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = 1580;
                        cmd.Parameters.Add("@ShipMethodID", SqlDbType.Int).Value = 3;
                        cmd.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = dt;
                        cmd.Parameters.Add("@ShipDate", SqlDbType.DateTime).Value = dt;
                        cmd.Parameters.Add("@SubTotal", SqlDbType.Money).Value = 123.40;
                        cmd.Parameters.Add("@TaxAmt", SqlDbType.Money).Value = 12.34;
                        cmd.Parameters.Add("@Freight", SqlDbType.Money).Value = 5.76;
                        cmd.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = dt;

                        cn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                //SQL Server Store is probably not available, log to table storage
                PersistenceErrorEventSource.Log.Failure(ex.Message);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
    }
}
