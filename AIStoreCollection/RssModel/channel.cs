using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AIStoreCollection.RssModel;

namespace AIStoreCollection
{
    public class channel
    {
        public string title { get; set; }
        public string description { get; set; }
        public string copyright { get; set; }
        public string lastBuildDate { get; set; }
        [XmlElement("id", Namespace = "http://www.w3.org/2005/Atom")]
        public string id { get; set; }
        [XmlElement("item")]
        public List<rssItem> items;
        [XmlElement("link", Namespace = "http://www.w3.org/2005/Atom")]
        public link link;

    }
}
