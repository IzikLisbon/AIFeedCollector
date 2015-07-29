using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIStoreCollection.MSDN.HtmlModel;

namespace AIStoreCollection.MSDN
{
    internal class ThreadInfo
    {
        /// <summary>
        /// The thread item in the root RSS.
        /// </summary>
        internal rssItem ThreadItemInRootRss { get; set; }

        /// <summary>
        /// The rss thread.   
        /// </summary>
        internal rss Rss { get; set; }

        /// <summary>
        /// the html of the thread.
        /// </summary>
        internal string html { get; set; }

        /// <summary>
        /// Parsed html
        /// </summary>
        internal HtmlThread htmlModel { get; set; }
    }
}
