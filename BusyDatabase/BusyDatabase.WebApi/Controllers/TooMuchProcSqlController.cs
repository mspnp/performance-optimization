// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Http;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using BusyDatabase.Support;
using Microsoft.WindowsAzure;

namespace BusyDatabase.WebApi.Controllers
{
    public class TooMuchProcSqlController : ApiController
    {
        private readonly string _sqlConnectionString;

        public TooMuchProcSqlController()
        {
            _sqlConnectionString = CloudConfigurationManager.GetSetting("connectionString");
        }     

        public async Task<IHttpActionResult> GetNameConcat()
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();               
                // get query from memory
                string commandString = BusyDatabaseUtil.GetQuery("TooMuchSql");               
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    command.CommandType = CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        { 
                            var value = await reader.GetFieldValueAsync<string>(0);
                        }                     
                    }

                    return Ok();
                }
            }
        }
    }
}
