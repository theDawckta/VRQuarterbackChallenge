using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using VOKE.VokeApp.DataModel;
using UnityEngine.SceneManagement;

public class ContentTileMenuController : MonoBehaviour
{
    public VideoGroup Videos;
    public GameObject ContentTilePrefab;
    public Vector2 TileSize;
    public int NumOfRows;
    public int NumOfColumns;
    public int SpaceBetweenTiles;
    public GameObject TileHolder1;
    public GameObject TileHolder2;
    public ButtonControllerReplacement PageLeftButton;
    public ButtonControllerReplacement PageRightButton;
    public GameObject PageIndicator;
    public GameObject PagingHolder;
    public RawImage BackStop;
    public TwoDPlayerController TwoDPlayer;
    public bool UseDefaultColors = false;
	public bool GlobalBackAffectsMenu = true;
    [HideInInspector]
    public ContentTileController CurrentTile;
    [HideInInspector]
    public Color Primary;
    [HideInInspector]
    public Color Secondary;
    [HideInInspector]
    public Color Highlight;
    [HideInInspector]
    public Color Gray;

    private CanvasGroup _canvasGroup;
    private CanvasGroup _pagingHolderCanvas;
    private GameObject _paging;
    private PageDirection _direction = PageDirection.None;
    private List<PageIndicatorController> _pageIndicators = new List<PageIndicatorController>();
    private ReticleController _reticle;
    private ContentTileController[] _contentTileList1;
    private ContentTileController[] _contentTileList2;
    private ContentTileController[] _currentContentTileList;
    private RectTransform _currentTileHolder;
    private List<float> _tileDelayArray;
    private RectTransform _menuRectTransform;
    private RectTransform _tileHolderRectTransform1;
    private RectTransform _tileHolderRectTransform2;
    private Vector3 _tileOffset;
    private Vector3 _menuSize;
    private float _animationDuration = 0.3f;
    private DataCursor _cursor;
    //private AudioController _AudioController;
    //disable menu while animating
    private float _startContentDrawWait = 0.35f;
    private List<string> searchTags;
    private string _sceneToNavTo;
	private int _minHistoryLength = 2;		//if history is less than this, we are essentially in "home"

    void Awake()
    {
        if (ResourceManager.Instance.Reticle == null)
            throw new Exception("A Reticle is required in ResourceManager for ContentTileMenuController");
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
        _pagingHolderCanvas = PagingHolder.GetComponent<CanvasGroup>();
        _paging = PagingHolder.transform.Find("Paging").gameObject;
        if (_paging == null) Debug.LogError(gameObject.name + " does not have Paging assigned properly");
        _menuSize = new Vector3((TileSize.x * NumOfColumns) + SpaceBetweenTiles * (NumOfColumns - 1),
                                (TileSize.y * NumOfRows) + SpaceBetweenTiles * (NumOfRows - 1),
                                 gameObject.transform.position.z);
        _reticle = ResourceManager.Instance.Reticle.GetComponent<ReticleController>();
        _contentTileList1 = new ContentTileController[NumOfRows * NumOfColumns];
        _contentTileList2 = new ContentTileController[NumOfRows * NumOfColumns];
        _currentContentTileList = _contentTileList1;
        _menuRectTransform = gameObject.GetComponent<RectTransform>();
        _menuRectTransform.sizeDelta = new Vector2(_menuSize.x, _menuSize.y);
        _tileHolderRectTransform1 = TileHolder1.GetComponent<RectTransform>();
        _tileHolderRectTransform2 = TileHolder2.GetComponent<RectTransform>();
        _tileOffset = new Vector3(-(_menuSize.x / 2) + TileSize.x / 2,
                                   -(_menuSize.y / 2) + TileSize.y / 2,
                                   gameObject.transform.position.z);
        _currentTileHolder = _tileHolderRectTransform1;
        _pagingHolderCanvas.DOFade(0.0f, 0.0f);
		if(BackStop != null)
        	BackStop.DOFade(0.0f, 0.0f);
        searchTags = new List<string>();

		//hide paging if Related
		if (Videos == VideoGroup.RelatedVideos)
		{
			PageLeftButton.gameObject.SetActive (false);
			PageRightButton.gameObject.SetActive (false);
		}

        if (UseDefaultColors)
        {
            Primary = GlobalVars.Instance.PrimaryColor;
            Secondary = GlobalVars.Instance.SecondaryColor;
            Highlight = GlobalVars.Instance.HighlightColor;
        }
    }

