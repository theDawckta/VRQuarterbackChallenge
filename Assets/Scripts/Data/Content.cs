using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace VOKE.VokeApp.DataModel
{
    public class Content
    {
        public Content()
        {
            Cameras = new List<CameraStream>();
            Contents = new List<Content>();
            Tags = new List<string>();
        }

        [XmlElement]
        public string ContentType { get; set; }

        [XmlAttribute]
        public string ID { get; set; }

        /// <summary>
        /// Define if the content is geo blocked or not.
        /// </summary>
        /// <value LeagueID="MLB">ARI</value>
        [XmlElement]
        public GeoBlocking IsGeoBlocked { get; set; }

        /// <summary>
        /// Define an audioclip that should be played in this channel
        /// </summary>
        /// <value>Any string - must match audio clip in AudioController or none will be played</value>
        [XmlElement]
        public string BackgroundAudio { get; set; }

        /// <summary>
        /// Define a location of the background image displayed on the tile
        /// </summary>
        /// <value>http://~ for a image on the web or file://~  for a image in the local disk</value>
        [XmlElement]
        public string BackgroundImageUrl { get; set; }

		/// <summary>
        /// Define a location of the background image displayed behind the ContentTileMenu
        /// </summary>
        /// <value>http://~ for a image on the web or file://~  for a image in the local disk</value>
        [XmlElement]
        public string ContentMenuBackgroundImage { get; set; }

        /// <summary>
        /// Define the caption text displayed on the first line on the tile
        /// </summary>
        /// <value>Any Text</value>
        [XmlElement]
        public string CaptionLine1 { get; set; }

        /// <summary>
        /// Define the caption text displayed on the second line on the tile
        /// </summary>
        /// <value>Any text</value>
        [XmlElement]
        public string CaptionLine2 { get; set; }

        /// <summary>
        /// Define whether the stream is live stream or on-demand stream
        /// </summary>
        /// <value><c>true</c> if this instance is live; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsLive { get; set; }

		/// <summary>
		/// Highlight Info
		/// </summary>
		/// <value><c>true</c> if this instance is a highlight; otherwise, <c>false</c>.</value>
		[XmlElement]
		public Highlight Highlight { get; set; }

        /// <summary>
        /// Define whether the stream is a highlight
        /// </summary>
        /// <value><c>true</c> if this instance is a highlight; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsHighlight { get; set; }

        /// <summary>
        /// Define whether the stream is a Recap (Master Highlight Reel)
        /// </summary>
        /// <value><c>true</c> if this instance is a highlight reel/ Recap; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsRecap { get; set; }

        /// <summary>
        /// Define whether the stream is a full replay of a content
        /// </summary>
        /// <value><c>true</c> if this instance is a full replay; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsFullReplay { get; set; }

        /// <summary>
        /// Define whether the stream is upcoming/coming soon
        /// </summary>
        /// <value><c>true</c> if this instance is upcoming; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsUpcoming { get; set; }

        /// <summary>
        /// Define whether the stream is in Beta state
        /// </summary>
        /// <value><c>true</c> if this instance is in Beta; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsBeta { get; set; }

        /// <summary>
        /// Define whether the stream is dealyed due to some reason
        /// </summary>
        /// <value><c>true</c> if this instance is delayed; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsDelayed { get; set; }

        /// <summary>
        /// Define whether the stream is cancelled
        /// </summary>
        /// <value><c>true</c> if this instance is cancelled; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Define whether we should use the radial menu
        /// </summary>
        /// <value><c>true</c> if this instance is a radial camera soon; otherwise, <c>false</c>.</value>
        [XmlElement]
        public bool IsLinearCamera { get; set; }

        /// <summary>
        /// url of the environment asset bundle
        /// </summary>
        /// <value>The EnvironmentUrl to pull from.</value>
        [XmlElement]
        public string EnvironmentID { get; set; }

		/// <summary>
		/// Gets or sets the tile style.
		/// </summary>
		/// <value>The tile style.</value>
		[XmlElement]
		public string TileStyle { get; set; }

        // backwards compatibility
        [XmlElement]
        public string Environment { get; set; }

        /// <summary>
        /// Define an url of the stats image displayed on the player scene
        /// </summary>
        /// <value>The right banner URL.</value>
        [XmlElement]
        public string StatsPanelUrl { get; set; }

        [XmlElement]
        public string CameraLayout { get; set; }

        [XmlElement]
        public string CameraLayoutUrl { get; set; }

		[XmlArray("CameraLayoutList")]
		[XmlArrayItem("CameraLayoutUrl")]
		public List<CameraLayoutUrl> CameraLayoutList { get; private set; }


		[XmlElement]
		public ScoreboardPosition ScoreboardPosition { get; set; }

        [XmlElement]
        public string TwitterURL { get; set; }

        /// <summary>
        /// Provide the straming urls in case of the clip type. Currently at most 4 urls are supported.
        /// </summary>
        /// <value>The stream urls list.</value>
        [XmlArray("StreamUrlsList")]
        [XmlArrayItem("StreamUrl", Type = typeof(LegacyCameraStream))]
        public List<CameraStream> LegacyStreams
        {
            get { return Cameras; }
        }


        [XmlArray("Cameras")]
        [XmlArrayItem("Camera")]
        public List<CameraStream> Cameras { get; private set; }

        //[XmlElement]
        //public CameraStream ProducedFeed { get; set; }

        /// <summary>
        /// Provide the child contents in case of the channel type.
        /// </summary>
        /// <value>The contents list.</value>
        [XmlArray("ContentsList")]
        [XmlArrayItem("Content")]
        public List<Content> Contents { get; private set; }

        [XmlElement]
        public string Featured { get; set; }

        [XmlArray("Tags")]
        [XmlArrayItem("Tag")]
        public List<string> Tags { get; private set; }

		[XmlArray("HighlightsList")]
		[XmlArrayItem("Highlight")]
		public List<Content> HighlightsList { get; private set; }

        [XmlElement("Password")]
        public bool LegacyIsPassword { get; set; }

        [XmlElement]
        public bool IsPrivate { get; set; }

        [XmlElement]
        public AuthenticationConfigurationElement RequiresAuthentication { get; set; }

        [XmlElement]
        public AuthorizationConfiguration Authorization { get; set; }

        [XmlElement]
        public string GameID { get; set; }

        [XmlElement]
        public string StatType { get; set; }

        [XmlElement]
        public bool IsTracked { get; set; }
    }
}
