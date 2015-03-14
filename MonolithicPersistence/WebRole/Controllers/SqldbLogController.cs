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
using WebRole.Models;

namespace WebRole.Controllers
{
    public class SqldbLogController : ApiController
    {
        private static string sqlDBConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            LogMessage logMessage = new LogMessage();
            await LogToSqldbAsync(logMessage).ConfigureAwait(false);
            return Ok();
        }

        private static async Task LogToSqldbAsync(LogMessage logMessage)
        {
            string queryString = "INSERT INTO dbo.SqldbLog(Message, LogId, LogTime) VALUES(@Message, @LogId, @LogTime)";
            using (SqlConnection cn = new SqlConnection(sqlDBConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.Add("@LogId", SqlDbType.NChar, 32).Value = logMessage.LogId;
                    cmd.Parameters.Add("@Message", SqlDbType.NText).Value = logMessage.Message;
                    cmd.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = logMessage.LogTime;
                    await cn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

        }

    }
}
