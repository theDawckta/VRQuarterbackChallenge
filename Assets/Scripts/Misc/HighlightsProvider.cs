using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class HighlightsProvider : MonoBehaviour
{
    public HighlightsEvent HighlightsChanged;

    private DataCursor _Cursor;

    public int PageSize = 10;

    IEnumerator Start()
    {
        if (HighlightsChanged == null)
            HighlightsChanged = new HighlightsEvent();

        yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _Cursor = cursor);
    }

    public bool HasHighlights
    {
        get
        {
			if(_Cursor != null)
            	return _Cursor.CurrentVideo.Children.Any();
            else
            	return false;
        }
    }

    public DataCursor GetHighlights()
    {
        var pager = new DataCursor(_Cursor.CurrentVideo, PageSize, DataCursorComponent.Instance.Data, new CursorHistoryData()); // do not use shared history

        return pager;
    }

    private void OnEnable()
    {
        DataCursorComponent.Instance.Updated += Instance_Updated;
    }

    private void OnDisable()
    {
        if (!DataCursorComponent.ApplicationIsQuitting)
            DataCursorComponent.Instance.Updated -= Instance_Updated;
    }

    private void Instance_Updated(object sender, ContentUpdatedEventArgs<DataCursor> e)
    {
        if (_Cursor != null && ListIsSame(e.Content.CurrentVideo.Children, _Cursor.CurrentVideo.Children))
            return;

        _Cursor = e.Content;

        var pager = GetHighlights();

        HighlightsChanged.Invoke(pager);
    }

    private bool ListIsSame(List<ContentViewModel> a, List<ContentViewModel> b)
    {
        if (a == b)
            return true; // exact same instance

        if (a == null || b == null)
            return false;

        // could check more than just ids if necessary
        return a.Select(_ => _.ID).SequenceEqual(b.Select(_ => _.ID));
    }
}