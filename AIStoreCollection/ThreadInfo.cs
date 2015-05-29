using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIStoreCollection.HtmlModel;

namespace AIStoreCollection
{
    public class ThreadInfo
    {
        /// <summary>
        /// The thread item in the root RSS.
        /// </summary>
        public rssItem ThreadItemInRootRss { get; set; }

        /// <summary>
        /// The rss thread.   
        /// </summary>
        public rss Rss { get; set; }

        /// <summary>
        /// the html of the thread.
        /// </summary>
        public string html { get; set; }

        /// <summary>
        /// Parsed html
        /// </summary>
        public HtmlThread htmlModel { get; set; }
    }
}
