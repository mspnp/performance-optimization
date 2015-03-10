using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBusQueueHandling
{
    public class ServiceBusQueueHandler
    {
        private NamespaceManager _namespaceManager;
        private string _serviceBusConnectionString;

        public ServiceBusQueueHandler(string serviceBusConnectionString)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
        }

        public async Task<QueueClient> GetQueueClientAsync(string queueName)
        {
            
            if (!_namespaceManager.QueueExists(queueName))
            {
                await _namespaceManager.CreateQueueAsync(queueName).ConfigureAwait(false);
            }

            return QueueClient.CreateFromConnectionString(_serviceBusConnectionString, queueName);
        }

        public async Task<long> AddWordToQueueAsync(QueueClient queueClient, string queueName, string word)
        {
            Debug.Assert(null != _namespaceManager);
            var message = new BrokeredMessage(word);

            await queueClient.SendAsync(message).ConfigureAwait(false);
            var queue = await _namespaceManager.GetQueueAsync(queueName).ConfigureAwait(false);
            return queue.MessageCount;
        }

        /// <summary>
        /// Adds a load of word analyzer tasks to the queue
        /// </summary>
        /// <param name="queueClient">Service bus queue client</param>
        /// <param name="queueName">Service bus queue name</param>
        /// <param name="number"></param>
        /// <returns>Number of messages in the queue</returns>
        public async Task<long> AddWorkLoadToQueueAsync(
            QueueClient queueClient,
            string queueName,
            double number)
        {
            Debug.Assert(null != _namespaceManager);

            var message = new BrokeredMessage(number);
            await queueClient.SendAsync(message).ConfigureAwait(false);

            var queue = await _namespaceManager.GetQueueAsync(queueName).ConfigureAwait(false);
            return queue.MessageCount;
        }
    }
}