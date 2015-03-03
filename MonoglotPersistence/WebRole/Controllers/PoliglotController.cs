using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebRole.Controllers
{
    public class PoliglotController : ApiController
    {
        string sqlServerConectionString = ConfigurationManager.ConnectionStrings["sqlServerConectionString"].ConnectionString;

        // GET: api/Poliglot/5
        public string Get(int id)
        {
            string result = "";
            try
            {
                PoliglotEventSource.Log.Startup();
                PoliglotEventSource.Log.PageStart(id, this.Url.Request.RequestUri.AbsoluteUri.ToString());
                string queryString = "SELECT Comment from Poliglot WHERE ID=@inputId";
                Stopwatch watch = new Stopwatch();
                watch.Start();
                using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    PoliglotEventSource.Log.ReadDataStart();
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
                PoliglotEventSource.Log.ReadDataFinish(elapsed);
                PoliglotEventSource.Log.PageEnd();
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
                PoliglotEventSource.Log.Startup();
                PoliglotEventSource.Log.PageStart(1, this.Url.Request.RequestUri.AbsoluteUri.ToString());
                string queryString = "INSERT INTO dbo.Poliglot VALUES (@comment)";
                Stopwatch watch = new Stopwatch();
                watch.Start();
                using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    string comment = value + "_" + new Random().Next(10000, 99999).ToString();
                    connection.Open();
                    command.Parameters.AddWithValue("@comment", comment);
                    PoliglotEventSource.Log.WriteDataStart();
                    command.ExecuteNonQuery();
                }
                watch.Stop();
                long elapsed = watch.ElapsedMilliseconds;
                PoliglotEventSource.Log.WriteDataFinish(elapsed);
                PoliglotEventSource.Log.PageEnd();
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

                queryString = "IF OBJECT_ID('dbo.Poliglot', 'U') IS NOT NULL DROP TABLE dbo.Poliglot";
                command = new SqlCommand(queryString, connection);
                connection.Open();
                command.ExecuteNonQuery();

                queryString = "CREATE TABLE Poliglot (ID int IDENTITY(1,1) PRIMARY KEY,Comment varchar(255) NOT NULL)";
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
                string queryString = "INSERT INTO dbo.Poliglot VALUES (@comment)";

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
