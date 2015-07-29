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
using AIFeedCommon.StackOverflowCollector;
using AIFeedStat.Models;
using AIFeedStat.StatisticsCalculators;
using AIRssCollection.MSDN;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeedStat
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DataProvider.Instance.RefreshFromStorage(DataProvider.Instance.CloudTable, refreshStorageFromSources: false);

            Task.Factory.StartNew(RefreshFromStorageAndSourcesPeriodically, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Every 2 hours, refresh all the static fields (UnRepliedThreads, UnAnsweredThreads, repliedCount...) from Storage 
        /// and the storage itself is being refrehsed from Sources (Stackoverflow, MSDN...).
        /// </summary>
        private void RefreshFromStorageAndSourcesPeriodically()
        {
            DataProvider.Instance.RefreshFromStorage(DataProvider.Instance.CloudTable, refreshStorageFromSources: true);

            Task.Delay(TimeSpan.FromHours(2)).GetAwaiter().GetResult();
            RefreshFromStorageAndSourcesPeriodically();
        }   
    }
}
