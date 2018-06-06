using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
    public class CameraStream
    {
        public CameraStream()
        {
            Links = new List<CameraLink>();
        }

        [XmlAttribute]
        public bool IsPreferred { get; set; }

        [XmlAttribute]
        public bool IsVRcast { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string ID { get; set; }

        [XmlAttribute]
        public string Label { get; set; }

        [XmlAttribute]
        public string Url { get; set; }

		[XmlElement]
		public string HEVC { get; set;}

		[XmlElement]
		public string H264 { get; set;}

        [XmlAttribute]
        public float? OffsetX { get; set; }

        [XmlAttribute]
        public float? OffsetY { get; set; }

        [XmlAttribute]
        public float? OffsetZ { get; set; }

        [XmlAttribute]
        public float? Rotation { get; set; }

        [XmlArray("Links")]
        [XmlArrayItem("Link")]
        public List<CameraLink> Links { get; private set; }
    }
}