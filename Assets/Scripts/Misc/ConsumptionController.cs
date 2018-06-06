using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VOKE.VokeApp.DataModel;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;
using DG.Tweening;


public class ConsumptionController : MonoBehaviour
{
    public float DelayBeforeLoad = 0.1f;
    public string SceneToLoad = string.Empty;
    public ReticleController Reticle;
    public ReticleController GVRReticleController;
    public GameObject CameraSelectionUI;
    public GameObject WaitCursorUI;
    public GameObject ControlsUI;
    //public GameObject NonGearVrComponentGroup;
    public GamedayScoresController GamedayScoresController;
    //public GameObject ScreenOverlay;
    public ReplayController ReplayContainer;
    public Texture TwoDConsumptionCubemap;
    [Header("Related Content/Highlights")]
    public GameObject RelatedVideos;
    public ContentTileMenuController RelatedContentTileMenu;
    public CanvasGroup PaginationGroup;
    public ButtonController LinearRelatedContentButton;
    //public ButtonController HighlightContentButton; // TODO Uncomment Later
    public ButtonController RelatedContentCloseButton;
    //	[Header("Camera Screen Overlay")]
    //	public GameObject CameraPrefab;
    //	public GameObject CameraOverlayContainer;
    //    public float OverlayCameraDistance = 3.0f;
    public TwoDPlayerController TwoDPlayer;
    public HighlightsController Highlights;
    public PlayerStatsController PlayerStats;
    public float StatsPollingInterval = 10.0f;
    public GameObject RelatedVideoButton;
    public GameObject ForwardButton;
    public GameObject BackButton;
    public GameObject JumpToLive;
    public GameObject HighlightsButton;
    public GameObject ScoresButton;
    public GameObject VideoScreenOverlay;
    public GameObject VideoFrame;
    public GameObject VideoFrameTimewarp;
    public GameObject UnderlayVideo;
    public GameObject UnderlayEnvironment;
    public GameObject[] DebugObjects;

    // rear banners
    public GameObject RearRightFrame;
    public GameObject RearLeftFrame;
    public GameObject RearRightBackSign;
    public GameObject RearLeftBackSign;

    //TODO these objects should be independent and not managed here
    #region Non-Gear VR Objects
    public GameObject DaydreamScreen;
    #endregion

    private VokeAppConfiguration _config;
    private ContentViewModel _Intent;
    private VokeAppConfiguration _data;
    private string _playerStatsEndpoint;
    private DataCursor _cursor;
    private float _ElapsedTimeToCheckNetwork = 0.0f;
    private float _IntervalTimeToCheckNetwork = 10.0f; // 10 seconds
    private int _SavedAntiAliasing;
    private VideoController _VideoController;
    private MessageBoardController _MessageBoardController;
    private bool _WasMounted;
    private bool _isDemoMode = false;
    //private Material _screenOverlayMaterial;
    private CanvasGroup _relatedVideosCanvasGroup;
    private AuthenticationController _Authentication;

    //private List<GameObject> _CameraOverlays = new List<GameObject>();
    private string curCameraUrl;
    private float _consumptionStartTime;
    private AudioController _AudioController;
    private bool _initStatToggle = true;
    private bool _highlightHasPlayed = false;
    private long _mainMoviePosition;
    private Texture _originalBackSignRightImage;
    private Texture _originalBackSignLeftImage;

    private bool backButtonPressed;

    //**************
    // CONSTRUCTOR
    //**************

    void Awake()
    {
        _consumptionStartTime = Time.time;
        _AudioController = Extensions.GetRequired<AudioController>();

        if (GlobalVars.Instance.IsDaydream && GVRReticleController != null)
            Reticle = GVRReticleController;

        #region Analytics Call
        PlayerPrefs.SetFloat("VideoStartTime", Time.time);
        PlayerPrefs.SetFloat("CurrentVideoStartTime", Time.time);
        #endregion

        // Clear the error
        SetLastError("");
        // disabled for now, as it causes problems on MALI chipset devices https://forums.oculus.com/developer/discussion/46673/gear-vr-crash-after-taking-off-headset-and-putting-it-back-on
        // Per Oculus developer on that thread: "For now, I'll suggest to avoid any buffer reallocation operation dynamically on Gear VR ( eg, changing msaa, changing eye buffer size etc )."
        //_SavedAntiAliasing = QualitySettings.antiAliasing;
        //QualitySettings.antiAliasing = 4;
        _VideoController = Extensions.GetRequired<VideoController>();
        _MessageBoardController = Extensions.GetRequired<MessageBoardController>();
        _WasMounted = UserPresenceController.Instance.IsMounted;
        _relatedVideosCanvasGroup = RelatedVideos.GetComponent<CanvasGroup>();
        _Authentication = Extensions.GetRequired<AuthenticationController>();

        DaydreamObjectToggle(DaydreamScreen);

#if (UNITY_ANDROID) && (!UNITY_EDITOR)
        if (!GlobalVars.Instance.IsDaydream && UnityEngine.XR.XRSettings.enabled) // GearVR
		{
			RenderSettings.skybox = null; // disable skybox so OVR underlay can take over
			DynamicGI.UpdateEnvironment();
		}
#endif

        // TODO: Uncomment later
        //if (PaginationGroup == null)
        //    throw new Exception("You must define a PaginationGroup.");

        backButtonPressed = false;

        if (RearRightBackSign != null && RearLeftBackSign != null)
        {
            _originalBackSignRightImage = RearRightBackSign.GetComponentInChildren<Renderer>().material.GetTexture("_MainTex");
            _originalBackSignLeftImage = RearLeftBackSign.GetComponentInChildren<Renderer>().material.GetTexture("_MainTex");
        }

        ControlsUI.SetActive(false);
        WaitCursorUI.SetActive(false);
    }

