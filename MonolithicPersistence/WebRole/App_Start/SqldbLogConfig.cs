// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WebRole.App_Start
{
    public class SqldbLogConfig
    {
        static private string sqlDBConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
        public static void CreateSqldbLogTableIfNotExist()
        {
            using (SqlConnection connection = new SqlConnection(sqlDBConnectionString))
            {
                var queryString = "IF OBJECT_ID('dbo.SqldbLog', 'U') IS NULL CREATE TABLE SqldbLog (ID int IDENTITY(1,1) PRIMARY KEY, LogId NCHAR(32), Message TEXT, LogTime DATETIME)";
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                //the following call is not async because it is a one time startup function.
                command.ExecuteNonQuery();
            }
        }
    }
}