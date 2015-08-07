namespace AIRssCollection
{
    using System.Collections.Generic;
    using AIFeed.AzureTableStorageEntities;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AzureTableUtils
    {
        /// <summary>
        /// Copy from one Azure Table to anohter.
        /// </summary>
        /// <param name="oldConnectionString">Connection string to the older Azure table</param>
        /// <param name="cloudTable">CloudTable that points to the new table</param>
        /// <remarks>I have used this method one time, when my azure subscription ended and I needed to copy the table to a new Azure account.</remarks>
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

        public static CloudTable GetCloudTable(string connectionStrig, string tableName = "ForumThreadsSummery")
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionStrig);
            CloudTableClient tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);

            return table;
        }
    }
}
