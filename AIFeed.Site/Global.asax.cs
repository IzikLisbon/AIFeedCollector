using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AIFeed.AzureTableStorageEntities;
using AIFeedStat.StatisticsCalculators;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeedStat
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static IReadOnlyList<ForumThreadEntity> ForumThreads { get; private set; }
        public static List<UserScore> UserScore { get; private set; }
        public static int PercentageOfRepliedThreads { get; private set; }
        public static int PercentageOfAcceptedAsAnswerThreads { get; private set; }
        public static int TotalThreads { get; set; }
        public static IEnumerable<string> UnRepliedThreads { get; set; }

        public static IEnumerable<string> UnAnsweredThreads { get; set; }
        

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RefreshFromStoragePeriodically();
        }

        private async void RefreshFromStoragePeriodically()
        {
            // Get CloudTable
            string webJobStorageConnectionString = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString();
            CloudStorageAccount account = CloudStorageAccount.Parse(webJobStorageConnectionString);
            CloudTableClient tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("ForumThreadsSummery");

            RefreshFromStorage(table);

            // call this method every 2 hours.
            await Task.Delay(TimeSpan.FromHours(2));
            RefreshFromStoragePeriodically();
        }

        public static void RefreshFromStorage(CloudTable table)
        {   
            ForumThreads = table.CreateQuery<ForumThreadEntity>().ToList();

            ProcessUserScore userScore = new ProcessUserScore(ForumThreads);
            userScore.Process();
            UserScore = userScore.UserScoreList;

            UnRepliedThreads = ForumThreads.Where<ForumThreadEntity>(thread => !thread.HasReplies).Select((thread) => thread.Path);
            UnAnsweredThreads = ForumThreads.Where<ForumThreadEntity>(thread => !thread.IsAnswerAccepted).Select((thread) => thread.Path);
            int repliedCount = ForumThreads.Count((thread) => thread.HasReplies);
            int answeredCount = ForumThreads.Count((thread) => thread.IsAnswerAccepted);

            PercentageOfRepliedThreads = (int)(((double)repliedCount / (double)ForumThreads.Count) * 100);
            PercentageOfAcceptedAsAnswerThreads = (int)(((double)answeredCount / (double)ForumThreads.Count) * 100);
            TotalThreads = ForumThreads.Count();
        }
    }
}
