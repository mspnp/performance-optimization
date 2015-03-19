// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.SqlClient;
using Microsoft.Azure;

namespace WebRole
{
    public class SqldbLogConfig
    {
        static private readonly string SqlDbConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");

        public static void CreateSqldbLogTableIfNotExist()
        {
            using (var connection = new SqlConnection(SqlDbConnectionString))
            {
                const string queryString = "IF OBJECT_ID('dbo.SqldbLog', 'U') IS NULL CREATE TABLE SqldbLog (ID int IDENTITY(1,1) PRIMARY KEY, LogId NCHAR(32), Message TEXT, LogTime DATETIME)";

                using (var command = new SqlCommand(queryString, connection))
                {
                    // These calls are not async because this is a one-time startup function.
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}