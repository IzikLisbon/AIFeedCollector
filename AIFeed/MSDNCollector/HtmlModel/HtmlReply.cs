using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIStoreCollection.MSDN.HtmlModel
{
    internal class HtmlReply
    {
        internal string Id { get; set; }

        internal string AuthorId { get; set; }

        internal string AuthorName { get; set; }

        internal string Affiliation { get; set; }

        internal int VoteUps { get; set; }

        internal bool MarkedAsAnswer { get; set; }
    }
}
