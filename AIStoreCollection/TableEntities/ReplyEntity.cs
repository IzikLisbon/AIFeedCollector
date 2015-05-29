using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIStoreCollection
{
    public class ReplyEntity : TableEntity
    {
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public bool IsAuthorMicrosoftEmploee { get; set; }
        public int VoteUps { get; set; }
        public bool MarkedAsAnswer { get; set; }
    }
}
