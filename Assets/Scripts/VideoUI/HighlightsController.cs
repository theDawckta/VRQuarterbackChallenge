using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VOKE.VokeApp.DataModel;
using DG.Tweening;

[System.Serializable]
public enum HighlightType
{
	NONE,
	FREED,
	TRUEVR
};

public class HighlightsController : MonoBehaviour 
{
	public delegate void OnMarkerClickEvent(long position);
	public event OnMarkerClickEvent OnMarkerClicked;

	public SpriteRenderer TrueVRTransitionSprite;
	public SpriteRenderer FreeDTransitionSprite;
	public ContentTileHighlightController ContentTilePrefab;
	public HighlightMarkerController HighlightMarkerPrefab;  
	public GameObject LeftTimelineEdge;
	public GameObject RightTimelineEdge;
	public GameObject HighlightButtonHolder;
	public Texture TrueVRHighlightBackground;
	public Texture FreeDHighlightBackground;
	public Texture HighlightBackground;
	public AudioClip HighlightShowAudio;

	private JackalopeMediaPlayer _mainMovie;
	private AudioController _AudioController;
	private List<ContentViewModel> _highlights = new List<ContentViewModel>();
	private List<ContentTileHighlightController> _highlightContentTiles = new List<ContentTileHighlightController>();
	private List<HighlightMarkerController> _highlightMarkers = new List<HighlightMarkerController>();
	private float _pollRate = 5.0f;
	private float _showHighlightTime = 10.0f;
	private float _maxMovieWaitTime = 5.0f;
	private long _videoTotalTime;
	private DataCursor _cursor;

    void Awake()
    {
		TrueVRTransitionSprite.DOFade (0.0f, 0.0f);
		FreeDTransitionSprite.DOFade (0.0f, 0.0f);
		_AudioController = Extensions.GetRequired<AudioController>();
    }

