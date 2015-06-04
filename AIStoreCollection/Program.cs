using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using AIStoreCollection;
using AIStoreCollection.HtmlModel;
using AIFeed.AzureTableStorageEntities;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AIRssCollection
{
    public class Program
    {
        static void Main(string[] args)
        {   
            JobHost host = new JobHost();
            host.Call(typeof(Program).GetMethod("UpdateForumData"));

            //ProcessUserScore processUserScore = new ProcessUserScore(threadsTable);
            //processUserScore.Process();
            //var a = processUserScore.UsersScore;
        }

        [NoAutomaticTrigger]
        public static void UpdateForumData(
            [Table("ForumThreadsSummery")] CloudTable cloudTable)
        {
            // Download the forum RSS and HTML and parse it. 
            List<ThreadInfo> rssAndHtml = DownloadAndParse.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            // Create an object module to save in Azure Table Storage.
            List<ForumThreadEntity> threadsTable = CreateTableStorageEntities(rssAndHtml);

            // Write the data to an Azure Table.
            foreach (ForumThreadEntity thread in threadsTable)
            {
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(thread);
                TableResult res = cloudTable.Execute(insertOrReplaceOperation);
                System.Console.WriteLine(res.HttpStatusCode);
            }
        }

        private static List<ForumThreadEntity> CreateTableStorageEntities(List<ThreadInfo> threads)
        {
            List<ForumThreadEntity> threadsTable = new List<ForumThreadEntity>();

            foreach (ThreadInfo thread in threads)
            {
                ForumThreadEntity threadEntity = new ForumThreadEntity();
                threadEntity.PartitionKey = "1";
                threadEntity.RowKey = thread.Rss.channel.id;
                threadEntity.Id = thread.Rss.channel.id;
                threadEntity.HasReplies = thread.htmlModel.Replies.Count > 0;
                threadEntity.IsAnswerAccepted = thread.htmlModel.Replies.Any((reply) => reply.MarkedAsAnswer);
                threadEntity.LastUpdated = thread.Rss.channel.lastBuildDate;
                threadEntity.PostedOn = thread.Rss.channel.items.First().pubDate;
                threadEntity.Path = thread.Rss.channel.link.href;

                var replies = new List<ReplyEntity>();
                foreach (HtmlReply htmlReply in thread.htmlModel.Replies)
                {
                    ReplyEntity reply = new ReplyEntity();
                    reply.AuthorId = htmlReply.AuthorId;
                    reply.IsAuthorMicrosoftEmploee = htmlReply.IsAffiliatedToMicrosoft;
                    reply.MarkedAsAnswer = htmlReply.MarkedAsAnswer;
                    reply.VoteUps = htmlReply.VoteUps;
                    rssItem item = thread.Rss.channel.items.FirstOrDefault((rssItem) =>
                    {
                        return rssItem.guid.Contains(htmlReply.Id);
                    });
                    if (item != null)
                    {
                        reply.AuthorName = item.author.name;
                        reply.IsAuthorMicrosoftEmploee = reply.IsAuthorMicrosoftEmploee
                            || item.author.name.IndexOf("MSFT", StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    replies.Add(reply);
                }

                threadEntity.Replies = ReplyEntitiesJsonSerializer.Serialize(replies);
                threadsTable.Add(threadEntity);
            }

            return threadsTable;
        }
    }
}
