using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
using AIStoreCollection.HtmlModel;
using AIStoreCollection.AzureTableStorageEntities;
using CsQuery;
using HtmlAgilityPack;

namespace AIStoreCollection.Processor
{
    public class ProcessUserScore
    {
        private List<ForumThreadEntity> Threads { get; set; }
        public Dictionary<string, int> UsersScore { get; set; }

        public ProcessUserScore(List<ForumThreadEntity> threads)
        {
            this.Threads = threads;
            this.UsersScore = new Dictionary<string, int>();
        }

        public void Process()
        {
            foreach (ForumThreadEntity forumThreadEntity in this.Threads)
            {
                List<ReplyEntity> replies = ReplyEntitiesJsonSerializer.Deserialize(forumThreadEntity.Replies);
                foreach (ReplyEntity reply in replies)
                {   
                    //if (!replies.IsAffiliatedToMicrosoft)
                    //{
                    //    continue;
                    //}

                    if (!this.UsersScore.ContainsKey(reply.AuthorName))
                    {
                        this.UsersScore[reply.AuthorName] = 0;
                    }

                    int authorScore = this.UsersScore[reply.AuthorName];

                    authorScore += 1;
                    authorScore += reply.VoteUps;
                    if (reply.MarkedAsAnswer)
                    {
                        authorScore += 5;
                    }

                    this.UsersScore[reply.AuthorName] = authorScore;
                }
            }
        }
    }
}
