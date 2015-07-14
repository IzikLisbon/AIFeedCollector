using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            rss aiRss = await ReadRss("https://social.msdn.microsoft.com/Forums/en-US/ApplicationInsights/threads?outputAs=rss").ConfigureAwait(false);
            List<Task> tasks = new List<Task>();

            foreach (rssItem item in aiRss.channel.items)
            {
                int guidIndex = item.link.IndexOf("en-US/") + "en-US/".Length;
                string id = item.link.Substring(guidIndex, guidLength);
                string uri = string.Format("https://social.msdn.microsoft.com/Forums/en-US/ApplicationInsights/thread/{0}?outputAs=rss", id);

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

        /// <summary>
        /// Downlaod the HTML from the url and parse it into an HtmlThread object.
        /// </summary>
        public static HtmlThread ParseHtmlOnly(string url)
        {
            string html = ReadHtml(url).GetAwaiter().GetResult();
            HtmlThread htmlThread = ParseHtml(html);

            return htmlThread;
        }

        private static HtmlThread ParseHtml(string html)
        {
            var htmlThread = new HtmlThread();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // get the replies section (skipping original quesion and answered questions. Answered questions are anyway duplicated in the allReplies section).
            HtmlNode allReplies = doc.DocumentNode.Descendants("div").Where(x => HasIdAttribute(x, "allReplies")).FirstOrDefault();
            if (allReplies == null)
            {
                // If there is only one reply and it is answered, then it will not be under the All Replies section and under the Answered secion but only under the Answer section.
                allReplies = doc.DocumentNode.Descendants("div").Where(x => HasIdAttribute(x, "answers")).FirstOrDefault();

                if (allReplies == null)
                {
                    return htmlThread;
                }
            }

            // class="message " or class="message  answer"
            IEnumerable<HtmlNode> links = allReplies.Descendants("li").Where(li => HasCssClass(li, "message ") || HasCssClass(li, "message  answer") || HasCssClass(li, "message  propose"));

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

                // User Name
                reply.AuthorName = ReadAuthorName(message);

                // Is Author Microsoft Employee
                HtmlNode affliationNode = message.Descendants("div").Where(x => HasCssClass(x, "profile-mini-affiliations")).FirstOrDefault();
                if (affliationNode != null)
                {
                    reply.Affiliation = affliationNode.InnerHtml;
                }

                affliationNode = message.Descendants("abbr").Where(x => HasCssClass(x, "affil")).FirstOrDefault();
                if (affliationNode != null && !string.IsNullOrWhiteSpace(affliationNode.InnerHtml))
                {
                    reply.Affiliation = affliationNode.InnerHtml;
                }

                if (reply.Affiliation == null)
                {
                    HttpClient client = new HttpClient();
                    try
                    {
                        string authorPaegHtml = client.GetStringAsync("https://social.msdn.microsoft.com/profile/" + WebUtility.UrlEncode(reply.AuthorName)).GetAwaiter().GetResult();
                        if (authorPaegHtml.Contains("Microsoft Employee"))
                        {
                            reply.Affiliation = "Microsoft Employee";
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        // ignore http errors.
                    }
                    catch (WebException e)
                    {
                        // ignore http errors.
                    }
                }

                htmlThread.Replies.Add(reply);
            }

            return htmlThread;
        }

        private static string ReadAuthorName(HtmlNode message)
        {
            string authorName = null;

            HtmlNode userSignature = message.Descendants("hr").Where(hr => HasCssClass(hr, "sig")).FirstOrDefault();
            if (userSignature != null && !string.IsNullOrWhiteSpace(userSignature.InnerHtml))
            {
                authorName = userSignature.InnerHtml;
            }

            HtmlNode authorLink = message.Descendants("div").Where(div => HasCssClass(div, "unified-baseball-card-mini")).FirstOrDefault();
            if (authorLink != null && authorName == null)
            {
                HtmlAttribute dataProfileUsercardCustomlinkAttribute = authorLink.Attributes.Where(attr => attr.Name == "data-profile-usercard-customlink").FirstOrDefault();
                if (dataProfileUsercardCustomlinkAttribute != null && dataProfileUsercardCustomlinkAttribute.Value != null)
                {
                    int userNameStartIndex = dataProfileUsercardCustomlinkAttribute.Value.IndexOf("threads?user=");
                    if (userNameStartIndex != -1)
                    {
                        userNameStartIndex += "threads?user=".Length;
                        int userNameEndIndex = dataProfileUsercardCustomlinkAttribute.Value.IndexOf('"', userNameStartIndex);
                        if (userNameEndIndex != -1)
                        {
                            authorName = dataProfileUsercardCustomlinkAttribute.Value.Substring(userNameStartIndex, userNameEndIndex - userNameStartIndex);
                        }
                    }
                }
            }

            if (authorName != null)
            {
                authorName = WebUtility.UrlDecode(authorName);
            }

            return authorName;
        }

        private static bool HasCssClass(HtmlNode node, string cssValue)
        {
            var cssAttribute = node.Attributes["class"];
            return cssAttribute != null && cssAttribute.Value == cssValue;
        }

        private static bool HasIdAttribute(HtmlNode node, string idValue)
        {
            var idAttribute = node.Attributes["id"];
            return idAttribute != null && idAttribute.Value == idValue;
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
                Stream rssFeed = await client.GetStreamAsync(uri).ConfigureAwait(false);

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
