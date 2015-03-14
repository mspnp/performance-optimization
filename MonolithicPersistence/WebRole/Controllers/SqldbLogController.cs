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
    public class SqldbLogController : ApiController
    {
        private string sqlServerConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
        public async Task PostAsync([FromBody]string value)
        {
            try
            {
                string queryString = "INSERT INTO dbo.SqldbLog(Message, LogDate) VALUES(@Message, @LogDate)";
                var dt = DateTime.UtcNow;
                using (SqlConnection cn = new SqlConnection(sqlServerConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(queryString, cn))
                    {
                        cmd.Parameters.Add("@Message", SqlDbType.NText).Value = "My Random Log Message " + new Random().Next();
                        cmd.Parameters.Add("@LogDate", SqlDbType.DateTime).Value = dt;
                        cn.Open();
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                //SQL Server Store is probably not available, log to table storage
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
    }
}
