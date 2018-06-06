using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("EnvironmentConfiguration")]
public class EnvironmentConfiguration
{
    public EnvironmentConfiguration()
    {
        Environments = new List<EnvironmentData>();
        Bundles = new List<BundleData>();
    }

    [XmlArray("Environments")]
    [XmlArrayItem("Environment")]
    public List<EnvironmentData> Environments { get; private set; }

//	[XmlElement]
//	public ScoreboardPosition ScoreboardPosition { get; set; }

    [XmlArray("Bundles")]
    [XmlArrayItem("Bundle")]
    public List<BundleData> Bundles { get; private set; }

    [XmlElement]
    public BaseConfiguration Base { get; set; }

//	public ScoreboardPosition GetScoreboardPosition()
//	{
//		return ScoreboardPosition;
//	}
}