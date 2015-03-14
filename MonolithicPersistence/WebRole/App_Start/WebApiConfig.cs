using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Tracing;
using WebApiContrib.Tracing.Slab;

namespace WebRole
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            CreateSqldbLogTableIfNotExist();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
        private static void CreateSqldbLogTableIfNotExist()
        {
            string sqlServerConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
            using (SqlConnection connection = new SqlConnection(sqlServerConnectionString))
            {
                var queryString = "IF OBJECT_ID('dbo.SqldbLog', 'U') IS NULL CREATE TABLE SqldbLog (ID int IDENTITY(1,1) PRIMARY KEY, Message TEXT, LogDate DATE)";
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

    }
}
