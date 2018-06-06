using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class EnvironmentAssetData : EnvironmentBundleReference
{
    [XmlAttribute]
    public string Slot { get; set; }

	[XmlAttribute]
	public string MediaUrl { get; set; }

	[XmlAttribute]
	public string MediaScreenRotation { get; set; }

	[XmlAttribute]
	public string Position { get; set; }

	[XmlAttribute]
	public string Rotation { get; set; }

	[XmlAttribute]
	public string Scale { get; set; }
}