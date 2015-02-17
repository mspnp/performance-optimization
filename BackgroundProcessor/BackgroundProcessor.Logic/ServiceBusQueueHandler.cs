namespace BackgroundProcessor.Logic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using BackgroundProcessor.WebRole.Models;

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
        /// <param name="wordsMap">Dictionary of various length words</param>
        /// <returns>Number of messages in the queue</returns>
        public static async Task<long> AddWorkLoadToQueueAsync(
            QueueClient queueClient,
            string queueName,
            IDictionary<LetterWordCount, ICollection<string>> wordsMap)
        {
            Debug.Assert(null != _namespaceManager);

            // Add each batch of words as a discrete message on the queue
            foreach (var kvp in wordsMap)
            {
                var message = new BrokeredMessage(kvp.Value);
                message.Properties.Add("LetterCount", kvp.Key.LetterCount);
                message.Properties.Add("WordCount", kvp.Key.WordCount);
                await queueClient.SendAsync(message).ConfigureAwait(false);
            }
           
            var queue = await _namespaceManager.GetQueueAsync(queueName).ConfigureAwait(false);
            return queue.MessageCount;
        }
    }
}