using System;
using System.Xml.Serialization;

public class BaseConfiguration
{
    [XmlAttribute]
    public string Url { get; set; }
}