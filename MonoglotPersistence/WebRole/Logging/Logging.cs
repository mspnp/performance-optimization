using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Web;

namespace WebRole
{
    public class Logging
    {
        private static ObservableEventListener listener0;
        private static ObservableEventListener listener1;
        private static ObservableEventListener listener2;

        private static SinkSubscription<SqlDatabaseSink> subscription0;
        private static SinkSubscription<WindowsAzureTableSink> subscription1;
        private static SinkSubscription<WindowsAzureTableSink> subscription2;
        public static void Start()
        {
            string sqlServerConnectionString = ConfigurationManager.ConnectionStrings["sqlServerConnectionString"].ConnectionString;
            listener0 = new ObservableEventListener();
            listener0.EnableEvents(MonoglotEventSource.Log, EventLevel.Informational);
            subscription0 = listener0.LogToSqlDatabase("Monolithic Anti Pattern", sqlServerConnectionString);

            string azureStorageConnectionString1 = ConfigurationManager.ConnectionStrings["azureStorageConnectionString1"].ConnectionString;
            listener1 = new ObservableEventListener();
            listener1.EnableEvents(PoliglotEventSource.Log, EventLevel.Informational);

            subscription1 = listener1.LogToWindowsAzureTable("Monolithic Anti Pattern", azureStorageConnectionString1);

            string azureStorageConnectionString2 = ConfigurationManager.ConnectionStrings["azureStorageConnectionString2"].ConnectionString;
            listener2 = new ObservableEventListener();
            listener2.EnableEvents(PersistenceErrorEventSource.Log, EventLevel.Informational);
            listener2.EnableEvents(SemanticLoggingEventSource.Log, EventLevel.Informational);
            subscription2 = listener2.LogToWindowsAzureTable("Monolithic Anti Pattern 1", azureStorageConnectionString2);
        }
        public static void End()
        {
            subscription0.Sink.FlushAsync();
            subscription0.Dispose();
            listener0.DisableEvents(MonoglotEventSource.Log);
            listener0.Dispose();

            subscription1.Sink.FlushAsync();
            subscription1.Dispose();
            listener1.DisableEvents(PoliglotEventSource.Log);
            listener1.Dispose();

            subscription2.Sink.FlushAsync();
            subscription2.Dispose();
            listener2.DisableEvents(PersistenceErrorEventSource.Log);
            listener2.Dispose();
        }
    }
}