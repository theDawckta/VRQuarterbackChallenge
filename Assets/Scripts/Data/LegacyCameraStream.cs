using System;
using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
    public class LegacyCameraStream : CameraStream
    {
        [XmlAttribute("PreferredCamera")]
        public bool LegacyPreferredCamera
        {
            get { return IsPreferred; }
            set { IsPreferred = value; }
        }

        [XmlText]
        public string LegacyUrl
        {
            get { return Url; }
            set { Url = value; }
        }
    }
}
