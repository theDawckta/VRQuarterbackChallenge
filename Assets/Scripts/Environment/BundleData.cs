using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

public class BundleData
{
    [XmlAttribute]
    public string ID { get; set; }

    [XmlAttribute]
    public string Url { get; set; }

    [XmlAttribute]
    public string Path { get; set; }

    [XmlAttribute]
    public int Version { get; set; }

    [XmlElement]
    public SuffixData Suffix { get; set; }
}