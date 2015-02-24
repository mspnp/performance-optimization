using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Web;

namespace WebRole.Logging
{
    public class MyCompanyLogging
    {
        private static ObservableEventListener listener1;
        private static ObservableEventListener listener2;
        private static SinkSubscription<SqlDatabaseSink> subscription1;
        private static SinkSubscription<WindowsAzureTableSink> subscription2;
        public static void Start()
        {
            string sqlServerConectionString = ConfigurationManager.ConnectionStrings["sqlServerConectionString"].ConnectionString;
            listener1 = new ObservableEventListener();
            listener1.EnableEvents(MyCompanyEventSource.Log, EventLevel.Informational);
            subscription1 = listener1.LogToSqlDatabase("Monoglot Persistence", sqlServerConectionString);

            string azureTableConnectionString = ConfigurationManager.ConnectionStrings["azureTableConnectionString"].ConnectionString;
            listener2 = new ObservableEventListener();
            listener2.EnableEvents(MyCompanyEventSource.Log, EventLevel.Informational);
            subscription2 = listener2.LogToWindowsAzureTable("Poliglot Persistence", azureTableConnectionString);
        }
        public static void End()
        {
            subscription1.Sink.FlushAsync();
            subscription1.Dispose();
            listener1.DisableEvents(MyCompanyEventSource.Log);
            listener1.Dispose();

            subscription2.Sink.FlushAsync();
            subscription2.Dispose();
            listener2.DisableEvents(MyCompanyEventSource.Log);
            listener2.Dispose();
        }
    }
}