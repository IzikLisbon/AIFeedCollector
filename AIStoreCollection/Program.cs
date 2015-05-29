using System.Collections.Generic;
using System.Linq;
using AIStoreCollection;
using AIStoreCollection.HtmlModel;
using AIStoreCollection.Processor;

namespace AIRssCollection
{
    class Program
    {
        static void Main(string[] args)
        {   
            //JobHost host = new JobHost();
            //host.RunAndBlock();
            
            List<ThreadInfo> threads = DownloadAndParse.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            List<ForumThreadEntity> threadsTable = new List<ForumThreadEntity>();

            UpdateThreadsTable(threads, threadsTable);

            ProcessUserScore processUserScore = new ProcessUserScore(threadsTable);
            processUserScore.Process();
            var a = processUserScore.UsersScore;
        }

        private static void UpdateThreadsTable(List<ThreadInfo> threads, List<ForumThreadEntity> threadsTable)
        {
            foreach (ThreadInfo thread in threads)
            {
                ForumThreadEntity threadEntity = new ForumThreadEntity();
                threadEntity.Id = thread.Rss.channel.id;
                threadEntity.HasReplies = thread.htmlModel.Replies.Count > 0;
                threadEntity.IsAnswerAccepted = thread.htmlModel.Replies.Any((reply) => reply.MarkedAsAnswer);
                threadEntity.LastUpdated = thread.Rss.channel.lastBuildDate;
                threadEntity.PostedOn = thread.Rss.channel.items.First().pubDate;
                threadEntity.Path = thread.Rss.channel.link.href;

                foreach (HtmlReply htmlReply in thread.htmlModel.Replies)
                {
                    ReplyEntity reply = new ReplyEntity();
                    reply.AuthorId = htmlReply.AuthorId;
                    reply.IsAuthorMicrosoftEmploee = htmlReply.IsAuthorMicrosoftEmploee;
                    reply.MarkedAsAnswer = htmlReply.MarkedAsAnswer;
                    reply.VoteUps = htmlReply.VoteUps;
                    rssItem item = thread.Rss.channel.items.FirstOrDefault((rssItem) =>
                    {
                        return rssItem.guid.Contains(htmlReply.Id);
                    });
                    if (item != null)
                    {
                        reply.AuthorName = item.author.name;
                    }
                    threadEntity.Replies.Add(reply);
                }

                ForumThreadEntity existingThreadEntity = threadsTable.FirstOrDefault((t) => t.Id == threadEntity.Id);
                if (existingThreadEntity != null)
                {
                    threadsTable.Remove(existingThreadEntity);
                }

                threadsTable.Add(threadEntity);
            }
        }
    }
}
