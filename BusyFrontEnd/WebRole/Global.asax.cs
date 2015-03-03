using System.Web;
using System.Web.Http;
using Common.Logic;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace WebRole
{
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

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
