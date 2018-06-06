using System;
using System.Xml.Serialization;

public abstract class EnvironmentBundleReference
{
    [XmlAttribute]
    public string BundleID { get; set; }

    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string Url { get; set; }
}