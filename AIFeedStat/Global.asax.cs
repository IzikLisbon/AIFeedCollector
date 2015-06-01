using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
        public static IReadOnlyList<ForumThreadEntity> ForumThread { get; private set; }
        public static List<UserScore> UserScore { get; private set; }
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            InitializeStorage();
        }

        private void InitializeStorage()
        {
            // Open storage account using credentials from .cscfg file.
            string webJobStorageConnectionString = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString();
            CloudStorageAccount account = CloudStorageAccount.Parse(webJobStorageConnectionString);

            // Create the table client. 
            CloudTableClient tableClient = account.CreateCloudTableClient();
            
            // Create the table if it doesn't exist. 
            CloudTable table = tableClient.GetTableReference("ForumThreadsSummery");

            ForumThread = table.CreateQuery<ForumThreadEntity>().ToList();

            ProcessUserScore userScore = new ProcessUserScore(ForumThread);
            userScore.Process();
            UserScore = userScore.UserScoreList;
        }
    }
}
