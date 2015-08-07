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

        public ActionResult Dashboard()
        {
            ViewBag.RepliedPercentage = DataProvider.Instance.PercentageOfRepliedThreads;
            ViewBag.AnsweredPercentage = DataProvider.Instance.PercentageOfAcceptedAsAnswerThreads;
            ViewBag.TotalThreads = DataProvider.Instance.TotalThreads;
            ViewBag.Reloading = DataProvider.Instance.Running;
            ViewBag.ShowLastReloadTime = DataProvider.Instance.LatestRefreshTime.HasValue;
            ViewBag.LastReloadTime = DataProvider.Instance.LatestRefreshTime;

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

        public EmptyResult ReloadData()
        {
            Task.Run(() => DataProvider.Instance.RefreshFromStorage(DataProvider.Instance.CloudTable, true));
            return new EmptyResult();
        }

        public JsonResult IsRefreshing()
        {
            string date = "unknown";
            if (DataProvider.Instance.LatestRefreshTime.HasValue)
            {
                date = DataProvider.Instance.LatestRefreshTime.Value.ToLocalTime().ToString("dd MMM yyyy hh:mm:ss");
            }

            return Json(
                new 
                {
                    running = DataProvider.Instance.Running,
                    lastRunTime = date
                }, 
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnReplied()
        {
            ViewBag.UnRepliedThreads = DataProvider.Instance.UnRepliedThreads;
            ViewBag.UnAnsweredThreads = DataProvider.Instance.UnAnsweredThreads;

            return View();
        }
    }
}