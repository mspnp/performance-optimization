using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using ServiceBusQueueHandling;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private const string ServiceBusConnectionStringKey = "Microsoft.ServiceBus.ConnectionString";
        private const string ServiceBusQueueNameKey = "Microsoft.ServiceBus.QueueName";

        private QueueClient _queueClient;
        private readonly ManualResetEvent _completedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            try
            {
                Trace.WriteLine("Starting processing of messages");

                // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
                _queueClient.OnMessageAsync(
                    async receivedMessage =>
                    {
                        try
                        {
                            // Process the message
                            Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());

                            //Simulate processing of message
                            Thread.SpinWait(Int32.MaxValue / 1000);

                            await receivedMessage.CompleteAsync();
                        }
                        catch
                        {
                            receivedMessage.Abandon();
                        }
                    });

                _completedEvent.WaitOne();
            }
            finally
            {
                _completedEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Setup the reader
            var serviceBusConnectionString = CloudConfigurationManager.GetSetting(ServiceBusConnectionStringKey);
            var queueName = CloudConfigurationManager.GetSetting(ServiceBusQueueNameKey);
            var serviceBusQueueHandler = new ServiceBusQueueHandler(serviceBusConnectionString);

            _queueClient = serviceBusQueueHandler.GetQueueClientAsync(queueName).Result;

            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            _queueClient.Close();
            _completedEvent.Set();
            base.OnStop();
        }
    }
}
