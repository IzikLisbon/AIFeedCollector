﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using AIStoreCollection;
using AIStoreCollection.HtmlModel;
using AIFeed.AzureTableStorageEntities;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using Microsoft.WindowsAzure.Storage;

namespace AIRssCollection
{
    public class FeedsCollector
    {
        /// <summary>
        /// Copy from one Azure Table to anohter
        /// </summary>
        /// <param name="oldConnectionString">Connection string to the older Azure table</param>
        /// <param name="cloudTable">CloudTable that points to the new table</param>
        public static void CopyToAnotherCloudTable(string oldConnectionString, CloudTable cloudTable)
        {
            // Open storage account using credentials from .cscfg file.
            // string webJobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=airsscollector;AccountKey=n5OFL64RLztYbW0wx45jXfblYxNt414trgOdGVjjMVrcFSn8g/cpyZoDqPez2/qBpV9aLgBXkc2Z7a3sEHSFcg==";
            CloudStorageAccount account = CloudStorageAccount.Parse(oldConnectionString);

            // Create the table client. 
            CloudTableClient tableClient = account.CreateCloudTableClient();

            // Create the table if it doesn't exist. 
            CloudTable oldTable = tableClient.GetTableReference("ForumThreadsSummery");


            var query = new TableQuery<ForumThreadEntity>();
            IEnumerable<ForumThreadEntity> list = oldTable.ExecuteQuery(query);

            foreach (ForumThreadEntity forumThread in list)
            {
                ForumThreadEntity newForumThreadEntity = new ForumThreadEntity();
                newForumThreadEntity.PartitionKey = forumThread.PartitionKey;
                newForumThreadEntity.RowKey = forumThread.RowKey;
                newForumThreadEntity.HasReplies = forumThread.HasReplies;
                newForumThreadEntity.Id = forumThread.Id;
                newForumThreadEntity.IsAnswerAccepted = forumThread.IsAnswerAccepted;
                newForumThreadEntity.LastUpdated = forumThread.LastUpdated;
                newForumThreadEntity.Path = forumThread.Path;
                newForumThreadEntity.PostedOn = forumThread.PostedOn;
                newForumThreadEntity.Replies = forumThread.Replies;

                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(newForumThreadEntity);
                TableResult res = cloudTable.Execute(insertOrReplaceOperation);
                System.Console.WriteLine(res.HttpStatusCode);
            }
        }

        /// <summary>
        /// Going over all the collected AI feeds and update thier statistics (by parsing the HTML) 
        /// </summary>
        /// <param name="cloudTable">The cloud table to query and update the Azure Table</param>
        public static void IterateOverExistingFeeds(CloudTable cloudTable)
        {
            // Uncomment to update a specific thread 
            //var query = TableOperation.Retrieve<ForumThreadEntity>("1", "45224325-0593-40cf-a4b6-5f4408bc93a3");
            //IEnumerable<ForumThreadEntity> list = new List<ForumThreadEntity>() { cloudTable.Execute(query).Result as ForumThreadEntity };

            // If previous 2 lines are uncomment - comment those 2 lines instead
            var query = new TableQuery<ForumThreadEntity>();
            IEnumerable<ForumThreadEntity> list = cloudTable.ExecuteQuery(query);

            foreach (ForumThreadEntity forumThread in list)
            {
                HtmlThread htmlThread = DownloadAndParse.ParseHtmlOnly(forumThread.Path);
                forumThread.HasReplies = htmlThread.Replies.Count > 0;
                forumThread.IsAnswerAccepted = htmlThread.Replies.Any((reply) => reply.MarkedAsAnswer);
                forumThread.Replies = RepliesToJson(htmlThread.Replies);

                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(forumThread);
                TableResult res = cloudTable.Execute(insertOrReplaceOperation);
                System.Console.WriteLine(res.HttpStatusCode);
            }
        }

        /// <summary>
        /// Iterates over latest RSS feeds. 
        /// </summary>
        /// <param name="cloudTable">The cloud table to query and update the Azure Table</param>
        public static void IterateOverRss(CloudTable cloudTable)
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

        private static string RepliesToJson(IEnumerable<HtmlReply> htmlReplies)
        {
            var replies = new List<ReplyEntity>();
            foreach (HtmlReply htmlReply in htmlReplies)
            {
                ReplyEntity reply = new ReplyEntity();
                reply.Id = htmlReply.Id;
                reply.AuthorId = htmlReply.AuthorId;
                reply.AuthorName = htmlReply.AuthorName;
                reply.IsAuthorMicrosoftEmploee =
                    (htmlReply.Affiliation != null &&
                    (htmlReply.Affiliation.Contains("MSFT") ||
                    htmlReply.Affiliation.Contains("Microsoft"))) ||
                    htmlReply.AuthorName.Contains("MSFT");
                reply.MarkedAsAnswer = htmlReply.MarkedAsAnswer;
                reply.VoteUps = htmlReply.VoteUps;

                replies.Add(reply);
            }

            return ReplyEntitiesJsonSerializer.Serialize(replies);
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
                threadEntity.Replies = RepliesToJson(thread.htmlModel.Replies); ;

                threadsTable.Add(threadEntity);
            }

            return threadsTable;
        }
    }
}
