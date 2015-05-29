using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AIStoreCollection.HtmlModel;
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
                foreach (ReplyEntity reply in forumThreadEntity.Replies)
                {
                    //if (!reply.IsAuthorMicrosoftEmploee)
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
