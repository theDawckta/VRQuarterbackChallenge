using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace VOKE.VokeApp.DataModel
{
	public class ScoreboardPosition
	{
		[XmlAttribute("OffsetX")]
		public string OffsetX { get; set; }

		[XmlAttribute("OffsetY")]
		public string OffsetY { get; set; }

		[XmlAttribute("OffsetZ")]
		public string OffsetZ { get; set; }

		[XmlText]
		public string Value { get; set; }
	}
}