using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIStoreCollection.MSDN.HtmlModel
{
    internal class HtmlThread
    {
        internal HtmlThread()
        {
            Replies = new List<HtmlReply>();
        }

        internal List<HtmlReply> Replies { get; set; } 
    }
}
