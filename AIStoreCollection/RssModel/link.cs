using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AIStoreCollection.RssModel
{
    public class link
    {
        [XmlAttribute]
        public string href { get; set; }
    }
}
