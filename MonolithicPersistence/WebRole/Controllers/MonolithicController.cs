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
    public class MonolithicController : ApiController
    {
        public async Task<IHttpActionResult> PostAsync([FromBody]string value)
        {
            int logCount = Convert.ToInt32(value);
            for (int i = 0; i < logCount; i++)
            {
                LogMessage logMessage = new LogMessage();
                await DataAccess.LogToSqldbAsync(logMessage);
            }
            await DataAccess.SelectProductDescriptionAsync(321);
            await DataAccess.InsertToPurchaseOrderHeaderTableAsync();
            return Ok();
        }

    }
}
