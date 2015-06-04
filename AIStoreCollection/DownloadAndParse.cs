using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AIStoreCollection.HtmlModel;
using HtmlAgilityPack;

namespace AIStoreCollection
{
    public class DownloadAndParse
    {
        const int guidLength = 36;

        /// <summary>
        /// Collect data for all threads. For every thread collect its RSS and HTML. 
        /// </summary>
        public async static Task<List<ThreadInfo>> StartAsync()
        {
            List<ThreadInfo> threadList = new List<ThreadInfo>();
            rss aiRss = await ReadRss("https://social.msdn.microsoft.com/Forums/en-US/ApplicationInsights/threads?outputAs=rss");
            List<Task> tasks = new List<Task>();

            foreach (rssItem item in aiRss.channel.items)
            {
                int guidIndex = item.link.IndexOf("en-US/") + "en-US/".Length;
                string uri = string.Format("https://social.msdn.microsoft.com/Forums/en-US/ApplicationInsights/thread/{0}?outputAs=rss", item.link.Substring(guidIndex, guidLength));

                Task<rss> rssTask = ReadRss(uri);
                Task<string> htmlTask = ReadHtml(item.link);
                
                Task.WaitAll(rssTask, htmlTask);
                
                threadList.Add(new ThreadInfo()
                {
                    ThreadItemInRootRss = item,
                    Rss = rssTask.Result,
                    html = htmlTask.Result,
                    htmlModel = ParseHtml(htmlTask.Result)
                });
            }

            return threadList;
        }

        private async static Task<bool> IsMicrosoftEmploee(string userUrl)
        {
            HttpClient client = new HttpClient();
            string html = await client.GetStringAsync(userUrl);

            return html.Contains("Microsoft Employee");
        }

        private static HtmlThread ParseHtml(string html)
        {
            var htmlThread = new HtmlThread();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // get the replies section (skipping original quesion and answered questions. Answered questions are anyway duplicated in the allReplies section).
            HtmlNode allReplies = doc.DocumentNode.Descendants("div").Where(x => HasCssId(x, "allReplies")).FirstOrDefault();
            if (allReplies == null)
            {
                return htmlThread;
            }

            // class="message " or class="message  answer"
            IEnumerable<HtmlNode> links = allReplies.Descendants("li").Where(x => HasCssClass(x, "message ") || HasCssClass(x, "message  answer"));

            foreach (HtmlNode message in links)
            {
                var reply = new HtmlReply();

                // is replies marked as answer
                reply.MarkedAsAnswer = message.Attributes["class"].Value == "message  answer";
                reply.Id = message.Attributes["id"].Value;

                // number of votes
                HtmlNode voteNumber = message.Descendants("div").Where(x => HasCssClass(x, "votenumber")).FirstOrDefault();
                if (voteNumber != null)
                {
                    int vote;
                    if (voteNumber.InnerText != null && int.TryParse(voteNumber.InnerText, out vote))
                    {
                        reply.VoteUps = vote;
                    }
                }

                // User Id
                HtmlNode userNode = message.Descendants("div").Where(x => HasCssClass(x, "unified-baseball-card-mini")).FirstOrDefault();
                if (userNode != null && userNode.Attributes.Contains("data-profile-userid"))
                {
                    string userId = userNode.Attributes["data-profile-userid"].Value;
                    if (userId != null)
                    {
                        reply.AuthorId = userId;
                    }
                }

                // Is Author Microsoft Employee
                HtmlNode affliationNode = message.Descendants("div").Where(x => HasCssClass(x, "profile-mini-affiliations")).FirstOrDefault();
                if (affliationNode != null)
                {
                    reply.IsAffiliatedToMicrosoft = affliationNode.InnerHtml.Contains("MSFT") || affliationNode.InnerHtml.Contains("Microsoft");
                }
                affliationNode = message.Descendants("abbr").Where(x => HasCssClass(x, "affil")).FirstOrDefault();
                if (affliationNode != null)
                {
                    reply.IsAffiliatedToMicrosoft = reply.IsAffiliatedToMicrosoft || affliationNode.InnerHtml.Contains("Microsoft");
                }

                htmlThread.Replies.Add(reply);
            }

            return htmlThread;
        }

        private static bool HasCssClass(HtmlNode node, string cssValue)
        {
            var cssAttribute = node.Attributes["class"];
            return cssAttribute != null && cssAttribute.Value == cssValue;
        }

        private static bool HasCssId(HtmlNode node, string cssValue)
        {
            var cssAttribute = node.Attributes["id"];
            return cssAttribute != null && cssAttribute.Value == cssValue;
        }

        private static Task<string> ReadHtml(string uri)
        {
            try
            {
                HttpClient client = new HttpClient();
                return client.GetStringAsync(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private static async Task<rss> ReadRss(string uri)
        {
            try
            {
                HttpClient client = new HttpClient();
                Stream rssFeed = await client.GetStreamAsync(uri);

                XmlSerializer serializer = new XmlSerializer(typeof(rss));
                rss rss = serializer.Deserialize(rssFeed) as rss;
                return rss;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
