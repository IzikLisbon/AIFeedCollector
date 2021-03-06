﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeed.AzureTableStorageEntities
{
    public class ForumThreadEntity : TableEntity
    {
        public string Id { get; set; }
        public string Path { get; set; }
        /// <summary>
        /// String is a list json-serialized ReplyEntity 
        /// </summary>
        public string Replies { get; set; }
        /// <summary>
        /// StackOverflow, MSDN, Twitter...
        /// </summary>
        public string Source { get; set; }
        public bool HasReplies { get; set; }
        public bool IsAnswerAccepted { get; set; }
        public string PostedOn { get; set; }
        public string LastUpdated { get; set; }
    }
}