    IEnumerator Start()
    {
        yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);

        yield return AppConfigurationLoader.Instance.GetDataAsync(data =>
        {
            _data = data;
            PlayerStats.SetTeamImageLocation(data.GetResourceValueByID("NBAStatsPanelTeamImageLocation"));
            _isDemoMode = data.IsDemoMode;
            if (DebugObjects != null)
            {
                bool isDebug = AppConfigurationLoader.Instance.IsLocalFile && !data.IsDemoMode;
                foreach (var o in DebugObjects)
                {
                    o.SetActive(isDebug);
                }
            }
        });

        //Display gameday only for live games
        if (_cursor.CurrentVideo != null && _cursor.CurrentVideo.IsLive)
        {
            if (!String.IsNullOrEmpty(_cursor.CurrentVideo.StatType))
            {
                GamedayScoresController.Init(_cursor.CurrentVideo.StatType);
            }
        }

        _Intent = _cursor.CurrentVideo;

        if (_Intent == null)
            yield break;

        if (_Authentication.RequiresAuthentication(_Intent))
        {
            ReturnToMainMenu();
            yield break;
        }

        if (_Intent.GameID != null)
        {
            if (_data.GetResourceValueByID("NBAPlayerStatsPollingRate") != null && _data.GetResourceValueByID("NBAPlayerStatsPollingRate") != "")
                StatsPollingInterval = float.Parse(_data.GetResourceValueByID("NBAPlayerStatsPollingRate"));
            _playerStatsEndpoint = _data.GetResourceValueByID("NBAPlayerStatsEndpoint") + "?gameId=" + _Intent.GameID;
            InvokeRepeating("UpdateStats", PlayerStats.DelayStartUpTime, StatsPollingInterval);
        }

        if (_Intent.Type == ContentType.Clip)
            Highlights.SetPlayer(_VideoController.MediaPlayer);

        CheckNetworkStatus();
        StartCoroutine(DelayIntroFade());

        //track analytics video type
        string contentType = "VOD";
        if (_Intent.IsLive)
        {
            contentType = "live";
        }
        //else if (_Intent.IsHighlight)
        else if (_Intent.Highlight != null)
        {
            if (_Intent.IsHighlight || _Intent.Highlight.Type == "FREED" || _Intent.Highlight.Type == "TRUEVR")
            {
                contentType = "highlight";
            }
        }

        Analytics.CustomEvent("VideoType", new Dictionary<string, object>
        {
            { "contentType", contentType}
        });

        //check to see if this is the first time a user has visited this tag
        CheckUniqueTagTracking();
        SetUpAndPlay();

