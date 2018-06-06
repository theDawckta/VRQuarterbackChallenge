using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataCursor : DataPager<ContentViewModel>
{
    private ContentViewModel _Root;
    private CursorData _Data;
    private CursorHistoryData _HistoryData;

    public bool SingleTileMode { get; set; }

    public DataCursor(ContentViewModel root, int pageSize, CursorData data, CursorHistoryData historyData)
    {
        if (root == null)
            throw new ArgumentNullException("root");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException("pageSize", "Page size must be at least 1.");

        if (data == null)
            throw new ArgumentNullException("data");

        if (historyData == null)
            historyData = new CursorHistoryData();

        _Data = data;
        _HistoryData = historyData;
        _Root = root;

        PageSize = pageSize;
    }

    #region Stateful data

    /*
     * These methods exist to ensure that the shared state objects (_Data, _HistoryData) are kept in sync across all instances of the cursor.
     * For example, more than one cursor may exist in the scene at once, or when new data comes in from the XML loader we may need to create
     * a new cursor, but we need to preserve the existing Stack of history or current video.  So we need to ensure that the history is recorded
     * any time it changes (ModifyState).
     */

    private void ModifyState(Action action)
    {
        EnsureCurrentVideoSynced();
        EnsureHistorySynced();
        EnsurePageIndexesSynced();

        action();

        _Data.CurrentVideoID = _CurrentVideo != null ? _CurrentVideo.ID : null;
        _HistoryData.HistoryIDs = _History.Select(_ => _.ID).ToArray();
        _HistoryData.PageIndexHistory = _PageIndexHistory.ToArray();
    }

    private Stack<int> _PageIndexHistory = new Stack<int>();
    protected Stack<int> PageIndexHistory
    {
        get
        {
            EnsurePageIndexesSynced();
            return _PageIndexHistory;
        }
    }

    private void EnsurePageIndexesSynced()
    {
        if (_HistoryData.PageIndexHistory != null && !_PageIndexHistory.SequenceEqual(_HistoryData.PageIndexHistory))
        {
            _PageIndexHistory = new Stack<int>(_PageIndexHistory.Reverse());
        }
    }

    private Stack<ContentViewModel> _History = new Stack<ContentViewModel>();
    protected Stack<ContentViewModel> History
    {
        get
        {
            EnsureHistorySynced();
            return _History;
        }
    }

    private void EnsureHistorySynced()
    {
        if (_HistoryData.HistoryIDs != null && !_History.Select(_ => _.ID).SequenceEqual(_HistoryData.HistoryIDs))
        {
            var currentItems = _Root.FindAllRecursive(_ => _HistoryData.HistoryIDs.Contains(_.ID));

            var items = from id in _HistoryData.HistoryIDs
                        join item in currentItems on id equals item.ID
                        select item;

            /*
             * history is stored in shared state as a string[] instead of a complex object because object references
             * may change when data is loaded fresh, so the references need to be rebuilt with the new data
             */
            _History = new Stack<ContentViewModel>(items.Reverse());
        }
    }

    private ContentViewModel _CurrentVideo;
    public ContentViewModel CurrentVideo
    {
        get
        {
            EnsureCurrentVideoSynced();

			#if UNITY_EDITOR
			if(_CurrentVideo == null)
			{
				return _Root.FindAllRecursive(_ => _.Type == ContentType.Clip).FirstOrDefault();
			}
			#endif

            return _CurrentVideo;

        }
    }

    private void EnsureCurrentVideoSynced()
    {
        if (!String.IsNullOrEmpty(_Data.CurrentVideoID) && (_CurrentVideo == null || _CurrentVideo.ID != _Data.CurrentVideoID))
        {
            _CurrentVideo = _Root.FindRecursive(_Data.CurrentVideoID);
        }
    }

    public override int PageIndex
    {
        get
        {
            return _HistoryData.PageIndex;
        }
        protected set
        {
            _HistoryData.PageIndex = value;
        }
    }
    #endregion

    protected override IEnumerable<ContentViewModel> DataToPage
    {
        get
        {
            IEnumerable<ContentViewModel> items = CurrentItems;

            if (Featured != null)
            {
                items = items.Except(new[] { Featured });
            }

            items = items.Except(PrivateChannels);

            return items;
        }
    }

    public ContentViewModel GetRoot()
    {
        return _Root;
    }

    public ContentViewModel Featured
    {
        get { return CurrentParent.Featured; }
    }

    public IEnumerable<ContentViewModel> PrivateChannels
    {
        get { return CurrentItems.Where(_ => _.IsPrivate); }
    }

    public ContentViewModel CurrentParent
    {
        get
        {
            if (History.Count < 1)
                return _Root;

            return History.Peek();
        }
    }

    public List<ContentViewModel> CurrentItems
    {
        get { return CurrentParent.Children; }
    }

	public List<ContentViewModel> GetHighlights()
	{
		List<ContentViewModel> highlightList = new List<ContentViewModel> ();
		for (int i = 0; i < CurrentVideo.Children.Count; i++) 
		{
			if(CurrentVideo.Children[i].IsHighlight)
				highlightList.Add(CurrentVideo.Children[i]);
		}
		return highlightList;
	}

    public bool CanMoveUp
    {
        get { return CurrentParent.Parent != null; }
    }

    public bool MoveUp()
    {
        if (!CanMoveUp)
            return false;

        MoveTo(CurrentParent.Parent);
        return true;
    }

    public bool IsHome
    {
        get { return CurrentParent == _Root; }
    }

	public void ClearHistory()
	{
		ModifyState(() =>
		{
			_History.Clear ();
		});
	}

	public void ClearLastItem()
	{
		ModifyState(() =>
		{
			_History.Pop ();
		});
	}

    public void MoveToHome()
    {
        if (IsHome)
            return;

        MoveTo(_Root);
    }

    public bool CanMoveBackInHistory
    {
        get { return History.Count > 0; }
    }

	public int GetHistoryLength()
	{
		return History.Count;
	}


    public bool MoveBackInHistory()
    {
        if (!CanMoveBackInHistory)
            return false;

        ModifyState(() =>
        {
            _History.Pop();

            //if the page exists in our history, allow step back - otherwise reset
            if (_PageIndexHistory.Count > 0)
            {
                int prevPage = _PageIndexHistory.Pop();
                PageIndex = prevPage;
            }
            else
            {
                PageIndex = 0;
            }
        });

        OnContentSelected(CurrentParent);
        return true;
    }

	public ContentViewModel ReturnRoot()
	{
		return _Root;
	}

    public void MoveTo(ContentViewModel content)
    {

        if (content == null)
            throw new ArgumentNullException("content");

        switch (content.Type)
        {
            case ContentType.Channel:
                ModifyState(() =>
                {
                    _History.Push(content);
                    _PageIndexHistory.Push(PageIndex);
                    PageIndex = 0; // reset paging when moving
                });
                break;

            case ContentType.Clip:
                ModifyState(() => _CurrentVideo = content);
                break;
			case ContentType.TWO_D_CONSUMPTION:
				ModifyState(() => _CurrentVideo = content);
				break;
			case ContentType.THREE_SIXTY:
				ModifyState(() => _CurrentVideo = content);
				break;

            default:
                return;
        }

        OnContentSelected(content);
    }

    protected virtual void OnContentSelected(ContentViewModel content)
    {
        var ev = Selected;
        if (ev != null)
        {
            var args = new ContentSelectedEventArgs(content);
            ev(this, args);
        }
    }

    public event EventHandler<ContentSelectedEventArgs> Selected;
}