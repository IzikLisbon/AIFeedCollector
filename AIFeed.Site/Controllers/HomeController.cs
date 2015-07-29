using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using AIFeedStat.Models;

namespace AIFeedStat.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
    public class HomeController : Controller
    {
        public static bool Running = false;
        private DateTime LastRunTime = DateTime.Now;

        [NoCache]
        public ActionResult Dashboard()
        {
            ViewBag.RepliedPercentage = DataProvider.Instance.PercentageOfRepliedThreads;
            ViewBag.AnsweredPercentage = DataProvider.Instance.PercentageOfAcceptedAsAnswerThreads;
            ViewBag.TotalThreads = DataProvider.Instance.TotalThreads;
            ViewBag.Reloading = Running;

            return View();
        }

        public ActionResult UserScore()
        {
            // Create tab seperated values that maps an author name to its score.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("authorName\tscore");

            foreach (var userScore in DataProvider.Instance.UserScore.OrderByDescending((userScore) => userScore.Score).Take(8))
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", userScore.AuthorName, userScore.Score));
            }

            string content = sb.ToString();
            return Content(content);
        }

        public ActionResult ReloadData()
        {
            lock (this)
            {
                if (Running)
                {
                    return View();
                }
                
                Running = true;
                LastRunTime = DateTime.Now;
            }

            Task.Run(
                () =>
                {   
                    try
                    {
                        DataProvider.Instance.RefreshFromStorage(DataProvider.Instance.CloudTable, true);
                    }
                    catch (Exception e)
                    {
                        // do nothing
                    }
                    finally
                    {
                        Running = false;
                    }
                }
            );

            return View();
        }

        public ActionResult UnReplied()
        {
            ViewBag.UnRepliedThreads = DataProvider.Instance.UnRepliedThreads;
            ViewBag.UnAnsweredThreads = DataProvider.Instance.UnAnsweredThreads;

            return View();
        }
    }
}