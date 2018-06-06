using System;
using System.Xml.Serialization;
using System.Collections.Generic;

public class EnvironmentData
{
    [XmlAttribute("ID")]
    public string ID { get; set; }

    public EnvironmentData()
    {
        Assets = new List<EnvironmentAssetData>();
    }

    [XmlElement]
    public EnvironmentModelData Model { get; set; }

    [XmlArray("Assets")]
    [XmlArrayItem("Asset")]
    public List<EnvironmentAssetData> Assets { get; private set; }
}