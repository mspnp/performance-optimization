using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Threading.Tasks;
using BusySql.Support;

namespace BusySql.WebApi.Controllers
{
    public class LessProcSqlController : ApiController
    {
        public async Task GetNameConcat()
        {
            using (var connection = new SqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            {
                await connection.OpenAsync();
                string commandString = BusySqlUtil.GetQuery("LessSql");                
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    command.CommandType = CommandType.Text;
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        
                        while (await reader.ReadAsync())
                        {
                            var field = reader.GetFieldValue<string>(0).Replace(',',' ').Replace("ca","cat"); 
                           
                        }
                    }


                }
            }


           
        }

    }
}
