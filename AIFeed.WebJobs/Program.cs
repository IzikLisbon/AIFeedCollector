using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using AIStoreCollection;
using AIStoreCollection.HtmlModel;
using AIFeed.AzureTableStorageEntities;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using AIRssCollection;

namespace AIFeedCollector.WebJobs
{
    public class Program
    {
        static void Main(string[] args)
        {   
            JobHost host = new JobHost();
            host.Call(typeof(Program).GetMethod("IterateOverRss"));

            // uncomment this line to update all the threads in the CloudTable. 
            // host.Call(typeof(Program).GetMethod("IterateOverExistingFeeds"));
        }

        [NoAutomaticTrigger]
        public static void IterateOverExistingFeeds(
            [Table("ForumThreadsSummery")] CloudTable cloudTable)
        {
            FeedsCollector.IterateOverExistingFeeds(cloudTable);
        }

        [NoAutomaticTrigger]
        public static void IterateOverRss(
            [Table("ForumThreadsSummery")] CloudTable cloudTable)
        {
            FeedsCollector.IterateOverRss(cloudTable);
        }
    }
}
