using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using VOKE.VokeApp.DataModel;

public class ContentViewModel
{
    private Content _content;

    public string ID { get; private set; }
    public List<ContentViewModel> Children { get; private set; }
    public List<CameraStreamModel> Streams { get; private set; }
    public int Level { get; set; }
    public bool IsLive { get; set; }
    public bool IsHighlight { get; set; }
	public Highlight Highlight { get; set; }
    public bool IsRecap { get; set; }
    public bool IsFullReplay { get; set; }
    public bool IsUpcoming { get; set; }
    public bool IsBeta { get; set; }
    public bool IsDelayed { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsLinearCamera { get; set; }
    public bool IsTracked { get; set; }
    public string BackgroundAudio { get; set; }
    public ContentViewModel Parent { get; private set; }
    public ContentViewModel Featured { get; set; }
    public CameraStreamModel ProducedFeed { get; private set; }

    public struct SearchResult
    {
        public DataCursor cursor;
        public bool redraw;
    }

    public ContentViewModel(Content content, int level = 0, ContentViewModel parent = null)
    {
        ID = content.ID ?? GenerateUniqueID(content);

        Parent = parent;
        _content = content;

        Level = level;
        IsLive = content.IsLive;
        IsHighlight = content.IsHighlight;
		Highlight = content.Highlight;
        IsRecap = content.IsRecap;
        IsFullReplay = content.IsFullReplay;
        IsBeta = content.IsBeta;
        IsUpcoming = content.IsUpcoming;
        IsDelayed = content.IsDelayed;
        IsCancelled = content.IsCancelled;
        IsLinearCamera = content.IsLinearCamera;
        IsTracked = content.IsTracked;
        BackgroundAudio = content.BackgroundAudio;

        var children = from Content childContent in content.Contents
                       select new ContentViewModel(childContent, level + 1, this);

        var list = children.ToList();

        if (!String.IsNullOrEmpty(content.Featured))
        {
            Featured = list.Find(_ => String.Equals(_.ID, content.Featured));
        }

        Children = list;

        Streams = content.Cameras.Select(_ => new CameraStreamModel(_)).ToList();

        foreach (var stream in content.Cameras)
        {
            var current = Streams.Find(_ => _.ID == stream.ID);

            if (current == null)
                continue;

            foreach (var link in stream.Links)
            {
                var other = Streams.Find(_ => _.ID == link.ID);

                if (other == null)
                {
                    Debug.LogWarningFormat("Invalid link under camera {0}, link id {1}", current.ID, link.ID);
                    continue;
                }

                string[] parts = link.Coordinates.Split(',');
                float x, y;

                if (parts.Length < 2 || !float.TryParse(parts[0], out x) || !float.TryParse(parts[1], out y))
                {
                    Debug.LogWarningFormat("Invalid format under camera {0} for link {1}, coordinates '{2}'", current.ID, link.ID, link.Coordinates);
                    continue;
                }

                var pos = new Vector2(x, y);

                current.Links.Add(pos, other);
            }
        }

        //        if (content.ProducedFeed != null)
        //        {
        //            ProducedFeed = new CameraStreamModel(content.ProducedFeed);
        //        }
    }

    #region Forwarded Properties
    public GeoBlocking IsGeoBlocked
    {
        get { return _content.IsGeoBlocked; }
    }

    public string CameraLayout
    {
        get { return _content.CameraLayout; }
    }

    public string CameraLayoutUrl
    {
        get { return _content.CameraLayoutUrl; }
    }

	public IList<CameraLayoutUrl> CameraLayoutList
	{
		get { return _content.CameraLayoutList; }
	}

    public string BackgroundImageUrl
    {
        get { return _content.BackgroundImageUrl; }
    }

	public string ContentMenuBackgroundImageUrl
    {
		get { return _content.ContentMenuBackgroundImage; }
    }

    public string CaptionLine1
    {
        get { return _content.CaptionLine1; }
    }

    public string CaptionLine2
    {
        get { return _content.CaptionLine2; }
    }

    public IList<string> Tags
    {
        get { return _content.Tags; }
    }

    public string EnvironmentID
    {
        get { return _content.EnvironmentID ?? _content.Environment; }
    }

    public string TwitterURL
    {
        get { return _content.TwitterURL; }
    }

    public string StatsPanelUrl
    {
        get { return _content.StatsPanelUrl; }
    }

    public bool RequiresAuthorization
    {
        get { return _content.Authorization != null || _content.LegacyIsPassword; }
    }

    public AuthenticationConfigurationElement RequiresAuthentication
    {
        get { return _content.RequiresAuthentication; }
    }

    public bool IsPrivate
    {
        get { return _content.IsPrivate || _content.LegacyIsPassword; }
    }

    public AuthorizationConfiguration Authorization
    {
        get { return _content.Authorization; }
    }

	public ScoreboardPosition ScoreboardPosition
	{
		get { return _content.ScoreboardPosition; }
	}

    public string GameID
    {
        get { return _content.GameID; }
    }

    public string StatType
    {
        get { return _content.StatType; }
    }

	public Content GetContent()
	{
		return _content;
	}

    #endregion

    /// <summary>
    /// recursively step through ancestor channels to see if any have an audio string defined
    /// </summary>
    /// <returns>The ancestor background audio as a string</returns>
    /// <param name="curItem">Current contentviewmodel to look for audio background in.</param>
    public string GetAncestorBackgroundAudio(ContentViewModel curItem)
    {
        if (curItem.BackgroundAudio != null)
            return curItem.BackgroundAudio;

        if (curItem.Parent != null)
            return GetAncestorBackgroundAudio(curItem.Parent);

        return null;
    }

	public string GetAncestorContentMenuBackgroundImage()
	{
		if (this.ContentMenuBackgroundImageUrl != null)
		{
			return this.ContentMenuBackgroundImageUrl;
		}

		if (this.Parent != null)
			return this.Parent.GetAncestorContentMenuBackgroundImage ();

		return null;
	}

    public ContentType Type
    {
        get
        {
            if (string.IsNullOrEmpty(_content.ContentType))
            {
                if (Streams.Count > 0)
                {
                    return ContentType.Clip;
                }
                else if (Children.Count > 0)
                {
                    return ContentType.Channel;
                }
                else
                {
                    return ContentType.Unknown;
                }
            }
            else
            {
                switch (_content.ContentType.ToUpper())
                {
                    case "CLIP":
                            return ContentType.Clip;
                    case "CHANNEL":
                            return ContentType.Channel;
                    case "2D":
                            return ContentType.TWO_D;
					case "2DCONSUMPTION":
                            return ContentType.TWO_D_CONSUMPTION;
                    case "360":
                            return ContentType.THREE_SIXTY;
                    default:
                            return ContentType.Unknown;
                }
            }
        }
    }

    private static string GenerateUniqueID(Content content)
    {
        var sb = new StringBuilder();

        sb.Append(content.BackgroundImageUrl);
        foreach (var s in content.Cameras)
        {
            sb.Append(s.ID);
            sb.Append(s.Name);
            sb.Append(s.Url);
        }

        return sb.ToString().ToLowerInvariant();
    }

    public static ContentViewModel FindGrandAncestor(ContentViewModel item)
    {
        if (item.Parent == null || item.Parent.Parent == null || item.Parent.Parent.Parent == null)
            return item;

        return FindGrandAncestor(item.Parent);
    }

    public ContentViewModel FindRecursive(string id)
    {
        return FindAllRecursive(item => String.Equals(item.ID, id)).FirstOrDefault();
    }

    public IEnumerable<ContentViewModel> FindAllRecursive(Predicate<ContentViewModel> predicate)
    {
        return FindAllRecursive(this, predicate);
    }

    public ContentViewModel FindRoot()
    {
        return FindRoot(this);
    }

    public static ContentViewModel FindRoot(ContentViewModel item)
    {
        if (item.Parent == null)
            return item;

        return FindRoot(item.Parent);
    }

    public static IEnumerable<ContentViewModel> FindAllRecursive(ContentViewModel item, Predicate<ContentViewModel> predicate)
    {
        if (predicate(item))
            yield return item;

        foreach (var i in item.Children)
        {
            foreach (var child in FindAllRecursive(i, predicate))
            {
                yield return child;
            }
        }
    }

    /// <summary>
    /// Check back up the tree to determine if this item or any of its ancestors are private or if ancestors share a parent
    /// </summary>
    /// <returns><c>true</c> if this instance has private ancestor the specified item; otherwise, <c>false</c>.</returns>
    private bool HasPrivateAncestor(ContentViewModel item, ContentViewModel curItem)
    {
        //if we have a parent and its shared, we should allow this related item
        if (curItem.Parent != null)
        {
            if (curItem.Parent == item.Parent)
                return false;
        }

        //if we are private, or if we've reached the top level
        if (item.IsPrivate || item.Parent == null)
            return item.IsPrivate;

        return HasPrivateAncestor(item.Parent, curItem);
    }

    private IEnumerable<ContentViewModel> GetRelatedVideos()
    {
        var items = FindRoot(this)
            .FindAllRecursive(_ => _.Type == ContentType.Clip && !HasPrivateAncestor(_, this))
            .Except(new[] { this })
            .OrderBy(_ => _.Tags.Intersect(Tags).Any() ? 0 : 1);

        return items;
    }

    public DataCursor GetRelatedVideos(int pageSize)
    {
        var content = new Content();
        content.Contents.AddRange(GetRelatedVideos().Take(pageSize).Select(vm => vm._content));
        var model = new ContentViewModel(content);

        var pager = new DataCursor(model, pageSize, DataCursorComponent.Instance.Data, new CursorHistoryData()); // do not use shared history

        return pager;
    }

    /// <summary>
    /// Finds Video clips which have any of the tags from the search tags
    /// </summary>
    /// <param name="cursorToSearch">cursor to search from</param>
    /// <param name="searchTags">search tags tp be looking up</param>
    /// <returns>SearchResults with appropriate values</returns>
    public SearchResult GetSearchedVideos(DataCursor cursorToSearch, List<string> searchTags)
    {
        SearchResult res = new SearchResult();
        res.redraw = true;

        var searchedTiles = FindAllRecursive(c => c.Type == ContentType.Clip && c.Tags.Intersect(searchTags).Any());
        Debug.Log("<color=red>Number of searched Tiles found = " + searchedTiles.Count() + "</color>");

        if (searchedTiles.Count() == 0) //No tiles found, data remains same,don't redraw tiles
        {
            res.redraw = false;
            res.cursor = cursorToSearch;
            return res;
        }

        var content = new Content();
        content.Contents.AddRange(searchedTiles.Select(c => c._content));
        var model = new ContentViewModel(content);

        var pager = new DataCursor(model, cursorToSearch.PageSize, DataCursorComponent.Instance.Data, new CursorHistoryData()); // do not use shared history
        res.cursor = pager;

        return res;
    }

    /// <summary>
    /// Finds Live video clips (that have IsLive tag)
    /// </summary>
    /// <param name="cursorToSearch">cursor to search from</param>
    /// <returns>Search results with all the live content</returns>
    public SearchResult GetLiveVideos(DataCursor cursorToSearch)
    {
        SearchResult res = new SearchResult();
        res.redraw = true;

        var searchedTiles = FindAllRecursive(c => c.Type == ContentType.Clip && c.IsLive == true);
        Debug.Log("<color=red>Number of Live Tiles found = " + searchedTiles.Count() + "</color>");

        if (searchedTiles.Count() == 0) //No tiles found, data remains same,don't redraw tiles
        {
            res.redraw = false;
            res.cursor = cursorToSearch;
            return res;
        }

        var content = new Content();
        content.Contents.AddRange(searchedTiles.Select(c => c._content));
        var model = new ContentViewModel(content);

        var pager = new DataCursor(model, cursorToSearch.PageSize, DataCursorComponent.Instance.Data, new CursorHistoryData()); // do not use shared history
        res.cursor = pager;

        return res;
    }

    public ContentViewModel GetWatchNextVideo()
    {
        int index = Parent.Children.IndexOf(this);

        if (index >= 0)
        {
            var nextInCurrentChannel = Parent.Children.ElementAtOrDefault(index + 1);

            if (nextInCurrentChannel != null)
            {
				if (nextInCurrentChannel.Type == ContentType.Clip || nextInCurrentChannel.Type == ContentType.TWO_D_CONSUMPTION)
                    return nextInCurrentChannel;
            }
        }

        var nextRelatedVideo = GetRelatedVideos().FirstOrDefault();

        if (nextRelatedVideo.CaptionLine1 != null && CaptionLine1 != null)
        {
            if (nextRelatedVideo.CaptionLine1 == CaptionLine1)
                nextRelatedVideo = GetRelatedVideos().Where(c => c.CaptionLine1 != CaptionLine1).FirstOrDefault();
        }

        if (nextRelatedVideo != null)
            return nextRelatedVideo;

        return null;
    }
}
