using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
    public class AuthenticationConfigurationElement
    {
        [XmlText]
        public bool Value { get; set; }
    }
}
