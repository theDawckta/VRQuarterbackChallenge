using System;
using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
	public class CameraLayoutUrl
	{
		[XmlAttribute]
		public string Name { get; set; }
		[XmlAttribute]
		public string ID { get; set; }
		[XmlText]
		public string Url { get; set;}
	}
}
