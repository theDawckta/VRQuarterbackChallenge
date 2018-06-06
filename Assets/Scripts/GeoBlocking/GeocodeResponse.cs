using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace VOKE.VokeApp.GeoDataModel
{
	[XmlRoot ("GeocodeResponse")]
	public class GeocodeResponse
	{
		public GeocodeResponse ()
		{
			ResultList = new List<AddressComponent> ();
		}

		[XmlArray ("result")]
		[XmlArrayItem ("address_component")]
		public List<AddressComponent> ResultList
		{ get; set; }
	}
}
