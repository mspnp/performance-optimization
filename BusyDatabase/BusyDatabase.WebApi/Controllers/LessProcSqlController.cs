// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Http;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BusyDatabase.Support;
using Microsoft.Azure;

namespace BusyDatabase.WebApi.Controllers
{
    public class LessProcSqlController : ApiController
    {
        private static readonly string SqlConnectionString = CloudConfigurationManager.GetSetting("connectionString");
        private static readonly string Query = BusyDatabaseUtil.GetQuery("LessSql");

        public async Task<IHttpActionResult> GetNameConcat()
        {
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(Query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    var modified = result.ToString().Replace("ca", "cat").Replace(',', ' ');

                    return Ok(modified);
                }
            }
        }

    }
}
