using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIStoreCollection.HtmlModel
{
    public class HtmlReply
    {
        public string Id { get; set; }

        public string AuthorId { get; set; }

        public string AuthorName { get; set; }

        public string Affiliation { get; set; } 

        public int VoteUps { get; set; }

        public bool MarkedAsAnswer { get; set; }
    }
}
