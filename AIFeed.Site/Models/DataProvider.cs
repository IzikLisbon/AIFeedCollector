using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using AIFeed.AzureTableStorageEntities;
using AIFeedCommon.StackOverflowCollector;
using AIFeedStat.StatisticsCalculators;
using AIRssCollection.MSDN;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeedStat.Models
{
    public class DataProvider
    {
        private static DataProvider DataProviderInstance;

        public static DataProvider Instance
        {
            get
            {
                return LazyInitializer.EnsureInitialized<DataProvider>(ref DataProviderInstance, () => new DataProvider());
            }
        }

        public CloudTable CloudTable { get; private set; }

        private DataProvider() 
        {
            this.CloudTable = GetCloudTable();
            this.Running = false;
        }

        public IReadOnlyList<ForumThreadEntity> ForumThreads { get; private set; }
        public List<UserScore> UserScore { get; private set; }
        public int PercentageOfRepliedThreads { get; private set; }
        public int PercentageOfAcceptedAsAnswerThreads { get; private set; }
        public int TotalThreads { get; set; }
        public IEnumerable<string> UnRepliedThreads { get; set; }
        public IEnumerable<string> UnAnsweredThreads { get; set; }
        public DateTime? LatestRefreshTime { get; set; }
        public bool Running { get; set; }

        /// <summary>
        /// Refreshes all the static fields (UnRepliedThreads, UnAnsweredThreads, repliedCount...) from Storage. 
        /// If <paramref name="refreshStorageFromSources"/> is true then the storage itself is being refrehsed from Sources (Stackoverflow, MSDN...).
        /// </summary>
        public void RefreshFromStorage(CloudTable table, bool refreshStorageFromSources)
        {
            // if already running - quit.
            lock (this)
            {
                if (this.Running)
                {
                    return;
                }

                Running = true;
            }

            try
            {
                if (refreshStorageFromSources)
                {
                    MSDNFeedCollector.IterateOverExistingFeeds(table);
                    MSDNFeedCollector.IterateOverRss(table);
                    StackOverflowCollector.IterateOverQuestionsWithApplicationInsightsTag(table);
                }

                this.ForumThreads = table.CreateQuery<ForumThreadEntity>().ToList();

                ProcessUserScore userScore = new ProcessUserScore(ForumThreads);
                userScore.Process();
                this.UserScore = userScore.UserScoreList;

                this.UnRepliedThreads = this.ForumThreads.Where<ForumThreadEntity>(thread => !thread.HasReplies).Select((thread) => thread.Path);
                this.UnAnsweredThreads = this.ForumThreads.Where<ForumThreadEntity>(thread => !thread.IsAnswerAccepted).Select((thread) => thread.Path);
                int repliedCount = this.ForumThreads.Count((thread) => thread.HasReplies);
                int answeredCount = this.ForumThreads.Count((thread) => thread.IsAnswerAccepted);

                this.PercentageOfRepliedThreads = (int)(((double)repliedCount / (double)this.ForumThreads.Count) * 100);
                this.PercentageOfAcceptedAsAnswerThreads = (int)(((double)answeredCount / (double)this.ForumThreads.Count) * 100);
                this.TotalThreads = this.ForumThreads.Count();
                LatestRefreshTime = DateTime.Now;
            }
            finally
            {
                this.Running = false;
            }
        }

        /// <summary>
        /// Get the azure cloud table that holds all the data. 
        /// </summary>
        public static CloudTable GetCloudTable(string tableName = "ForumThreadsSummery")
        {
            string webJobStorageConnectionString = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString();
            CloudStorageAccount account = CloudStorageAccount.Parse(webJobStorageConnectionString);
            CloudTableClient tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);

            return table;
        }
    }
}