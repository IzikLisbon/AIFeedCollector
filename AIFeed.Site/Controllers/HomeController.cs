﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeedStat.Controllers
{
    public class HomeController : Controller
    {   
     
        public ActionResult Dashboard()
        {
            ViewBag.RepliedPercentage = MvcApplication.PercentageOfRepliedThreads;
            ViewBag.AnsweredPercentage = MvcApplication.PercentageOfAcceptedAsAnswerThreads;
            ViewBag.TotalThreads = MvcApplication.TotalThreads;

            return View();
        }

        public ActionResult UserScore()
        {
            // Create tab seperated values that maps an author name to its score.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("authorName\tscore");

            foreach (var userScore in MvcApplication.UserScore.OrderByDescending((userScore) => userScore.Score).Take(8))
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", userScore.AuthorName, userScore.Score));
            }

            return Content(sb.ToString());
        }

        public static bool running = false;

        public ActionResult ReloadData()
        {
            lock (this)
            {
                if (running)
                {
                    return View();
                }
                
                running = true;
            }

            Task.Run(
                () =>
                {   
                    try
                    {
                        // Get CloudTable
                        string webJobStorageConnectionString = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString();
                        CloudStorageAccount account = CloudStorageAccount.Parse(webJobStorageConnectionString);
                        CloudTableClient tableClient = account.CreateCloudTableClient();
                        CloudTable table = tableClient.GetTableReference("ForumThreadsSummery");

                        // // Iterate and update 
                        AIRssCollection.FeedsCollector.IterateOverExistingFeeds(table);
                        AIRssCollection.FeedsCollector.IterateOverRss(table);
                    }
                    catch (Exception e)
                    {
                        // do nothing
                    }
                    finally
                    {
                        running = false;
                    }
                }
            );

            return View();
        }

    }
}