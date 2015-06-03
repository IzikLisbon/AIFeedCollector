using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AIFeedStat.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Dashboard()
        {
            ViewBag.RepliedPercentage = MvcApplication.PercentageOfRepliedThreads;
            ViewBag.AnsweredPercentage = MvcApplication.PercentageOfAcceptedAsAnswerThreads;

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
    }
}