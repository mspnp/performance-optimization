using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using ServiceBusQueueHandling;

namespace WebRole.Controllers
{
    public class WorkInBackgroundController : ApiController
    {
        private const string ServiceBusConnectionStringKey = "Microsoft.ServiceBus.ConnectionString";

        private const string ServiceBusQueueNameKey = "Microsoft.ServiceBus.QueueName";

        static readonly QueueClient QueueClient;
        static readonly string QueueName;
        static readonly ServiceBusQueueHandler ServiceBusQueueHandler;
        
        static WorkInBackgroundController()
        {
            var serviceBusConnectionString = CloudConfigurationManager.GetSetting(ServiceBusConnectionStringKey);
            QueueName = CloudConfigurationManager.GetSetting(ServiceBusQueueNameKey);
            ServiceBusQueueHandler = new ServiceBusQueueHandler(serviceBusConnectionString);
            QueueClient = ServiceBusQueueHandler.GetQueueClientAsync(QueueName).Result;
        }

        [HttpPost]
        [Route("api/workinbackground")]
        public Task<long> Post()
        {
            return ServiceBusQueueHandler.AddWorkLoadToQueueAsync(
                    QueueClient,
                    QueueName,
                    0);
        }
    }
}
