using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebRole.Logging;

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
                string queryString = "SELECT ProductID, Name from Production.Product WHERE ProductID=@productId";
                Stopwatch watch = new Stopwatch();
                watch.Start();
                using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    MonoglotEventSource.Log.ReadDataStart();
                    connection.Open();
                    command.Parameters.AddWithValue("@productId", id);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        result = String.Format("ProductID={0}, Name={1}", reader[0], reader[1]);
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
        public void Put([FromBody]string value)
        {
            try
            {
                int addressID = getRandomAddresID();
                MonoglotEventSource.Log.Startup();
                MonoglotEventSource.Log.PageStart(addressID, this.Url.Request.RequestUri.AbsoluteUri.ToString());
                string queryString = "UPDATE Person.Address SET AddressLine1 = @addressLine1 WHERE AddressID =@addressID";
                Stopwatch watch = new Stopwatch();
                watch.Start();
                using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
                {
                    Random rand = new Random();
                    int randNumber = rand.Next(10000, 99999);
                    string addressLine1 = randNumber.ToString() + " " + value;
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    command.Parameters.AddWithValue("@addressID", addressID);
                    command.Parameters.AddWithValue("@addressLine1", addressLine1);
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
        private int getRandomAddresID()
        {
            int addressID = 1;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
            {
                MonoglotEventSource.Log.ReadDataStart();
                // SELECT A RANDOME ROW 
                string queryString = "SELECT TOP 1 AddressID from Person.Address ORDER BY NEWID()";
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    addressID = (int)reader[0];
                }
                reader.Close();
            }
            watch.Stop();
            long elapsed = watch.ElapsedMilliseconds;
            MonoglotEventSource.Log.ReadDataFinish(elapsed);
            return addressID;
        }

    }
}
