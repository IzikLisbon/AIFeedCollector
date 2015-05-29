using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AIStoreCollection.HtmlModel;
using CsQuery;
using HtmlAgilityPack;

namespace AIStoreCollection.Processor
{
    public class UserScore
    {
        private ThreadInfo thread;
        
        public UserScore(ThreadInfo threadInfo)
        {   
            this.thread = threadInfo;
        }

        public void Process()
        {
            foreach (HtmlReply reply in this.thread.htmlModel.Replies)
            {
                if (reply.)
            }
        }
    }
}
