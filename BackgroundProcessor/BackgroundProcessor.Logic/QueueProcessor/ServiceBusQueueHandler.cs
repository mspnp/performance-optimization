namespace BackgroundProcessor.Logic.QueueProcessor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using BackgroundProcessor.Logic.WordProcessor;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

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
            Debug.Assert(null!=_namespaceManager);
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
        /// <param name="lwcList"></param>
        /// <returns>Number of messages in the queue</returns>
        public static async Task<long> AddWorkLoadToQueueAsync(
            QueueClient queueClient,
            string queueName,
            ICollection<LetterWordCount> lwcList)
        {
            Debug.Assert(null != _namespaceManager);

            // Add each batch of words as a discrete message on the queue
            foreach (var lwc in lwcList)
            {
                var message = new BrokeredMessage(lwc);
                message.Properties.Add("LetterCount",lwc.LetterCount);
                message.Properties.Add("WordCount", lwc.WordCount);
                await queueClient.SendAsync(message).ConfigureAwait(false);
            }
           
            var queue = await _namespaceManager.GetQueueAsync(queueName).ConfigureAwait(false);
            return queue.MessageCount;
        }
    }
}