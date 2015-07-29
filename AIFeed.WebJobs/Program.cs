using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using AIStoreCollection;
using AIFeed.AzureTableStorageEntities;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using AIRssCollection.MSDN;
using AIFeedCommon.StackOverflowCollector;

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
            MSDNFeedCollector.IterateOverExistingFeeds(cloudTable);
        }

        [NoAutomaticTrigger]
        public static void IterateOverRss(
            [Table("ForumThreadsSummery")] CloudTable cloudTable)
        {
            MSDNFeedCollector.IterateOverRss(cloudTable);
        }

        [NoAutomaticTrigger]
        public static void IterateOverRssAndExisingFeeds(
            [Table("ForumThreadsSummery")] CloudTable cloudTable)
        {
            StackOverflowCollector.IterateOverQuestionsWithApplicationInsightsTag(cloudTable);
            MSDNFeedCollector.IterateOverExistingFeeds(cloudTable);
            MSDNFeedCollector.IterateOverRss(cloudTable);
        }
    }
}
