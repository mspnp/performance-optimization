// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using BusyDatabase.Support;


namespace BusyDatabase.WebApi.Controllers
{
    public class TooMuchProcSqlController : ApiController
    {
     

     
        public async Task<IHttpActionResult> GetNameConcat()
        {

                             
            using (var connection = new SqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            {
                await connection.OpenAsync();               
                //get query from memory
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
