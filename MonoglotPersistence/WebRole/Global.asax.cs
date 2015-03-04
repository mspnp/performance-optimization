using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using WebRole.Controllers;

namespace WebRole
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            //ResetPurchaseOrderHeaderTable();
            Logging.Start();
        }
        protected void Application_End()
        {
            Logging.End();
        }
        private static void ResetPurchaseOrderHeaderTable()
        {
            string sqlServerConnectionString = ConfigurationManager.ConnectionStrings["sqlServerConnectionString"].ConnectionString;
            String queryString = "DELETE FROM Purchasing.PurchaseOrderHeader WHERE PUrchaseOrderID > 4012";
            using (SqlConnection cn = new SqlConnection(sqlServerConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(queryString, cn))
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();
                }
            }
        }
    }
}
