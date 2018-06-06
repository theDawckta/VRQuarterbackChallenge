using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace VOKE.VokeApp.DataModel
{
	[XmlRoot("VokeAppConfiguration")]
	public class VokeAppConfiguration
	{
		public VokeAppConfiguration()
		{
			ContentsDefinition = new List<Content>();
			Resources = new List<Resource>();
		}

		[XmlElement("DemoMode")]
        public bool IsDemoMode { get; set; }

        [XmlElement]
        public string Featured { get; set; }

		[XmlElement]
		public string TileStyle { get; set; }

        [XmlElement]
        public string EnvironmentID { get; set; }

		[XmlElement]
		public bool IsPlayoffs { get; set; }

		[XmlElement]
		public string DefaultTab { get; set; }

        [XmlElement]
        public string BackgroundAudio { get; set; }

        [XmlArray("ContentsDefinition")]
        [XmlArrayItem("Content")]
        public List<Content> ContentsDefinition { get; set; }

		[XmlArray("Resources")]
		[XmlArrayItem("Resource")]
		public List<Resource> Resources { get; set; }

		[XmlArray("DeviceList")]
		[XmlArrayItem("Device")]
		public List<Device> DeviceList { get; set; }

		public string GetResourceValueByID(string id)
		{
			Resource r = Resources.FirstOrDefault(_=>_.ID == id);
			return r != null ? r.Value : null;
		}

        public Content GetContent()
        {
            var content = new Content();
            content.Featured = Featured;
            content.EnvironmentID = EnvironmentID;
			content.TileStyle = TileStyle;
            content.Contents.AddRange(ContentsDefinition);
            return content;
        }

        public string GetURLForTeamInfos(string sportType, string leagueType)
        {
            string url = "";
            switch (sportType)
            {
                case "FTB":
                    if (leagueType == "NCAA")
                        url = GetResourceValueByID("NCAAFootballTeamsUrl");
                    else if (leagueType == "NFL")
                        url = GetResourceValueByID("NFLTeamsUrl");
                    break;

                case "BKB":
                    if (leagueType == "NCAA")
                        url = GetResourceValueByID("NCAABasketballTeamsUrl");
                    else if (leagueType == "NBA")
                        url = GetResourceValueByID("NBATeamsUrl");
                    break;

                case "BSB":
                    if (leagueType == "MLB")
                        url = GetResourceValueByID("MLBTeamsUrl");
                    break;           
            }

            return url;
        }

        public string GetURLForLeagueLogo(string leagueType)
        {
            string idValue = leagueType + "LogoUrl";

            return GetResourceValueByID(idValue);
        }

        public string GetURLForPlayerInfos(string sportType, string leagueType)
        {
            string url = "";

            switch (sportType)
            {
                case "FTB":
                    break;

                case "BKB":
                    break;

                case "BSB":
                    if (leagueType == "MLB")
                        url = GetResourceValueByID("MLBHeadshotsURL");
                    break;
                
                case "GLF":
                    url = GetResourceValueByID("PGAPlayerUrlFormat");
                    break;
            }

            return url;

        }
    }
}