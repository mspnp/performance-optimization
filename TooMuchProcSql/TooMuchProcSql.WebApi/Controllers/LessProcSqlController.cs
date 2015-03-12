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
using TooMuchProcSql.Support;

namespace TooMuchProcSql.WebApi.Controllers
{
    public class LessProcSqlController : ApiController
    {
        public async Task GetNameConcat(int Id)
        {
            using (var connection = new SqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            {
                await connection.OpenAsync(new System.Threading.CancellationToken());

            //    string commandString = SupportFiles.GetSqlQuery("LessProcSql2.txt");  
                string commandString = SupportFiles.GetSqlQuery("LessProcSql.txt");  
                
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    command.CommandType = CommandType.Text;
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        string value=" Name: ";
                        while (await reader.ReadAsync())
                        {
                            var field = reader.GetFieldValue<string>(0);
                            string[] str=field.Split(',');
                            foreach( string s in str)
                            {
                                value = value + s;

                            }

                        }
                    }


                }
            }


            //   return "value";
        }

    }
}
