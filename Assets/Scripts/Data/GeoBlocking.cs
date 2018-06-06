using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
	public class GeoBlocking
	{
		public GeoBlocking()
		{
      IsLocalBlocked = true;
    }

		[XmlAttribute("LeagueID")]
		public string LeagueID { get; set; }

    [XmlAttribute("IsLocalBlocked")]
		public bool IsLocalBlocked { get; set; }

		[XmlText]
		public string Value {get; set;}
	}
}
