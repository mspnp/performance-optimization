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
using TooMuchProcSql.Support;


namespace TooMuchProcSql.WebApi.Controllers
{
    public class TooMuchProcSqlController : ApiController
    {
     

     
        public async Task GetNameConcat(int Id)
        {
       
            using (var connection = new SqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            {
                await connection.OpenAsync(new System.Threading.CancellationToken());               
                //get query from memory
                string commandString = TooMuchProcUtil.GetQuery("TooMuchSql");               
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    command.CommandType = CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        { 
                            var value = reader.GetFieldValue<string>(0);                             
                        }                     
                    }
                }

            
            }

          
         
        }


    }
}