    void Start()
    {
        PageLeftButton.Enable();
        PageRightButton.Enable();
    }

    public void StartInit(DataCursor cursor = null)
    {
        StartCoroutine(Init(cursor));
    }

    IEnumerator Init(DataCursor curCursor = null)
    {
		int firstCurrentVideos = 0;

        if (curCursor != null)
        {
            _cursor = curCursor;
			_cursor.PageSize = NumOfRows * NumOfColumns;
        }
        else
        {
            yield return DataCursorComponent.Instance.GetCursorAsync(cursor =>
            {
                _cursor = cursor;
                _cursor.PageSize = NumOfRows * NumOfColumns;
            });
        }

		for(int i = 0; i < _cursor.CurrentItems.Count; i++)
		{
			if (!_cursor.CurrentItems [i].IsUpcoming) 
			{
				firstCurrentVideos = i;
				break;
			}
			if (firstCurrentVideos == 0)
				firstCurrentVideos = _cursor.CurrentItems.Count - 1;
		}

		for (int k = 1; k * NumOfRows * NumOfColumns < firstCurrentVideos + 1; k++)
			_cursor.MoveToNextPage();

        StartCoroutine("GetVideosAndDraw");
		if(BackStop != null)
	        BackStop.DOFade(0.8f, 0.3f).SetDelay(0.3f);
    }

    public void InitColors(Color primary, Color secondary, Color gray, Color highlight)
    {
        Primary = primary;
        Secondary = secondary;
        Gray = gray;
        Highlight = highlight;
        PageLeftButton.Init(Primary, Secondary, Gray, Highlight);
        PageRightButton.Init(Primary, Secondary, Gray, Highlight);
    }

    public void HideTiles()
    {
        for (int i = 0; i < _currentContentTileList.Length; i++)
        {
            if (_currentContentTileList[i] != null)
            {
                _currentContentTileList[i].FadeTileOut(0.0f);
            }
        }
    }

    public void SetNavScene(string sceneToNavTo)
    {
        _sceneToNavTo = sceneToNavTo;
    }

    public void RemoveOldTiles()
    {
        if (TileHolder1 != null)
        {
            foreach (Transform tile in TileHolder1.transform)
            {
                Destroy(tile.gameObject);
            }
        }

        if (TileHolder2 != null)
        {
            foreach (Transform tile in TileHolder2.transform)
            {
                Destroy(tile.gameObject);
            }
        }
    }

    public void ChangeVideoType(VideoGroup videoGroup)
    {
        //Videos = videoGroup;
        StartCoroutine(GetVideosAndDraw());
    }

