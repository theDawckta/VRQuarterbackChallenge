using System;
using System.Xml.Serialization;

public class AuthorizationConfiguration
{
    [XmlAttribute]
    public string Hash { get; set; }
}