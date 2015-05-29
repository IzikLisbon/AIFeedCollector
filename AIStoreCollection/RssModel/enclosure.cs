using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AIStoreCollection
{
    public class enclosure
    {
        [XmlAttribute]
        public string url { get; set; }
        
        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        public int length { get; set; }
    }
}
