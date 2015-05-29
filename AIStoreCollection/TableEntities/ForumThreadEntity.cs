using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIStoreCollection
{
    public class ForumThreadEntity : TableEntity
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public List<ReplyEntity> Replies { get; set; }
        public bool HasReplies { get; set; }
        public bool IsAnswerAccepted { get; set; }
        public string PostedOn { get; set; }
        public string LastUpdated { get; set; }

        public ForumThreadEntity()
        {
            this.Replies = new List<ReplyEntity>();
        }
    }
}
