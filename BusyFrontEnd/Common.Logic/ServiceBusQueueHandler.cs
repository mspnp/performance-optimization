using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task<long> AddWorkLoadToQueueAsync(
            QueueClient queueClient,
            string queueName,
            string data)
        {
            Debug.Assert(null != _namespaceManager);

            var message = new BrokeredMessage(data);
            await queueClient.SendAsync(message).ConfigureAwait(false);

            var queue = await _namespaceManager.GetQueueAsync(queueName).ConfigureAwait(false);
            return queue.MessageCount;
        }
    }
}
