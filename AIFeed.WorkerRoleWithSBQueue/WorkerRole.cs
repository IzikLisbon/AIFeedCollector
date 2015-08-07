using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using AIFeedCommon;
using AIFeedCommon.StackOverflowCollector;
using AIRssCollection;
using AIRssCollection.MSDN;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeed.WorkerRoleWithSBQueue
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "RefreshQueue";
        
        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);
        bool Refreshing;
        private RefreshManager RefreshManager;

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            Client.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                        if (receivedMessage != null)
                        {
                            // Process the message
                            Trace.WriteLine("Processing", receivedMessage.SequenceNumber.ToString());

                            // Add this code
                            // View the message as an OnlineOrder
                            RefreshMessage refreshMessage = receivedMessage.GetBody<RefreshMessage>();
                            Trace.WriteLine("refreshMessage " + refreshMessage == null ? "is null" : "has value");

                            Refresh(forceRefresh: false);

                            receivedMessage.Complete();
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("Refresh failure: {0}", e);
                    }
                });

            CompletedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("MicrosoftServiceBusConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            // Initialize the connection to Service Bus Queue
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            RefreshManager = new RefreshManager(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            try
            {
                Refresh(forceRefresh: true);
            }
            catch (Exception e)
            {
                Trace.TraceError("Refresh failure: {0}", e);
            }

            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            Client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }

        private void Refresh(bool forceRefresh)
        {
            if (!Refreshing || forceRefresh)
            {
                this.RefreshManager.UpdateStartDate(DateTime.UtcNow);
                Refreshing = true;

                try
                {
                    CloudTable cloudTable = AzureTableUtils.GetCloudTable(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                    StackOverflowCollector.IterateOverQuestionsWithApplicationInsightsTag(cloudTable);
                    MSDNFeedCollector.IterateOverExistingFeeds(cloudTable);
                    MSDNFeedCollector.IterateOverRss(cloudTable);

                    this.RefreshManager.UpdateEndRefreshDate(DateTime.UtcNow);
                }
                catch
                {
                    Refreshing = false;
                    throw;
                }
            }
        }
    }
}
