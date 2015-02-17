namespace BackgroundProcessor.WorkerRole
{
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    using BackgroundProcessor.Logic;

    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;

    public class WorkerRole : RoleEntryPoint
    {
        private const string AppSettingKeyServiceBusConnectionString = "Microsoft.ServiceBus.ConnectionString";

        private const string AppSettingKeyServiceBusQueueName = "Microsoft.ServiceBus.QueueName";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        private QueueClient _queueClient;
        private readonly ManualResetEvent _completedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            this._queueClient.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                        Trace.WriteLine(
                            string.Format(
                                "Message properties - Letter count: {0}, Word count: {1}",
                                receivedMessage.Properties["LetterCount"],
                                receivedMessage.Properties["WordCount"]));
                    }
                    catch
                    {
                        // Handle any message processing specific exceptions here
                    }
                });

            this._completedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Setup the reader
            var storageConnectionString = CloudConfigurationManager.GetSetting(AppSettingKeyServiceBusConnectionString);
            var queueName = CloudConfigurationManager.GetSetting(AppSettingKeyServiceBusQueueName);
            this._queueClient = ServiceBusQueueHandler.GetQueueClientAsync(storageConnectionString, queueName).Result;

            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            this._queueClient.Close();
            this._completedEvent.Set();
            base.OnStop();
        }
    }
}
