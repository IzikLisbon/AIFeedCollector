using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIStoreCollection.HtmlModel
{
    public class HtmlThread
    {
        public HtmlThread()
        {
            Replies = new List<HtmlReply>();
        }

        public List<HtmlReply> Replies { get; set; } 
    }
}
