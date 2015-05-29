using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net.Http;
using System.Reflection;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Serialization;
using AIStoreCollection;
using AIStoreCollection.RssModel;
using AIStoreCollection.HtmlModel;
using HtmlAgilityPack;

namespace AIRssCollection
{
    class Program
    {
        static void Main(string[] args)
        {   
            //JobHost host = new JobHost();
            //host.RunAndBlock();
            
            List<ThreadInfo> threads = DownloadAndParse.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            List<ThreadSummery> threadSummeryList = new List<ThreadSummery>();

            foreach (ThreadInfo thread in threads)
            {
                ProcessUserScore processUserScore = new ProcessUserScore(thread);
            }
        }
    }
}
