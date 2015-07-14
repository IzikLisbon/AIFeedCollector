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
            host.Call(typeof(Program).GetMethod("IterateOverRssAndExisingFeeds"));
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

        [NoAutomaticTrigger]
        public static void IterateOverRssAndExisingFeeds(
            [Table("ForumThreadsSummery")] CloudTable cloudTable)
        {
            FeedsCollector.IterateOverExistingFeeds(cloudTable);
            FeedsCollector.IterateOverRss(cloudTable);
        }
    }
}
