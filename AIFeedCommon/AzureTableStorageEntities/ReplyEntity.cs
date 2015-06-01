using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeed.AzureTableStorageEntities
{
    [DataContract]
    public class ReplyEntity
    {
        [DataMember]
        public string AuthorId { get; set; }
        [DataMember]
        public string AuthorName { get; set; }
        [DataMember]
        public bool IsAuthorMicrosoftEmploee { get; set; }
        [DataMember]
        public int VoteUps { get; set; }
        [DataMember]
        public bool MarkedAsAnswer { get; set; }
    }
}
