using System;
using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
    public class CameraLink
    {
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public string Coordinates { get; set; }
    }
}