	IEnumerator Start()
	{
		yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);
		_highlights = _cursor.GetHighlights ();
	}

	public void SetPlayer (JackalopeMediaPlayer mediaPlayer)
	{
		_mainMovie = mediaPlayer;
		if(_highlights.Count > 0)
			StartCoroutine("InitHighlights");
	}

	IEnumerator InitHighlights()
	{
		float timePassed = 0.0f;

		while (timePassed < _maxMovieWaitTime) 
		{
			if (_mainMovie.Length > 0.0f) 
			{
				PlaceHighlights ();
				break;
			} 
			timePassed = timePassed + Time.deltaTime;
			yield return null;
		}
	}

	void PlaceHighlights()
	{
		List<string> positions = new List<string> ();

		for (int i = 0; i < _highlights.Count; i++) 
		{
			positions.Add (_highlights[i].Highlight.Timecode);
		}

		// remove duplicate positions
		positions = positions.Distinct().ToList();

		for (int j = 0; j < positions.Count; j++)
		{
			double timeInMilliSeconds = (TimeSpan.Parse (positions[j]).TotalSeconds) * 1000;
			float percentOfLength = (float)(timeInMilliSeconds / _mainMovie.Length);
			HighlightMarkerController marker = (HighlightMarkerController)Instantiate (HighlightMarkerPrefab);
			marker.Position = (long)timeInMilliSeconds;
			_highlightMarkers.Add (marker);
			marker.OnMarkerClicked += HighlightMarkerClicked;
			marker.transform.SetParent (LeftTimelineEdge.transform.parent, false);
			float sliderLen = (RightTimelineEdge.transform.localPosition.x - LeftTimelineEdge.transform.localPosition.x);
			marker.transform.localPosition = new Vector3 (sliderLen * percentOfLength, 0, 0);
		}

		InvokeRepeating ("CheckHighlights", 10.0f, _pollRate);
	}

	void CheckHighlights()
	{
		bool skip = false;
		for(int j = 0; j < _highlights.Count; j++)
		{
			if(TimecodeToMilliseconds(_highlights[j].Highlight.Timecode) <= _mainMovie.Position && 
				TimecodeToMilliseconds(_highlights[j].Highlight.Timecode) > _mainMovie.Position - 5000.0f)
			{
				// Check to see if its already showing, set skip variable if it is
				for (int i = 0; i < _highlightContentTiles.Count; i++)
					if (_highlightContentTiles [i].TileContentViewModel == _highlights[j])
						skip = true;
				
				if (!skip) 
				{
					_AudioController.PlayAudioClip(HighlightShowAudio);
					ContentTileHighlightController highlightContentTile = (ContentTileHighlightController)Instantiate (ContentTilePrefab);
					_highlightContentTiles.Add (highlightContentTile);
					highlightContentTile.Init (_highlights [j], _cursor);
					highlightContentTile.TileHighlightType = _highlights [j].Highlight.Type.ToString();
					highlightContentTile.CTAButton.GetComponent<RectTransform> ().sizeDelta = new Vector2 (35.0f, 35.0f);
					highlightContentTile.gameObject.layer = LayerMask.NameToLayer ("TopUI");
					highlightContentTile.transform.SetParent (HighlightButtonHolder.transform, false);

					if (_highlights[j].IsHighlight && (_highlights[j].Highlight.Type != "FREED" && _highlights[j].Highlight.Type != "TRUEVR"))
						highlightContentTile.ChangeBackgroundImage (HighlightBackground);
					else if(_highlights[j].Highlight.Type == "FREED")
						highlightContentTile.ChangeBackgroundImage (FreeDHighlightBackground);
					else if(_highlights[j].Highlight.Type == "TRUEVR")
						highlightContentTile.ChangeBackgroundImage (TrueVRHighlightBackground);
					
					highlightContentTile.CTAButton.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (highlightContentTile.CTAButton.transform.localPosition.x, -2.0f);
					highlightContentTile.OnTileSelected += HighlightSelected;
					StartCoroutine (HideTileTimer(highlightContentTile));
				}
				skip = false;
			}
		}
	}

	IEnumerator HideTileTimer(ContentTileHighlightController contentTile)
	{
		float timePassed = 0.0f;

		while (timePassed < _showHighlightTime && contentTile != null) 
		{
			timePassed = timePassed + Time.deltaTime;
			yield return null;
		}

		if (contentTile != null) 
		{
			for (int i = 0; i < _highlightContentTiles.Count; i++) 
			{
				if (_highlightContentTiles [i] == contentTile) 
				{
					contentTile.FadeTileOut ();
					yield return new WaitForSeconds (0.5f);
					_highlightContentTiles.Remove (contentTile);
					Destroy (contentTile.gameObject);
				}
			}
		}
	}

	void HighlightSelected (ContentTileHighlightController tileClicked)
	{
		PlayerPrefs.SetFloat ("mainMovieLastPosition", (long)_mainMovie.Position);

		tileClicked.OnTileSelected -= HighlightSelected;
		for (int i = 0; i < _highlightMarkers.Count; i++)
		{
			_highlightMarkers[i].gameObject.SetActive(false);
		}
		ResourceManager.Instance.VRCameraFadeObj.FadeCameraOut (1.0f);
		if (tileClicked.TileHighlightType == "FREED")
		{
			Sequence logoSequence = DOTween.Sequence();
			logoSequence.AppendInterval(1.0f);
			logoSequence.Append(FreeDTransitionSprite.DOFade (1.0f, 0.3f));
			logoSequence.AppendInterval(1.3f);
			logoSequence.Append(FreeDTransitionSprite.DOFade (0.0f, 0.3f));
		}
		else if (tileClicked.TileHighlightType == "TRUEVR")
		{
			Sequence logoSequence = DOTween.Sequence();
			logoSequence.AppendInterval(1.0f);
			logoSequence.Append(TrueVRTransitionSprite.DOFade (1.0f, 0.3f));
			logoSequence.AppendInterval(1.3f);
			logoSequence.Append(TrueVRTransitionSprite.DOFade (0.0f, 0.3f));
		}

		StartCoroutine(DoTileDoMove(tileClicked));
	}

	IEnumerator DoTileDoMove(ContentTileHighlightController tileClicked)
	{
		yield return new WaitForSeconds(2.6f);

		tileClicked.DoMove();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	void OnVideoPositionChangedHandler()
    {
		for (int i = 0; i < _highlightContentTiles.Count; i++)
    	{
			CleanUpTiles();
    	}
    }

	void HighlightMarkerClicked(HighlightMarkerController markerClicked)
	{
		for (int i = 0; i < _highlightMarkers.Count; i++)
		{
			if (markerClicked == _highlightMarkers [i]) 
			{
				OnMarkerClicked (_highlightMarkers [i].Position);
				CleanUpTiles();
			}
		}
	}

	void CleanUpTiles()
	{
		StopAllCoroutines ();
		for (int i = 0; i < _highlightContentTiles.Count; i++)
		{
			Destroy(_highlightContentTiles [i].gameObject);
		}
		_highlightContentTiles.Clear ();
		_mainMovie.OnReadyToPlay += StartCheckHighlights;
	}

	void StartCheckHighlights()
	{
		_mainMovie.OnReadyToPlay -= StartCheckHighlights;
		StartCoroutine ("WaitForMainMovieLength");
	}

	IEnumerator WaitForMainMovieLength()
	{
		yield return new WaitForEndOfFrame ();
		CheckHighlights ();
	}

    long TimecodeToMilliseconds(string timeCode)
    {
    	long milliseconds = 0;
    	long seconds = 0;
		long minutes = 0;
		long hours = 0;
		String[] substrings = timeCode.Split(':');
		for(int i = 3; i >= 0; i--)
		{
			switch (i)
			{
				case 2:
				long.TryParse(substrings[i], out seconds);
				break;
				case 1:
				long.TryParse(substrings[i], out minutes);
				break;
				case 0:
				long.TryParse(substrings[i], out hours);
				break;
			}
		}
		milliseconds = (seconds * 1000) + (minutes * 60 * 1000) + (hours * 60 * 60 * 1000);
		return milliseconds;
    }

	void OnEnable()
	{
		EventManager.OnVideoPositionChangedEvent += OnVideoPositionChangedHandler;
	}

    void OnDisable()
    {
		EventManager.OnVideoPositionChangedEvent -= OnVideoPositionChangedHandler;
    }
}
