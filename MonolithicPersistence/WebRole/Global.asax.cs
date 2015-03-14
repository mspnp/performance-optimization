using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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
            CreateSQldbLogTable();
            Logging.Start();
        }
        protected void Application_End()
        {
            Logging.End();
        }

        private static void CreateSQldbLogTable()
        {
            string sqlServerConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
            using (SqlConnection connection = new SqlConnection(sqlServerConnectionString))
            {
                String queryString = null;
                SqlCommand command = null;

                queryString = "IF OBJECT_ID('dbo.SqldbLog', 'U') IS NULL CREATE TABLE SqldbLog (ID int IDENTITY(1,1) PRIMARY KEY, Message TEXT, LogDate DATE)";
                command = new SqlCommand(queryString, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

    }
}
