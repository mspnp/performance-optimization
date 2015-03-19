// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebRole;

namespace WebRole.Controllers
{
    public class ProductDescriptionController : ApiController
    {
        private string sqlDBConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");

        public async Task<string> GetAsync(int id)
        {
            string result = "";
            string queryString = "SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=@inputId";
            using (SqlConnection cn = new SqlConnection(sqlDBConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@inputId", id);
                    await cn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            result = reader.GetFieldValue<string>(0); ;
                        }
                    }
                }
            }
            return result;
        }
    }
}
