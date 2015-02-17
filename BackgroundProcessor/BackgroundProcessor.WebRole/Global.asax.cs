namespace BackgroundProcessor.WebRole
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using BackgroundProcessor.Logic.QueueProcessor;

    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure;

    public class WebApiApplication : HttpApplication
    {
        private const string AppSettingKeyServiceBusConnectionString = "Microsoft.ServiceBus.ConnectionString";

        private const string AppSettingKeyServiceBusQueueName = "Microsoft.ServiceBus.QueueName";

        public static QueueClient QueueClient;

        public static string QueueName;

        protected void Application_Start()
        {
            var storageConnectionString = CloudConfigurationManager.GetSetting(AppSettingKeyServiceBusConnectionString);
            QueueName = CloudConfigurationManager.GetSetting(AppSettingKeyServiceBusQueueName);
            QueueClient = ServiceBusQueueHandler.GetQueueClientAsync(storageConnectionString, QueueName).Result;
            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