    IEnumerator GetVideosAndDraw()
    {
        bool redrawTiles = true;
        if (_cursor == null)
        {
            yield return DataCursorComponent.Instance.GetCursorAsync(cursor =>
            {
                cursor.PageSize = NumOfRows * NumOfColumns;
                _cursor = cursor;

				if (Videos == VideoGroup.RelatedVideos && cursor.CurrentVideo != null)
              	{
                  _cursor = cursor.CurrentVideo.GetRelatedVideos (NumOfRows * NumOfColumns);
				}

                if (_cursor != null && _cursor.CurrentItems.Count > 0)
                {
                    if (redrawTiles)
                        StartCoroutine(StartDrawContentTiles());
                }
                else
                {
                    gameObject.SetActive(false);
                }
            });
        }
        else
        {
            if (_cursor != null && _cursor.CurrentItems.Count > 0)
            {
                if (redrawTiles)
                    StartCoroutine(StartDrawContentTiles());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public ContentViewModel.SearchResult GetSearchedVideos(DataCursor cursorToSearch, List<string> searchTags)
    {
        return (cursorToSearch.CurrentParent.FindRoot().GetSearchedVideos(cursorToSearch, searchTags));
    }

    public ContentViewModel.SearchResult GetLiveVideos(DataCursor cursorToSearch)
    {
        return (cursorToSearch.CurrentParent.FindRoot().GetLiveVideos(cursorToSearch));
    }

    void OnMenuUpdated(object sender, ContentUpdatedEventArgs<DataCursor> args)
    {
        _cursor = args.Content;
        _cursor.PageSize = NumOfColumns * NumOfRows;
        StartCoroutine(StartDrawContentTiles());
    }

    /// <summary>
    /// Draws the content Tiles based on the Cursor that is set
    /// </summary>
    public void DrawContentTiles()
    {
        StartCoroutine(StartDrawContentTiles());
    }

    IEnumerator StartDrawContentTiles()
    {
        RemoveOldTiles();
        yield return new WaitForSeconds(_startContentDrawWait);
        GameObject TempContentTile;
        Vector3 currentLocation;
        float delay;
        int count = 0;
        List<ContentViewModel> tiles = _cursor.GetCurrentPage().ToList();
        DrawPaging();
        CreateRandomDelayArray(tiles.Count);

        // Need to implement ContentTileMenu background images
//		if(_cursor.CurrentParent.ContentMenuBackgroundImageUrl != null)
//    	{
//    		Debug.Log("WE HAVE BACKGROUND");
//			Debug.Log ("_cursor.CurrentParent.ContentMenuBackgroundImageUrl: " + _cursor.CurrentParent.ContentMenuBackgroundImageUrl);
//    	}
//    	else
//    	{
//			Debug.Log("WE DON'T HAVE BACKGROUND");
//    	}

		string curImage = _cursor.CurrentParent.GetAncestorContentMenuBackgroundImage ();
		if (!string.IsNullOrEmpty (curImage))
			EventManager.Instance.BackgroundImageTriggerEvent (curImage);


        for (int y = NumOfRows - 1; y >= 0; y--)
        {
            for (int x = 0; x < NumOfColumns; x++)
            {
                currentLocation = new Vector3(((TileSize.x * x) + (SpaceBetweenTiles * x)) + _tileOffset.x,
                                              ((TileSize.y * y) + (SpaceBetweenTiles * y)) + _tileOffset.y,
                                                0.0f);

                if (count < tiles.Count)
                {
                    delay = _tileDelayArray[count];
                    TempContentTile = (GameObject)Instantiate(ContentTilePrefab, currentLocation, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                    TempContentTile.transform.SetParent(_currentTileHolder.gameObject.transform, false);
                    TempContentTile.layer = _currentTileHolder.gameObject.layer;
                    _currentContentTileList[count] = TempContentTile.GetComponent<ContentTileController>();
                    _currentContentTileList[count].InitColors(Primary, Secondary, Gray, Highlight);
                    if (_direction == PageDirection.Right)
                        _currentContentTileList[count].Init(tiles[count], _cursor, true, delay);
                    else if (_direction == PageDirection.Left)
                        _currentContentTileList[count].Init(tiles[count], _cursor, false, delay);
                    else
                        _currentContentTileList[count].Init(tiles[count], _cursor, delay);
                    _currentContentTileList[count].ContentTileMenu = gameObject.GetComponent<ContentTileMenuController>();
                    _currentContentTileList[count].Reticle = _reticle;
                    _currentContentTileList[count].ResizeTile(TileSize);
                    count++;
                }
            }
        }

        if (_currentTileHolder == _tileHolderRectTransform1)
            _currentTileHolder = _tileHolderRectTransform2;
        else
            _currentTileHolder = _tileHolderRectTransform1;

        StartCoroutine("CleanupOldTiles");
		bool isHome = false;
		if (_cursor.IsHome || _cursor.GetHistoryLength () <= _minHistoryLength)
			isHome = true;
        EventManager.Instance.DrawTilesCompleteEvent(isHome);
		EventManager.Instance.DrawContentMenuTilesEvent (_cursor.CurrentParent);
        _direction = PageDirection.None;
        yield return null;
    }

    public void AnimTilesOut()
    {
        float delay;
        for (int i = 0; i < _currentContentTileList.Length; i++)
        {
            delay = UnityEngine.Random.Range(0.0f, 0.3f);
            if (_currentContentTileList[i] != null)
            {
                _currentContentTileList[i].FadeTileOut(delay);
            }
        }
    }

    /// <summary>
    /// Shuffle our delay list - TODO: Abstract to take any list
    /// </summary>
    void ShuffleDelayList()
    {
        int tileCount = _tileDelayArray.Count;
        int n = tileCount;

        while (n > 0)
        {
            n--;
            int newLoc = UnityEngine.Random.Range(0, tileCount);
            float newLocVal = _tileDelayArray[n];
            float oldLocVal = _tileDelayArray[newLoc];
            _tileDelayArray[newLoc] = newLocVal;
            _tileDelayArray[n] = oldLocVal;
        }
    }


    /// <summary>
    /// Keep our delay equivalent to the number of tiles there are
    /// </summary>
    /// <param name="tileCount">Tile count.</param>
    void CreateRandomDelayArray(int tileCount)
    {
        _tileDelayArray = new List<float>();
        for (int i = 0; i < tileCount; i++)
        {
            float delay = i * 0.04f;
            _tileDelayArray.Add(delay);
        }
        ShuffleDelayList();
    }


    void DrawPaging()
    {
        if (!_cursor.HasNextPage)
            PageRightButton.Disable();
        else
            PageRightButton.Enable();

        if (!_cursor.HasPreviousPage)
            PageLeftButton.Disable();
        else
            PageLeftButton.Enable();

        if (_cursor.NumberOfPages > 1)
        {
            if (_pagingHolderCanvas.alpha == 0.0f)
                _pagingHolderCanvas.DOFade(1.0f, 0.3f);
            ResetPaging();
            for (int x = 0; x < _cursor.NumberOfPages; x++)
            {
                GameObject pageIndicator = (GameObject)Instantiate(PageIndicator);
                pageIndicator.transform.SetParent(_paging.transform, false);
                _pageIndicators.Add(pageIndicator.GetComponent<PageIndicatorController>());

                if (_cursor.PageIndex == x)
                    _pageIndicators[x].TurnPageOn();
            }
        }
        else
        {
			PageRightButton.Hide ();
			PageLeftButton.Hide ();

			if (_pagingHolderCanvas.alpha == 1.0f)
			{
				_pagingHolderCanvas.DOFade (0.0f, 0.3f);
			}
        }
    }

    public void ResetPaging()
    {
        foreach (PageIndicatorController pageIndicator in _pageIndicators)
        {
            Destroy(pageIndicator.gameObject);
        }
        _pageIndicators.Clear();
    }

    public enum PageDirection
    {
        Left,
        Right,
        None
    }

    void HandlePageClick(object sender)
    {
		Debug.Log ("*** HandlePageClick ***");
        ButtonControllerReplacement clickedButton = sender as ButtonControllerReplacement;
        EventManager.Instance.ContentTileMenuPagingBtnClickEvent();
        if (clickedButton == PageLeftButton)
        {
            _direction = PageDirection.Left;
        }
        else if (clickedButton == PageRightButton)
        {
            _direction = PageDirection.Right;
        }
        else
        {
            Debug.LogFormat("HandlePageClick called from a non button.");
            return;
        }
        StartCoroutine(MoveToPage(_direction));
    }

    IEnumerator MoveToPage(PageDirection direction)
    {
        for (int x = 0; x < _currentContentTileList.Length; x++)
        {
            if ((direction == PageDirection.Left) && _currentContentTileList[x] != null)
            {
                _currentContentTileList[x].FadeTileOut();
            }
            else if ((direction == PageDirection.Right) && _currentContentTileList[x] != null)
            {
                _currentContentTileList[x].FadeTileOut();
            }
            else
                Debug.Log("race condition in ContentTileMenuController, CurrentInteractible not defined in _reticle");
        }

        if (direction == PageDirection.Left)
        {
            _cursor.MoveToPreviousPage();
        }

        if (direction == PageDirection.Right)
        {
            _cursor.MoveToNextPage();
        }

        if (_currentTileHolder == _tileHolderRectTransform1)
            _currentContentTileList = _contentTileList2;
        else
            _currentContentTileList = _contentTileList1;

        StartCoroutine(StartDrawContentTiles());

        yield return new WaitForSeconds(0.3f);
    }

    public void FadeCurrentTiles()
    {
        for (int x = 0; x < _currentContentTileList.Length; x++)
        {
            if (_currentContentTileList[x] != null)
                _currentContentTileList[x].FadeTileOut();
        }
    }

    void OnBackBtnClickHandler()
    {
        //only do back button actions on home
        if (_cursor.CanMoveBackInHistory)
        {
			string curSceneName = SceneManager.GetActiveScene().name.ToLower();
			//TODO: Unique to NBA due to how cursor is handled on launch - when a user hits 2 items in history, they should exit
			if (curSceneName.IndexOf ("home") != -1 && _cursor.GetHistoryLength() <= _minHistoryLength)
			{
				QuitApplication ();
			}else{
				MoveCursorBackInHistory();
			}
        }
        else
        {
			QuitApplication ();
        }
    }

	void QuitApplication()
	{
		//TODO Revisit later for cleanup
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_STANDALONE || UNITY_WSA
		Application.Quit();
		#elif UNITY_ANDROID
		OVRManager.instance.ReturnToLauncher();
		#endif
	}

    public void CheckAndMoveBackInHistory()
    {
        if (_cursor != null)
        {
            if (_cursor.CanMoveBackInHistory)
                MoveCursorBackInHistory();
        }
    }

    void MoveCursorBackInHistory()
    {
        FadeCurrentTiles();
        _cursor.MoveBackInHistory();
        ResetPaging();
        StartCoroutine(StartDrawContentTiles());
    }

    IEnumerator CleanupOldTiles()
    {
        yield return new WaitForSeconds(_animationDuration);
        if (_currentTileHolder == _tileHolderRectTransform1)
        {
            foreach (Transform tile in TileHolder1.transform)
            {
                Destroy(tile.gameObject);
            }
        }
        else
        {
            foreach (Transform tile in TileHolder2.transform)
            {
                Destroy(tile.gameObject);
            }
        }
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _canvasGroup.DOFade(1.0f, 0.3f);
    }

    public void Hide()
    {
        _canvasGroup.DOFade(0.0f, 0.3f).OnComplete(Disable);
    }

    void OnEnable()
    {
        //PageLeftButton.OnButtonClicked += HandlePageClick;
        //PageRightButton.OnButtonClicked += HandlePageClick;
        EventManager.OnControllerSwipe += EventManager_OnControllerSwipe;
        DataCursorComponent.Instance.Updated += OnMenuUpdated;
        if (GlobalBackAffectsMenu)
		{
			EventManager.OnBackBtnClickEvent += OnBackBtnClickHandler;
		}
    }

    void OnDisable()
    {
        //PageLeftButton.OnButtonClicked -= HandlePageClick;
        //PageRightButton.OnButtonClicked -= HandlePageClick;
        EventManager.OnControllerSwipe -= EventManager_OnControllerSwipe;
        if (!DataCursorComponent.ApplicationIsQuitting)
            DataCursorComponent.Instance.Updated -= OnMenuUpdated;
        if (GlobalBackAffectsMenu)
		{
			EventManager.OnBackBtnClickEvent -= OnBackBtnClickHandler;
		}
    }

    /// <summary>
    /// Handle swipes from our GVRSwipe script - could combine OVRTouchpad controls into same script as well
    /// </summary>
    /// <param name="swipeDirection">Swipe direction - "left" or "right".</param>
    void EventManager_OnControllerSwipe(string swipeDirection)
    {
        if (swipeDirection == "left")
        {
            if (_cursor.HasPreviousPage)
            {
                _direction = PageDirection.Left;
                StartCoroutine(MoveToPage(PageDirection.Left));
            }
        }
        else if (swipeDirection == "right")
        {
            if (_cursor.HasNextPage)
            {
                _direction = PageDirection.Right;
                StartCoroutine(MoveToPage(PageDirection.Right));
            }
        }
    }

#if !UNITY_WSA_10_0
    private void OVRTouchpad_TouchHandler(object sender, EventArgs e)
    {
        var args = (OVRTouchpad.TouchArgs)e;
        switch (args.TouchType)
        {
            case OVRTouchpad.TouchEvent.Left:
                if (_cursor.HasPreviousPage)
                {
                    _direction = PageDirection.Left;
                    StartCoroutine(MoveToPage(PageDirection.Left));
                }
                break;

            case OVRTouchpad.TouchEvent.Right:
                if (_cursor.HasNextPage)
                {
                    _direction = PageDirection.Right;
                    StartCoroutine(MoveToPage(PageDirection.Right));

                }
                break;

            default:
                break;
        }
    }
#endif
}