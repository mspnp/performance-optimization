using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebRole.Controllers
{
    public class MonoglotController : ApiController
    {
        private string sqlServerConectionString = ConfigurationManager.ConnectionStrings["sqlServerConectionString"].ConnectionString;
        // GET: api/Monoglot
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Monoglot/5
        public string Get(int id)
        {
            string result = "";
            for (int i = 0; i <= 1000; i++)
            {

                MyCompanyEventSource.Log.Startup();
                MyCompanyEventSource.Log.PageStart(id, this.Url.ToString());
                string queryString = "SELECT ProductID, Name from Production.Product WHERE ProductID=@productId";
                using (SqlConnection connection = new SqlConnection(sqlServerConectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    try
                    {
                        MyCompanyEventSource.Log.ReadDataStart();
                        connection.Open();
                        command.Parameters.AddWithValue("@productId", id);
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            result = String.Format("ProductID={0}, Name={1}", reader[0], reader[1]);
                        }
                        reader.Close();
                        MyCompanyEventSource.Log.ReadDataFinish();
                    }
                    catch (Exception ex)
                    {
                        MyCompanyEventSource.Log.Failure(ex.Message);
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return result;
        }

        // POST: api/Monoglot
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Monoglot/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Monoglot/5
        public void Delete(int id)
        {
        }
    }
}
