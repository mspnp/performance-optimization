using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebRole;

namespace WebRole.Controllers
{
    public class MonoglotController : ApiController
    {
        private string sqlServerConectionString = ConfigurationManager.ConnectionStrings["sqlServerConectionString"].ConnectionString;
        public string Get(int id)
        {
            string result = "";
            try
            {
                MonoglotEventSource.Log.Startup();
                MonoglotEventSource.Log.PageStart(id, this.Url.Request.RequestUri.AbsoluteUri.ToString());
                string queryString = "SELECT Comment from Monoglot WHERE ID=@inputId";
                Stopwatch watch = new Stopwatch();
                watch.Start();
                using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    MonoglotEventSource.Log.ReadDataStart();
                    connection.Open();
                    command.Parameters.AddWithValue("@inputId", id);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        result = String.Format("Comment = {0}", reader[0]);
                    }
                    reader.Close();
                }
                watch.Stop();
                long elapsed = watch.ElapsedMilliseconds;
                MonoglotEventSource.Log.ReadDataFinish(elapsed);
                MonoglotEventSource.Log.PageEnd();
            }
            catch (Exception ex)
            {
                //SQL Server Store is probably not available, log to table storage
                PersistenceErrorEventSource.Log.Failure(ex.Message);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
            return result;
        }
        public void Post([FromBody]string value)
        {
            try
            {
                MonoglotEventSource.Log.Startup();
                MonoglotEventSource.Log.PageStart(1, this.Url.Request.RequestUri.AbsoluteUri.ToString());
                string queryString = "INSERT INTO dbo.Monoglot VALUES (@comment)";
                Stopwatch watch = new Stopwatch();
                watch.Start();
                using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    string comment = value + "_" + new Random().Next(10000, 99999).ToString();
                    connection.Open();
                    command.Parameters.AddWithValue("@comment", comment);
                    MonoglotEventSource.Log.WriteDataStart();
                    command.ExecuteNonQuery();
                }
                watch.Stop();
                long elapsed = watch.ElapsedMilliseconds;
                MonoglotEventSource.Log.WriteDataFinish(elapsed);
                MonoglotEventSource.Log.PageEnd();
            }
            catch (Exception ex)
            {
                //SQL Server Store is probably not available, log to table storage
                PersistenceErrorEventSource.Log.Failure(ex.Message);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        public static void ClearTable()
        {
            string sqlServerConectionString = ConfigurationManager.ConnectionStrings["sqlServerConectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
            {
                String queryString = null;
                SqlCommand command = null;

                queryString = "IF OBJECT_ID('dbo.Monoglot', 'U') IS NOT NULL DROP TABLE dbo.Monoglot";
                command = new SqlCommand(queryString, connection);
                connection.Open();
                command.ExecuteNonQuery();

                queryString = "CREATE TABLE Monoglot (ID int IDENTITY(1,1) PRIMARY KEY,Comment varchar(255) NOT NULL)";
                command = new SqlCommand(queryString, connection);
                command.ExecuteNonQuery();
            }
        }
        public static void PopulateTable()
        {
            string sqlServerConectionString = ConfigurationManager.ConnectionStrings["sqlServerConectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
            {
                SqlCommand command = null;
                string queryString = "INSERT INTO dbo.Monoglot VALUES (@comment)";

                connection.Open();
                for (int i = 1; i < 100; i++)
                {
                    command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@comment", "Comment " + i.ToString());
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
