// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text;
using System.Web.Http;
using BusyDatabase.Support;
using Microsoft.Azure;

namespace BusyDatabase.WebApi.Controllers
{
    public class TooMuchProcSqlController : ApiController
    {
        private static readonly string SqlConnectionString = CloudConfigurationManager.GetSetting("connectionString");
        private static readonly string Query = Queries.Get("TooMuchSql");

        public async Task<IHttpActionResult> Get(int id)
        {
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(Query, connection))
                {
                    command.Parameters.AddWithValue("@TerritoryId", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var xml = new StringBuilder();
                        while (await reader.ReadAsync())
                        {
                            xml.Append(reader.GetString(0));
                        }
                        return ResponseMessage(HttpResponseHelper.CreateMessageFrom(xml.ToString()));
                    }
                }
            }
        }
    }
}
