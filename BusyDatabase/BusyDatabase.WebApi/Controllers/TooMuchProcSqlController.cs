// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Http;
using System.Threading.Tasks;
using System.Data.SqlClient;
using BusyDatabase.Support;
using Microsoft.Azure;

namespace BusyDatabase.WebApi.Controllers
{
    public class TooMuchProcSqlController : ApiController
    {
        private static readonly string SqlConnectionString = CloudConfigurationManager.GetSetting("connectionString");
        private static readonly string Query = BusyDatabaseUtil.GetQuery("TooMuchSql");

        public async Task<IHttpActionResult> GetNameConcat()
        {
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(Query, connection))
                {
                    var result = await command.ExecuteScalarAsync();

                    return Ok(result);
                }
            }
        }
    }
}
