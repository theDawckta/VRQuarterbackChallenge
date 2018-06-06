using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace VOKE.VokeApp.GeoDataModel
{
	public class AddressComponent
	{
		[XmlElement("type")]
		public string AddressType
		{ get; set; }

		[XmlElement("long_name")]
		public string LongName
		{ get; set; }

		[XmlElement("short_name")]
		public string ShortName
		{ get; set; }
	}
}