        //TODO Uncomment later
        //PaginationGroup.alpha = 1.0f;
    }

    void UpdateStats()
    {
        string statsEndPoint;
#if !UNITY_EDITOR  
        if (GlobalVars.Instance.CurrentTimestamp != null && GlobalVars.Instance.CurrentTimestamp != "")
        {
            statsEndPoint = _playerStatsEndpoint + "&currentTimeUTC=" + GlobalVars.Instance.CurrentTimestamp;
            PlayerStats.StartCoroutine(PlayerStats.GetPlayerStats(statsEndPoint));
        }
        else
        {
            statsEndPoint = _playerStatsEndpoint;
            PlayerStats.StartCoroutine(PlayerStats.GetPlayerStats(statsEndPoint));
        }
#else
        statsEndPoint = _playerStatsEndpoint;
        PlayerStats.StartCoroutine(PlayerStats.GetPlayerStats(statsEndPoint));
#endif
    }

	/// <summary>
	/// Checks to see if this is the first user visit to a tag, also track all tags
	/// </summary>
	void CheckUniqueTagTracking()
	{
		if (_Intent.Tags != null)
		{
			foreach (var tag in _Intent.Tags)
			{
				string tagStr = tag.ToString ();
				string tagName = "tag-" + tagStr;
				string playerPrefTag = PlayerPrefs.GetString (tagName);

				//if we have an existing tag, see if enough time has passed to track again
				if (!String.IsNullOrEmpty (playerPrefTag))
				{
					DateTime oldDate = Convert.ToDateTime (playerPrefTag);
					if (oldDate.AddHours (12) < DateTime.Now)
					{
						PlayerPrefs.SetString (tagName, DateTime.Now.ToString ());
						TrackUniqueTag (tagStr);
					} else
					{
						//Debug.Log ("not tracking tag, as not enough time has passed");
					}
				} else
				{
					PlayerPrefs.SetString (tagName, DateTime.Now.ToString ());
					TrackUniqueTag (tagStr);
				}
#region Analytics Call
				Analytics.CustomEvent ("TagViewsTotal", new Dictionary<string, object> {
					{ "TagTitle", tagStr }
				});
#endregion
			}
		} else
		{
			//Debug.Log ("Not tracking tag: no tags defined");
		}
	}

	void TrackUniqueTag(string tagToTrack)
	{
#region Analytics Call
		Analytics.CustomEvent("TagViewsUnique", new Dictionary<string, object> {
			{ "TagTitle", tagToTrack }
		});
#endregion
	}

	IEnumerator DelayIntroFade()
	{
		yield return new WaitForSeconds(1.0f);
		if (ResourceManager.Instance.VRCameraFadeObj != null)
			yield return StartCoroutine(ResourceManager.Instance.VRCameraFadeObj.BeginFadeIn(false));

//		Texture temp = null;
//		//for rear banners, they will hide if there is no Image Url specified in the EnvironmentConfig
//		temp = RearRightBackSign.GetComponentInChildren<Renderer> ().material.GetTexture ("_MainTex");
//		if (temp == _originalBackSignRightImage)
//		{
//			RearRightBackSign.gameObject.SetActive (false);
//			RearRightFrame.gameObject.SetActive (false);
//		}
//		temp = RearLeftBackSign.GetComponentInChildren<Renderer> ().material.GetTexture ("_MainTex");
//		if (temp == _originalBackSignLeftImage)
//		{
//			RearLeftBackSign.gameObject.SetActive (false);
//			RearLeftFrame.gameObject.SetActive (false);
//		}
	}

	void SetUserPresence(bool isUserPresent)
	{
		bool isPlaying;
		if(_Intent.Type == ContentType.Clip)
			isPlaying = _VideoController.IsPlaying;
		else if(_Intent.Type == ContentType.TWO_D_CONSUMPTION)
			isPlaying = TwoDPlayer.MediaPlayer.IsPlaying;
		else
			isPlaying = false;

		if (isPlaying && !isUserPresent)
		{
			if (_Intent.IsLive)
			{
				if(_Intent.Type == ContentType.Clip)
					_VideoController.Stop();
				else if(_Intent.Type == ContentType.TWO_D_CONSUMPTION)
					TwoDPlayer.MediaPlayer.Stop();
			}
			else
			{
				if(_Intent.Type == ContentType.Clip)
					_VideoController.Pause();
				else if(_Intent.Type == ContentType.TWO_D_CONSUMPTION)
					TwoDPlayer.MediaPlayer.Pause();
			}
		}

		if (!isPlaying && isUserPresent)
		{
			if(_Intent.Type == ContentType.Clip)
				_VideoController.Play();
			else if(_Intent.Type == ContentType.TWO_D_CONSUMPTION)
				TwoDPlayer.MediaPlayer.Play();

		}
	}

	void OnEnable()
	{
		EventManager.OnVideoBufferingEvent += ShowWaitCursor;
		EventManager.OnVideoReadyToPlayEvent += HideWaitCursor;
		EventManager.OnBackBtnClickEvent += OnBackBtnClickHandler;
		EventManager.OnHomeBtnClickEvent += OnHomeBtnClickHandler;
		EventManager.OnSwitchCameraEvent += OnCameraSwitchHandler;
		EventManager.OnVideoErrorEvent += OnVideoErrorHandler;
		EventManager.OnVideoEndEvent += OnVideoEndHandler;
		//EventManager.OnAnalyticsStatsGazeEvent += OnAnalyticsStatsGaze;
		EventManager.OnPanelOpenComplete += OnPanelOpenComplete;
		EventManager.OnPanelCloseComplete += OnPanelCloseComplete;
		EventManager.OnStatsToggled += OnStatsToggled;
        EventManager.OnGamedayScoresOpened += OnGamedayScoresOpened;
		if (LinearRelatedContentButton)
			LinearRelatedContentButton.OnButtonClicked += ShowRelatedVideo;
		RelatedContentCloseButton.OnButtonClicked += HideRelatedVideo;
		//TODO Uncomment later
		//		HighlightContentButton.OnButtonClicked += ShowHighlightVideo;
		EventManager.OnBannerAdStart += BannerAdStarted;
		EventManager.OnBannerAdStop += BannerAdStopped;
        EventManager.OnPopupShown += OnPopupShown;
		ReplayContainer.OnReplayButtonClickedEvent += ReplayVideo;
		Highlights.OnMarkerClicked += HighlightMarkerClicked;
		TwoDPlayer.OnTwoDPlayerFinishedEvent += OnVideoEndHandler;
		TwoDPlayer.ButtonControllerRelatedContent.OnButtonClicked += ShowRelatedVideo;
        SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SetTotalConsumptionTime ();
		EventManager.OnVideoBufferingEvent -= ShowWaitCursor;
		EventManager.OnVideoReadyToPlayEvent -= HideWaitCursor;
		EventManager.OnBackBtnClickEvent -= OnBackBtnClickHandler;
		EventManager.OnHomeBtnClickEvent -= OnHomeBtnClickHandler;
		EventManager.OnSwitchCameraEvent -= OnCameraSwitchHandler;
		EventManager.OnVideoErrorEvent -= OnVideoErrorHandler;
		EventManager.OnVideoEndEvent -= OnVideoEndHandler;
		EventManager.OnPanelOpenComplete -= OnPanelOpenComplete;
		EventManager.OnPanelCloseComplete -= OnPanelCloseComplete;
		//EventManager.OnAnalyticsStatsGazeEvent -= OnAnalyticsStatsGaze;
		EventManager.OnStatsToggled -= OnStatsToggled;
        EventManager.OnGamedayScoresOpened -= OnGamedayScoresOpened;
		if (LinearRelatedContentButton)
			LinearRelatedContentButton.OnButtonClicked -= ShowRelatedVideo;
		RelatedContentCloseButton.OnButtonClicked -= HideRelatedVideo;
		//TODO Uncomment later
		//		HighlightContentButton.OnButtonClicked -= ShowHighlightVideo;
		EventManager.OnBannerAdStart -= BannerAdStarted;
		EventManager.OnBannerAdStop -= BannerAdStopped;
        EventManager.OnPopupShown -= OnPopupShown;
		ReplayContainer.OnReplayButtonClickedEvent -= ReplayVideo;
		Highlights.OnMarkerClicked += HighlightMarkerClicked;
		TwoDPlayer.OnTwoDPlayerFinishedEvent -= OnVideoEndHandler;
		TwoDPlayer.ButtonControllerRelatedContent.OnButtonClicked -= ShowRelatedVideo;
        SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (PlayerPrefs.GetInt("FeedbackSystemDisabled", 0) != 1)
			FeedbackSystemController.Instance.CheckFeedbackFormState(false);
	}

	void SetUpAndPlay()
	{
		if(_Intent.Type == ContentType.Clip)
		{
			TwoDPlayer.MediaPlayer.Stop();
			TwoDPlayer.Hide();

			ShowWaitCursor();
			ShowVideoControllerButtons();
			_VideoController.SetMoviePath(_Intent.Streams.FirstOrDefault().Url);

			_VideoController.Play();
		}
		else if(_Intent.Type == ContentType.TWO_D_CONSUMPTION)
		{
			//When video type is 2DConsumption, we want to remove the video screen from timewarp and only render
			//the environment but with a different cubemap

			ResourceManager.Instance.TimeWarpController.overlays[1].gameObject.SetActive(false);
			OVROverlay environmentOverlay = ResourceManager.Instance.TimeWarpController.overlays[0];
			for (int i = 0; i < environmentOverlay.textures.Length; i++)
			{
				environmentOverlay.textures[i] = TwoDConsumptionCubemap;
			}

			if (GlobalVars.Instance.IsDaydream)
				DaydreamScreen.SetActive(false);

			HideWaitCursor();
			HideVideoControllerButtons();
			_VideoController.Stop();

			TwoDPlayer.SetMediaUrl(string.Empty);	
			TwoDPlayer.Show();
			TwoDPlayer.SetMediaUrl(_Intent.Streams.FirstOrDefault().Url);
			TwoDPlayer.Play();
		}

		if (_Intent.IsHighlight && PlayerPrefs.GetFloat ("mainMovieLastPosition", 0.0f) != 0.0f && PlayerPrefs.GetInt ("highlightPlayed", 0) == 0) 
		{
			RelatedVideoButton.SetActive (false);
			ForwardButton.SetActive (false);
			BackButton.SetActive (false);
			JumpToLive.SetActive (false);
			HighlightsButton.SetActive (false);
			ScoresButton.SetActive (false);
			PlayerPrefs.SetInt ("highlightPlayed", 1);
		}
		else if(PlayerPrefs.GetFloat("mainMovieLastPosition", 0.0f) != 0.0f && PlayerPrefs.GetInt("highlightPlayed", 0) == 1)
		{
			RestartMovie ();
		}
	}

	private void OnApplicationQuit()
	{
#region Analytics Call
		SetTotalConsumptionTime();
		float sessionTime = Time.time - PlayerPrefs.GetFloat("StartTime");
		float timeInConsumption = PlayerPrefs.GetFloat ("TotalConsumptionTime");
		Analytics.CustomEvent("SessionDuration", new Dictionary<string, object>
		{
			{ "TotalTimeInConsumption", timeInConsumption},
			{ "TotalTimeInApp", sessionTime }
		});

		float videoTime = Time.time - PlayerPrefs.GetFloat("VideoStartTime");

		Analytics.CustomEvent("TimeWatchingVideo", new Dictionary<string, object>
		{
			{ "VideoId", _Intent.ID },
			{ "TimeInSeconds", videoTime },
		});
        //Kochava Analytics
        //if(_Intent.IsTracked)
        //{
        //  float endTime = Time.time - _consumptionStartTime;
        //  if(_Intent.IsLive)
        //  {
        //    // Debug.Log("live-trackingClip-"+_Intent.CaptionLine1.ToLower()+"-"+_Intent.CaptionLine2.ToLower()+"..."+endTime);
        //    Tracker.SendEvent("live-trackingClip-"+_Intent.CaptionLine1.ToLower()+"-"+_Intent.CaptionLine2.ToLower(),endTime.ToString());
        //  }
        //  else
        //  {
        //    // Debug.Log("vod-trackingClip-"+_Intent.CaptionLine1.ToLower()+"-"+_Intent.CaptionLine2.ToLower()+"..."+endTime);
        //    Tracker.SendEvent("vod-trackingClip-"+_Intent.CaptionLine1.ToLower()+"-"+_Intent.CaptionLine2.ToLower(),endTime.ToString());
        //  }
        //}
#endregion

		//In case headset is removed directly after waching video
		int mins = (int) (videoTime / 60f);
		PlayerPrefs.SetInt("LastVideoDuration", mins);

		EventManager.Instance.ChannelTriggerEvent();

		AnalyticsTrackCameraTime ();

		if (GlobalVars.Instance.feedbackPopupShown)
			Application.OpenURL("https://www.surveymonkey.com/r/truevr");
	}

	void Update()
	{
		// Check the internet connectivity every 10 seconds
		_ElapsedTimeToCheckNetwork += Time.deltaTime;
		if (_ElapsedTimeToCheckNetwork > _IntervalTimeToCheckNetwork)
		{
			CheckNetworkStatus();
			_ElapsedTimeToCheckNetwork = 0.0f;
		}

		CheckUserPresence();
	}

	/// <summary>
	/// track time spent watching a specific camera
	/// </summary>
	void AnalyticsTrackCameraTime()
	{
#region Analytics Call
		if (!String.IsNullOrEmpty(curCameraUrl))
		{
			//if we're switching from a previous camera, track our time
			float videoTime = Time.time - PlayerPrefs.GetFloat("CurrentVideoStartTime");
			Analytics.CustomEvent("TimeWatchingCamera", new Dictionary<string, object>
			{
				{ "CameraUrl", curCameraUrl },
				{ "TimeInSeconds", videoTime }
			});
		}
		PlayerPrefs.SetFloat("CurrentVideoStartTime", Time.time);
#endregion
	}

	void CheckUserPresence()
	{
		bool isMounted = UserPresenceController.Instance.IsMounted;
		if (isMounted != _WasMounted)
		{
			SetUserPresence(isMounted);
			_WasMounted = isMounted;
		}
	}

	void CheckNetworkStatus()
	{
		StartCoroutine(NetworkUtility.CheckInternetConnection((isConnected) =>
		{
			if (!isConnected && !_isDemoMode)
			{
				_MessageBoardController.SetErrorMessage(GlobalVars.Instance.NoInternet, true);
			}
			else
			{
				//_MessageBoardController.ClearErrorMessage();
			}
		}));
	}

	/// <summary>
	/// Asynchronously start the main scene load.
	/// </summary>
	IEnumerator DelayedSceneLoad()
	{
		//disable our ability to click hardware buttons
		EventManager.Instance.DisableUserClickEvent ();
		// delay one frame to make sure everything has initialized
		yield return 0;
		if(Reticle != null)
			Reticle.AnimReticleOut ();
		yield return new WaitForSeconds(DelayBeforeLoad);

		string sceneToLoad = SceneToLoad;

		if (_cursor.SingleTileMode)
		{
			var scene = SceneManager.GetActiveScene();
			sceneToLoad = scene.name;
		}
		SceneChanger.Instance.FadeToSceneAsync(sceneToLoad, UnderlayVideo, UnderlayEnvironment);
	}

	void ReturnToMainMenu()
	{
		_VideoController.Stop();
		HideVideoControllerButtons();
		HideCameraSelectionButtons();
		//QualitySettings.antiAliasing = _SavedAntiAliasing;
		StartCoroutine(DelayedSceneLoad());
	}

	void SetLastError(string errorMessage)
	{
		PlayerPrefs.SetString("MediaPlayer_LastError", errorMessage);
		PlayerPrefs.Save();
	}

	void HideCameraSelectionButtons()
	{
		if (CameraSelectionUI != null)
		{
			CameraSelectionUI.SetActive(false);
		}
	}

	void HideVideoControllerButtons()
	{
		if (ControlsUI != null)
		{
			ControlsUI.SetActive(false);
		}
	}

	void ShowVideoControllerButtons()
	{
		if (ControlsUI != null)
		{
			ControlsUI.SetActive(true);
		}
	}

	void HideWaitCursor()
	{
		if (WaitCursorUI != null)
		{
			WaitCursorUI.SetActive(false);
		}
	}

	void ShowWaitCursor()
	{
		if (WaitCursorUI != null)
		{
			WaitCursorUI.SetActive(true);
		}
	}

	void ShowCameraSelectionButtons()
	{
		if (CameraSelectionUI != null)
		{
			CameraSelectionUI.SetActive(true);
		}
	}

	/// <summary>
	/// Triggered by the camera icons to change camera angles
	/// </summary>
	/// <param name="mediaUrl">the URL we want to swap to</param>
	private IEnumerator SwitchCameraTo(string mediaUrl)
	{
		yield return 0;

		long? pos = null;
		_VideoController.Stop();

		if (_VideoController.IsPlaying)
		{
			pos = _VideoController.MediaPlayer.Position;
		}

		yield return new WaitForSeconds(0.1f);
		//TODO Uncomment later
		//SetStreamsOverlay(_Intent.Streams.FirstOrDefault(_ => _.Url == mediaUrl));

		_VideoController.SetMoviePath(mediaUrl);

		if (_Intent != null && _Intent.IsLive)
		{
			_VideoController.ResetPosition();
		}
		else if (pos != null)
		{
			_VideoController.MediaPlayer.Position = pos.Value; // restore position on camera switch
		}

		yield return new WaitForSeconds(0.1f);

		_VideoController.Play();
	}

	//TODO Uncomment later
	//    private void SetStreamsOverlay(CameraStreamModel cameraStreamModel)
	//    {
	//        _CameraOverlays.ForEach(_ => Destroy(_));
	//        _CameraOverlays.Clear();
	//
	//        if (cameraStreamModel == null || cameraStreamModel.Links.Count < 1)
	//            return;
	//
	//        var camLocation = Camera.main.transform;
	//
	//        var mesh = ScreenOverlay.GetComponent<MeshFilter>().mesh;
	//
	//        foreach (var pair in cameraStreamModel.Links)
	//        {
	//            var position = UvTo3D(mesh, ScreenOverlay.transform, pair.Key);
	//
	//            if (position == Vector3.zero)
	//            {
	//                Debug.LogWarningFormat("Link for stream {0} on stream {1} is invalid coordinates {2}", pair.Value.ID, cameraStreamModel.ID, pair.Key);
	//                continue;
	//            }
	//
	//            var ray = new Ray(camLocation.position, position);
	//
	//            RaycastHit hit;
	//            if (!Physics.Raycast(ray, out hit))
	//                continue;
	//
	//            position = Vector3.MoveTowards(camLocation.position, position, OverlayCameraDistance);
	//
	//            var obj = Instantiate(CameraPrefab, position, Quaternion.LookRotation(hit.normal)) as GameObject;
	//			obj.transform.SetParent(CameraOverlayContainer.transform, true);
	//
	//            BtnCameraSelectionHandler camSelection = obj.GetComponentInChildren<BtnCameraSelectionHandler>();
	//
	//            var stream = pair.Value;
	//
	//            camSelection.MediaURL = stream.Url;
	//            camSelection.SetLabel(stream.Label);
	//
	//            _CameraOverlays.Add(obj);
	//        }
	//    }

	private static Vector3 UvTo3D(Mesh mesh, Transform root, Vector2 uv)
	{
		int[] tris = mesh.triangles;
		Vector2[] uvs = mesh.uv;

		Vector3[] verts = mesh.vertices;

		for (int i = 0; i < tris.Length; i += 3)
		{
			var u1 = uvs[tris[i]]; // get the triangle UVs
			var u2 = uvs[tris[i + 1]];
			var u3 = uvs[tris[i + 2]];

			// calculate triangle area - if zero, skip it
			var a = Area(u1, u2, u3);
			if (a == 0)
				continue;

			// calculate barycentric coordinates of u1, u2 and u3
			// if anyone is negative, point is outside the triangle: skip it

			var a1 = Area(u2, u3, uv) / a;
			if (a1 < 0)
				continue;

			var a2 = Area(u3, u1, uv) / a;
			if (a2 < 0)
				continue;

			var a3 = Area(u1, u2, uv) / a;
			if (a3 < 0)
				continue;

			// point inside the triangle - find mesh position by interpolation...
			var p3D = a1 * verts[tris[i]] + a2 * verts[tris[i + 1]] + a3 * verts[tris[i + 2]];
			// and return it in world coordinates:
			return root.TransformPoint(p3D);
		}
		// point outside any uv triangle: return Vector3.zero
		return Vector3.zero;
	}

	// calculate signed triangle area using a kind of "2D cross product":
	private static float Area(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		var v1 = p1 - p3;
		var v2 = p2 - p3;
		return (v1.x * v2.y - v1.y * v2.x) / 2;
	}

	void OnBackBtnClickHandler()
	{
		if (_Intent.IsHighlight && _Intent.Parent != null && _Intent.Parent.Type == ContentType.Clip)
		{
			EventManager.Instance.DisableUserClickEvent ();
			_cursor.MoveTo(_Intent.Parent);
			if(Reticle != null)
				Reticle.AnimReticleOut ();
			SceneChanger.Instance.FadeToConsumptionAsync();
		}
		else
		{
			ReturnToMainMenu();
		}
//#region Kochava analytics
//        if(_Intent.IsTracked)
//        {
//          float endTime = Time.time - _consumptionStartTime;
//          if(_Intent.IsLive)
//          {
//            // Debug.Log("live-trackingClip-"+_Intent.CaptionLine1.ToLower()+"-"+_Intent.CaptionLine2.ToLower()+"..."+endTime);
//            Tracker.SendEvent("live-trackingClip-"+_Intent.CaptionLine1.ToLower()+"-"+_Intent.CaptionLine2.ToLower(),endTime.ToString());
//          }
//          else
//          {
//            // Debug.Log("vod-trackingClip-"+_Intent.CaptionLine1.ToLower()+"-"+_Intent.CaptionLine2.ToLower()+"..."+endTime);
//            Tracker.SendEvent("vod-trackingClip-"+_Intent.CaptionLine1.ToLower()+"-"+_Intent.CaptionLine2.ToLower(),endTime.ToString());
//          }
//        }
//#endregion

		backButtonPressed = true;
	}

	void OnCameraSwitchHandler(string mediaUrl)
	{
		if (!string.IsNullOrEmpty (mediaUrl))
		{
			if (curCameraUrl != mediaUrl)
			{
				ShowWaitCursor ();
				StartCoroutine (SwitchCameraTo (mediaUrl));
				AnalyticsTrackCameraTime ();
				curCameraUrl = mediaUrl;
			} else
			{
				if (_Intent != null)
				{
					if (_Intent.IsLive)
					{
						ShowWaitCursor ();
						StartCoroutine (SwitchCameraTo (mediaUrl));
						AnalyticsTrackCameraTime ();
						curCameraUrl = mediaUrl;
					}
				}
			}
		}
	}

	void OnHomeBtnClickHandler()
	{
		_cursor.MoveToHome();
		ReturnToMainMenu();
		backButtonPressed = true;
	}

	void OnVideoErrorHandler(string message)
	{
		_MessageBoardController.SetErrorMessage(GlobalVars.Instance.VideoError, true);
		//ReturnToMainMenu();
	}

	void UpdateScreenOverlayAlpha(float value)
	{
		//_screenOverlayMaterial.color = new Color(0.0f, 0.0f, 0.0f, value);
	}

	void UpdateRelatedVideosCanvasGroupAlphaComplete()
	{
		RelatedContentTileMenu.gameObject.SetActive(false);
		//Debug.Log("Updated Related Canvas Group Alpha");
	}

	void OnVideoEndHandler()
	{
        //Hiding Related content and snapping out stats, so focus is on ReplayContainer
        EventManager.Instance.PanelOpenEvent();
        HideRelatedVideo(null);
		HideVideoControllerButtons ();
		if (_Intent.IsHighlight)
			EventManager.Instance.BackBtnClickEvent();
		else
			ReplayContainer.ShowReplayContainer();
	}

	void HighlightMarkerClicked(long position)
	{
		if (_Intent.Type == ContentType.Clip)
			_VideoController.MediaPlayer.Position = position;
		else if (_Intent.Type == ContentType.TWO_D_CONSUMPTION)
			TwoDPlayer.MediaPlayer.Position = position;
	}

	void TwoDPlayerFinished()
	{
		if(ResourceManager.Instance.VRCameraFadeObj != null)
			ResourceManager.Instance.VRCameraFadeObj.FadeCameraIn();
		TwoDPlayer.MediaPlayer.Stop();
		TwoDPlayer.Hide();
		if(!_highlightHasPlayed)
		{
			HideRelatedVideo(null);
			ReplayContainer.ShowReplayContainer();
		}
		else
		{
			RestartMovie();
			ShowVideoControllerButtons();
			_highlightHasPlayed = false;
		}
	}

	// have to use this way to restart the movie player as setting position after play does not work
	void RestartMovie()
	{
		_mainMoviePosition = (long)PlayerPrefs.GetFloat ("mainMovieLastPosition", 0.0f);
		_VideoController.MediaPlayer.OnReadyToPlay += SetMoviePosition;
		_VideoController.MediaPlayer.Play();
	}

	void SetMoviePosition()
	{
		_VideoController.MediaPlayer.Position = _mainMoviePosition;
		_VideoController.MediaPlayer.OnReadyToPlay -= SetMoviePosition;
	}

	/// <summary>
	/// when leaving the consumption environment, record the time spent in consumption
	/// </summary>
	void SetTotalConsumptionTime()
	{
		float endTime = Time.time - _consumptionStartTime;
		float totalConsumptionTime = PlayerPrefs.GetFloat ("TotalConsumptionTime");
		totalConsumptionTime = totalConsumptionTime + endTime;
		PlayerPrefs.SetFloat ("TotalConsumptionTime", totalConsumptionTime);
	}

	void HideRelatedVideo(object sender)
	{
		_AudioController.PlayAudio(AudioController.AudioClips.ArrowClick);
		_relatedVideosCanvasGroup.DOFade(0.0f, 0.04f).OnComplete(UpdateRelatedVideosCanvasGroupAlphaComplete);
		GlobalVars.Instance.IsAllowControlsOpen = true;
		EventManager.Instance.PanelCloseEvent ();
		if (sender != null && VideoScreenOverlay != null)
		{
			SetVideoOverlay(false);
		}
	}

	void ShowRelatedVideo(object sender)
	{
		_AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
		//if (GlobalVars.Instance.IsAllowControlsOpen)
		//{
		//GlobalVars.Instance.IsAllowControlsOpen = false;
		EventManager.Instance.PanelOpenEvent();
		ReplayContainer.HideReplayContainer();
		// need logic for showing either highlights or related videos
		_relatedVideosCanvasGroup.alpha = 0.0f;
		RelatedContentTileMenu.gameObject.SetActive(true);
		RelatedContentTileMenu.ChangeVideoType(VideoGroup.RelatedVideos);
		_relatedVideosCanvasGroup.DOFade(1.0f, 0.4f);
		if (sender != null && VideoScreenOverlay != null)
		{
			SetVideoOverlay (true);
		}
		//}
	}

	/// <summary>
	/// Shows/Hides the Stats Underlay object
	/// </summary>
	/// <param name="show">whether to show the underlay or not</param>
	void SetVideoOverlay(bool show)
	{
		if (VideoScreenOverlay != null)
			VideoScreenOverlay.SetActive(show);
	}

	void ReplayVideo(object sender)
	{
        //SetUpAndPlay();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void OnAnalyticsStatsGaze(string message)
	{
#region Analytics Call
		Analytics.CustomEvent("GazeStats", new Dictionary<string, object>
		{
			{ "statspanel", message },
			{ "mediaurl", _VideoController.GetMoviePath()}
		});
#endregion
	}

    /// <summary>
    /// Event Handler when Stats are Toggled in or out
    /// </summary>
    /// <param name="toggleState">snappedIn or snappedOut state of Stats</param>
	void OnStatsToggled(string toggleState)
	{
		if (toggleState == "snappedIn")
		{
			HideRelatedVideo (null);
			ReplayContainer.HideReplayContainer ();
		}

		// do not call analytics for our firs stat toggle as it toggles immediately on open
		if (!_initStatToggle)
		{
#region Analytics Call
			Analytics.CustomEvent ("StatButtonClicked", new Dictionary<string, object> {
				{ "toggleState", toggleState },
				{ "mediaurl", _VideoController.GetMoviePath () }
			});
#endregion
		} else
		{
			_initStatToggle = false;
		}
	}

    /// <summary>
    /// Event Handler when Gameday scores are opened
    /// </summary>
    void OnGamedayScoresOpened()
    {
        HideRelatedVideo(null);
        ReplayContainer.HideReplayContainer();
    }

	void OnPanelOpenComplete()
	{
		if (LinearRelatedContentButton != null)
			LinearRelatedContentButton.Disable();

        PopupController.Instance.HidePopup();

		//if (CameraOverlayContainer != null)
		//    CameraOverlayContainer.SetActive(false);
	}
	void OnPanelCloseComplete()
	{
		if (LinearRelatedContentButton != null)
			LinearRelatedContentButton.Enable();

		//if (CameraOverlayContainer != null)
		//    CameraOverlayContainer.SetActive(true);
	}

	void BannerAdStarted()
	{
		HideVideoControllerButtons();
	}

	void BannerAdStopped()
	{
		ShowVideoControllerButtons();
	}

    void OnPopupShown()
    {
        HideRelatedVideo(null);
        ReplayContainer.HideReplayContainer();
    }

    void DaydreamObjectToggle(GameObject obj)
    {
        if (obj != null)
        {
            if (GlobalVars.Instance.IsDaydream)
            {
                obj.SetActive(true);
            }
            else
            {
#if (UNITY_ANDROID) && (!UNITY_EDITOR)
                if ( UnityEngine.XR.XRSettings.enabled )
                {
                obj.SetActive(false);
                }
#else
                obj.SetActive(true);
#endif
            }
        }
        else
        {
            Debug.LogError(obj.name);
        }

        // TODO: Uncomment later
        //if (PaginationGroup == null)
        //    throw new Exception("You must define a PaginationGroup.");

        backButtonPressed = false;
    }

    void OnDestroy()
	{
#region Analytics Call
		float sessionTime = Time.time - PlayerPrefs.GetFloat("VideoStartTime");

		Analytics.CustomEvent("TimeWatchingVideo", new Dictionary<string, object>
		{
			{ "VideoId", _Intent.ID },
			{ "TimeInSeconds", sessionTime },
		});
#endregion

		int mins = (int)(sessionTime / 60f);
		PlayerPrefs.SetInt("LastVideoDuration", mins);

		AnalyticsTrackCameraTime ();
	}
}
