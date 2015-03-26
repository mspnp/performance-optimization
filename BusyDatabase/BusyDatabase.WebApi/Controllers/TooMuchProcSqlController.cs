// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Http;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
                    command.Parameters.AddWithValue("@OrderId", id);

                    var reader = await command.ExecuteReaderAsync();
                    var xml = new StringBuilder();
                    while (await reader.ReadAsync())
                    {
                        xml.Append(reader.GetString(0));
                    }

                    return ResponseMessage(CreateMessageFrom(xml.ToString()));
                }
            }
        }

        //TODO: move this to a better location
        public static HttpResponseMessage CreateMessageFrom(string result)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var mediaType = new MediaTypeHeaderValue("application/xml");
            mediaType.Parameters.Add(new NameValueHeaderValue("charset", "utf-8"));
            response.Content = new StringContent(result);
            response.Content.Headers.ContentType = mediaType;
            return response;
        }
    }
}
