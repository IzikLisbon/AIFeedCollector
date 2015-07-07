using System.Collections.Generic;
using AIFeed.AzureTableStorageEntities;

namespace AIFeedStat.StatisticsCalculators
{
    public class ProcessUserScore
    {
        private IEnumerable<ForumThreadEntity> Threads { get; set; }
        private Dictionary<string, int> UsersScore { get; set; }
        public List<UserScore> UserScoreList { get; set; }

        public ProcessUserScore(IEnumerable<ForumThreadEntity> threads)
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
                    if (!reply.IsAuthorMicrosoftEmploee)
                    {
                        continue;
                    }

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

            this.UserScoreList = new List<UserScore>();
            foreach (var keyAndValue in UsersScore)
            {
                this.UserScoreList.Add(new UserScore { AuthorName = keyAndValue.Key, Score = keyAndValue.Value });
            }
        }
    }
}
