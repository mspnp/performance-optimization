using System.Threading.Tasks;
using System.Web.Http;
using Common.Logic;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace WebRole.Controllers
{
    public class WorkInBackgroundController : ApiController
    {
        private const string ServiceBusConnectionString = "Microsoft.ServiceBus.ConnectionString";

        private const string AppSettingKeyServiceBusQueueName = "Microsoft.ServiceBus.QueueName";

        static readonly QueueClient QueueClient;
        static readonly string QueueName;
        
        static WorkInBackgroundController()
        {
            var serviceBusConnectionString = CloudConfigurationManager.GetSetting(ServiceBusConnectionString);
            QueueName = CloudConfigurationManager.GetSetting(AppSettingKeyServiceBusQueueName);
            QueueClient = ServiceBusQueueHandler.GetQueueClientAsync(serviceBusConnectionString, QueueName).Result;
        }

        public Task Get(double number)
        {
            return ServiceBusQueueHandler.AddWorkLoadToQueueAsync(
                    QueueClient,
                    QueueName,
                    number);
        }
    }
}
