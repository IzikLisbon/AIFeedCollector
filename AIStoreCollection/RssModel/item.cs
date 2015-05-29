using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AIStoreCollection
{
    public class rssItem
    {
        public string guid { get; set; }
        public string link { get; set; }
        [XmlElement("author", Namespace = "http://www.w3.org/2005/Atom")]
        public author author { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string pubDate { get; set; }
        [XmlElement("updated", Namespace = "http://www.w3.org/2005/Atom")]
        public string updated { get; set; }
        public enclosure enclosure {get; set;}
        
    }
}
