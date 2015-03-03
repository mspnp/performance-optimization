using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Common.Logic
{
    public class ServiceBusQueueHandler
    {
        private static NamespaceManager _namespaceManager;
        public static async Task<QueueClient> GetQueueClientAsync(string sbConnectionString, string queueName)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(sbConnectionString);

            if (!_namespaceManager.QueueExists(queueName))
            {
                await _namespaceManager.CreateQueueAsync(queueName).ConfigureAwait(false);
            }

            return QueueClient.CreateFromConnectionString(sbConnectionString, queueName);
        }

        public static async Task<long> AddWordToQueueAsync(QueueClient queueClient, string queueName, string word)
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
        public static async Task<long> AddWorkLoadToQueueAsync(
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