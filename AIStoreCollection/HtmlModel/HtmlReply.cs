﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIStoreCollection.HtmlModel
{
    public class HtmlReply
    {
        public string AuthorId { get; set; }

        public bool IsAuthorMicrosoftEmploee { get; set; } 

        public int VoteUps { get; set; }
        public bool MarkedAsAnswer { get; set; }
    }
}
