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
using System.Threading.Tasks;
using System.Web.Http;
using WebRole;

namespace WebRole.Controllers
{
    public class ProductDescriptionController : ApiController
    {
        //private string sqlServerConnectionString = ConfigurationManager.ConnectionStrings["sqlServerConnectionString"].ConnectionString;
        private string sqlServerConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");

        public async Task<string> GetAsync(int id)
        {
            string result = "";
            try
            {
                string queryString = "SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=@inputId";
                using (SqlConnection cn = new SqlConnection(sqlServerConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(queryString, cn))
                    {
                        cmd.Parameters.AddWithValue("@inputId", id);
                        cn.Open();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                result = reader.GetFieldValue<string>(0); ;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log to table storage in case of SQL error 
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
            return result;
        }
    }
}
