using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AIFeed.AzureTableStorageEntities;
using AIFeedCommon.StackOverflowCollector.Model;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AIFeedCommon.StackOverflowCollector
{
    public class StackOverflowCollector
    {
        private static readonly DateTime BeginningOfTime = new DateTime(1970, 1, 1, 0, 0, 0, 0,DateTimeKind.Utc);

        public static void IterateOverQuestionsWithApplicationInsightsTag(CloudTable cloudTable)
        {
            string questionsByTagUrl = @"https://api.stackexchange.com/2.2/questions?fromdate=1433116800&order=desc&sort=activity&tagged=ms-application-insights&site=stackoverflow";
            Root<Question> rootQuestions;

            try
            {
                MyWebClient client = new MyWebClient();
                string aiQuestions = client.DownloadString(new Uri(questionsByTagUrl));

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Error = (s, errorEventArgs) => Console.WriteLine(errorEventArgs.ErrorContext.Error);
                rootQuestions = JsonConvert.DeserializeObject<Root<Question>>(aiQuestions, jsonSettings);
                
                if (rootQuestions == null)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            List<ForumThreadEntity> threads = CreateTableStorageEntities(rootQuestions.items).ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (ForumThreadEntity thread in threads)
            {
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(thread);
                TableResult res = cloudTable.Execute(insertOrReplaceOperation);
                System.Console.WriteLine(res.HttpStatusCode);
            }
        }

        private static async Task<List<ForumThreadEntity>> CreateTableStorageEntities(List<Question> questions)
        {
            List<ForumThreadEntity> threadsTable = new List<ForumThreadEntity>();

            foreach (Question stackOverflowQuestion in questions)
            {
                ForumThreadEntity threadEntity = new ForumThreadEntity();
                threadEntity.PartitionKey = "1";
                threadEntity.RowKey = stackOverflowQuestion.question_id.ToString();
                threadEntity.Id = threadEntity.RowKey;
                threadEntity.HasReplies = stackOverflowQuestion.answer_count > 0;
                threadEntity.IsAnswerAccepted = stackOverflowQuestion.is_answered;
                threadEntity.LastUpdated = UnixTimeStampToDateTime(stackOverflowQuestion.last_activity_date);
                threadEntity.PostedOn = UnixTimeStampToDateTime(stackOverflowQuestion.creation_date);
                threadEntity.Path = stackOverflowQuestion.link;
                threadEntity.Replies = await RepliesToJson(stackOverflowQuestion.question_id);
                threadEntity.Source = "StackOverflow";

                threadsTable.Add(threadEntity);
            }

            return threadsTable;
        }

        private static async Task<string> RepliesToJson(int questionId)
        {
            string answersUrl = @"https://api.stackexchange.com/2.2/questions/{0}/answers?order=desc&sort=activity&site=stackoverflow";
            answersUrl = string.Format(CultureInfo.InvariantCulture, answersUrl, questionId);
            Root<Answer> rootAnswers;

            try
            {
                MyWebClient client = new MyWebClient();
                string aiAnswers = client.DownloadString(new Uri(answersUrl));

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Error = (s, errorEventArgs) => Console.WriteLine(errorEventArgs.ErrorContext.Error);
                rootAnswers = JsonConvert.DeserializeObject<Root<Answer>>(aiAnswers, jsonSettings);

                if (rootAnswers == null)
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }

            List<ReplyEntity> replies = new List<ReplyEntity>();

            foreach (Answer stackOverflowAnswer in rootAnswers.items)
            {
                ReplyEntity reply = new ReplyEntity();
                reply.Id = stackOverflowAnswer.answer_id.ToString();
                reply.AuthorId = stackOverflowAnswer.owner.user_id.ToString();
                reply.AuthorName = stackOverflowAnswer.owner.display_name;
                reply.IsAuthorMicrosoftEmploee = false;
                reply.MarkedAsAnswer = stackOverflowAnswer.is_accepted;
                reply.VoteUps = stackOverflowAnswer.score;

                replies.Add(reply);
            }

            return ReplyEntitiesJsonSerializer.Serialize(replies);
        }

        public static string UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime.ToString("ddd, dd MMM yyyy hh:mm:ss Z");
        }
    }
}
