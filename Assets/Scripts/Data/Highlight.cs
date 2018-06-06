using System;

using System.Collections.Generic;
using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
	[System.Serializable]
	public class Highlight
	{
		[XmlAttribute]
		public string Timecode { get; set; }
		[XmlAttribute]
		public String Type { get; set; }
	}
}