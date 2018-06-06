using System;
using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
	public class Resource
	{
		[XmlAttribute("ID")]
		public string ID { get; set; }

		[XmlText]
		public string Value { get; set; }
	}
}