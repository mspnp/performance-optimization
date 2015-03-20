// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Http;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using BusyDatabase.Support;
using Microsoft.Azure;

namespace BusyDatabase.WebApi.Controllers
{
    public class LessProcSqlController : ApiController
    {
        private readonly string _sqlConnectionString;

        public LessProcSqlController()
        {
            _sqlConnectionString = CloudConfigurationManager.GetSetting("connectionString");
        }

        public async Task<IHttpActionResult> GetNameConcat()
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                string commandString = BusyDatabaseUtil.GetQuery("LessSql");

                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    command.CommandType = CommandType.Text;
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var field = await reader.GetFieldValueAsync<string>(0);
                            field = field.Replace("ca", "cat").Replace(',', ' ');
                        }
                    }

                    return Ok();
                }
            }
        }

    }
}
