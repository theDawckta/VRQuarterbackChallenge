using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VOKE.VokeApp.DataModel;

public class DataCursorComponent : Singleton<DataCursorComponent>
{
    public int PageSize = 6;
    private DataCursor _Cursor;

    /*
     * These are stored on the singleton so that they are not disposed during scene changes.
     * for example, when a user selects a video in the Home scene, the same state needs to
     * exist in the Consumption scene when a new cursor is built there.
     */

    public CursorData Data = new CursorData();
    public CursorHistoryData History = new CursorHistoryData();

    private void Awake()
    {
        AppConfigurationLoader.Instance.Updated += Instance_Updated;
    }

    private void Instance_Updated(object sender, ContentUpdatedEventArgs<VokeAppConfiguration> e)
    {
        LoadData(e.Content);

        var ev = Updated;
        if (ev != null)
        {
            var args = new ContentUpdatedEventArgs<DataCursor>(_Cursor);
            ev(this, args);
        }
    }

    public Coroutine GetCursorAsync(Action<DataCursor> callback)
    {
        if (_Cursor != null)
        {
            callback(_Cursor);
            return null;
        }
        else
        {
            return AppConfigurationLoader.Instance.GetDataAsync(data =>
            {
                LoadData(data);
                callback(_Cursor);
            });
        }
    }

    private void LoadData(VokeAppConfiguration data)
    {
        var content = data.GetContent();
        var model = new ContentViewModel(content);
        var newCursor = new DataCursor(model, PageSize, Data, History);

        if (model.Children.Count == 1
            && model.Featured != null
            && model.Featured.Type == ContentType.Clip
            && model.Featured == model.Children[0]
            && newCursor.CurrentVideo == null)
        {
            newCursor.MoveTo(model.Featured);
            newCursor.SingleTileMode = true;
        }

        newCursor.Selected += _Cursor_Selected;
        _Cursor = newCursor;
    }

    private void _Cursor_Selected(object sender, ContentSelectedEventArgs e)
    {
        var ev = Selected;
        if (ev != null)
            ev(sender, e);
    }

    public event EventHandler<ContentSelectedEventArgs> Selected;
    public event EventHandler<ContentUpdatedEventArgs<DataCursor>> Updated;
}