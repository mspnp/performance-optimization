// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.SqlClient;
using Microsoft.Azure;
using System;
using WebRole.Controllers;

namespace WebRole
{
    static public class SqldbLogConfig
    {

        public static void CreateSqldbLogTableIfNotExist()
        {
            string ProductionDb = CloudConfigurationManager.GetSetting("ProductionSqlDbCnStr");

            string LogDb = CloudConfigurationManager.GetSetting("LogSqlDbCnStr");

            CreateSqldbLogTableIfNotExist(ProductionDb, MonoController.LogTableName);

            CreateSqldbLogTableIfNotExist(LogDb, PolyController.LogTableName);

        }
        public static void CreateSqldbLogTableIfNotExist(string connectionStr, string LogTableName)
        {
            try
            {
                using (var connection = new SqlConnection(connectionStr))
                {
                    string queryString = "IF OBJECT_ID('dbo." + LogTableName + "', 'U') IS NULL CREATE TABLE " + LogTableName + "(ID int IDENTITY(1,1) PRIMARY KEY, LogId UNIQUEIDENTIFIER, Message TEXT, LogTime DATETIME)";
                    using (var command = new SqlCommand(queryString, connection))
                    {
                        // These calls are not async because this is a one-time startup function.
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // no need to do anything here. May need to manually create the table later.
            }
        }

    }
}