using System;
using System.Xml.Serialization;

public class SuffixData
{
    [XmlAttribute]
    public string Mobile { get; set; }

    [XmlAttribute]
    public string Standalone { get; set; }
}