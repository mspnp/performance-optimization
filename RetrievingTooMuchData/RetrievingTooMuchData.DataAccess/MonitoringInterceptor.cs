// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;

namespace RetrievingTooMuchData.DataAccess
{
    public class MonitoringInterceptor : IDbCommandInterceptor
    {
        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            PrepareTimingOperation(interceptionContext);
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogExecutionTime(command, interceptionContext);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            PrepareTimingOperation(interceptionContext);
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogExecutionTime(command, interceptionContext);
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            PrepareTimingOperation(interceptionContext);
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogExecutionTime(command, interceptionContext);
        }

        private void PrepareTimingOperation<T>(DbCommandInterceptionContext<T> interceptionContext)
        {
            // Note: By using the GetTimestamp method instead of using a new Stopwatch instance for each operation,
            //       we reduce the amount of objects that are created in heap memory. This reduces the amount
            //       of garbage collection required.

            interceptionContext.UserState = Stopwatch.GetTimestamp();
        }

        private static long GetExecutionTimeInMilliseconds<T>(DbCommandInterceptionContext<T> interceptionContext)
        {
            long difference = Stopwatch.GetTimestamp() - ((long)interceptionContext.UserState);
            long elapsedMilliseconds = (long)(difference * (1000.0 / Stopwatch.Frequency));
            return elapsedMilliseconds;
        }

        private void LogExecutionTime<T>(IDbCommand command, DbCommandInterceptionContext<T> interceptionContext)
        {
            var elapsedMilliseconds = GetExecutionTimeInMilliseconds(interceptionContext);
            var commandText = command.CommandText;

            // TODO: Call your monitoring API here with the timing information, and optionally the command information
        }
    }
}
