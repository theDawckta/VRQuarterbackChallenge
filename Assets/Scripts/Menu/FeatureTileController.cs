using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FeatureTileController : MonoBehaviour
{
    public GameObject FeatureContentTileGameObject;
    public ContentTileMenuController HomeMenuController;
	public bool AllowTileFade = true;

    private DataCursor _cursor;
    [HideInInspector]
    public ContentTileController FeatureContentTile;
	//pause before loading in feature tile - should generally match delay in ContentTileMenuController, though does not have to match 1/1
	//slight delay allows it to load in a bit later, which "feels" better
	private float _startContentDrawWait = 0.65f;

    IEnumerator Start()
    {
        FeatureContentTile = FeatureContentTileGameObject.GetComponent<ContentTileController>();
        FeatureContentTile.ContentTileMenu = HomeMenuController;
        yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);
        DrawFeatureTile(_cursor.Featured, _cursor);
    }

    void ContentSelected(object sender, ContentSelectedEventArgs e)
    {
        if (e.SelectedContent.Type == ContentType.Channel)
        {
            DrawFeatureTile(e.SelectedContent.Featured, (DataCursor)sender);
        }
    }

    void OnEnable()
    {
        DataCursorComponent.Instance.Selected += ContentSelected;
    }

    void OnDisable()
    {
        if (!DataCursorComponent.ApplicationIsQuitting)
            DataCursorComponent.Instance.Selected -= ContentSelected;
    }

	void OnTileClickAction()
	{
		EventManager.Instance.TileClickEvent ();
		if (_cursor.Featured != null)
		{
			if (FeatureContentTile != null)
			{
				FeatureContentTile.FadeTileOut ();
			}
		}
	}

	IEnumerator StartDrawFeatureTile(ContentViewModel item)
	{
		yield return new WaitForSeconds(_startContentDrawWait);
		//always start at 0 alpha - it will get faded up
		FeatureContentTile.GetComponent<CanvasGroup> ().alpha = 0;
		FeatureContentTile.gameObject.GetComponent<Canvas>().enabled = true;
		FeatureContentTile.Init(item, _cursor);
		FeatureContentTile.AllowClicks = true;
		FeatureContentTile.SetTileFade(AllowTileFade);

	}

    void DrawFeatureTile(ContentViewModel item, DataCursor cursor)
    {
        if (item != null)
        {
			StartCoroutine(StartDrawFeatureTile(item));
        }
        else
        {
			FeatureContentTile.GetComponent<CanvasGroup> ().alpha = 0;
            FeatureContentTile.gameObject.GetComponent<Canvas>().enabled = false;
            FeatureContentTile.AllowClicks = false;
        }
    }
}