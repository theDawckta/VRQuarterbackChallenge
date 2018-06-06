using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public partial class JackalopeMediaPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject _leftRenderingObject;

    [SerializeField]
    private GameObject _rightRenderingObject;

    [SerializeField]
    public string _moviePath;

    [SerializeField]
    private bool _autoPlay;

    [SerializeField]
    private bool _loop;

    private string _cameraName = string.Empty;
    private string _mediaFullPath = string.Empty;
    private bool _startedVideo = false;

    private Texture2D _nativeTexture = null;
    private IntPtr _nativeTexId = IntPtr.Zero;
    private int _textureWidth = 2400;
    private int _textureHeight = 1600;

    public void SetMute(bool mute)
    {
        SetMuteInternal(mute);
    }

    partial void SetMuteInternal(bool mute);

    private Renderer _leftMediaRenderer = null;
    private Renderer _rightMediaRenderer = null;

    private long _savedPosition;
    private bool _saveLastPosition = true;
    private bool _isLive = false;
    private bool _isDebugMode = true;
	private bool _is2DUsingTimeWarp = false;
	private bool _is360UsingTimeWarp = false;

	public delegate void MoviePathAssigned();
	public event MoviePathAssigned OnMoviePathAssigned;

    public delegate void PauseVideoAction();
    public event PauseVideoAction OnPauseVideo;

    public delegate void PlayVideoAction();
	public event PlayVideoAction OnPlayVideo;

    public delegate void BufferingAction();
	public event BufferingAction OnBuffering;

    public delegate void ReadyToPlayAction();
	public event ReadyToPlayAction OnReadyToPlay;

    public delegate void EndOfStreamAction();
	public event EndOfStreamAction OnEndOfStream;

    public delegate void ErrorAction(int what, int extra, string message);
	public event ErrorAction OnError;

    public delegate void UserDefinedTextMetadataAction(string id, string value, string description);
    public event UserDefinedTextMetadataAction OnUserDefinedTextMetadata;

    private enum MediaRendererEventType
    {
        Initialize = 0,
        Shutdown = 1,
        Update = 2,
        Max_EventType
    };

    private enum MediaPlaybackState
    {
        Idle = 1,
        Preparing = 2,
        Buffering = 3,
        Ready = 4,
        Ended = 5,
        Playing = 6,
        Paused = 7,
        Error = 8,
        Unknown = 9
    };

	public bool Is2DUsingTimeWarp 
	{
		set{ _is2DUsingTimeWarp = value; }
		get{ return _is2DUsingTimeWarp; }
	}

	public bool Is360UsingTimeWarp
	{
		set{ _is360UsingTimeWarp = value; }
		get{ return _is360UsingTimeWarp; }
	}

    public string CameraName
    {
        set { _cameraName = value; }
        get { return _cameraName; }
    }

    public string MoviePath
    {
		set 
		{ 
			_moviePath = value; 
			if(OnMoviePathAssigned != null)
				OnMoviePathAssigned(); 
		}
        get { return _moviePath; }
    }

    public bool AutoPlay
    {
        set { _autoPlay = value; }
        get { return _autoPlay; }
    }

    public bool Loop
    {
        set
        {
            _loop = value;
#if (UNITY_IOS) && (!UNITY_EDITOR)
			loopVideo(value);
#endif
        }
        get
        {
            return _loop;
        }
    }

    public bool SaveLastPosition
    {
        set { _saveLastPosition = value; }
    }

    public GameObject LeftRenderingObject
    {
        set { _leftRenderingObject = value; }
        get { return _leftRenderingObject; }
    }

    public GameObject RightRenderingObject
    {
        set { _rightRenderingObject = value; }
        get { return _rightRenderingObject; }
    }

    public bool IsLive
    {
        get
        {
#if (UNITY_IOS) && (!UNITY_EDITOR)
			return isLive();
#else
            return _isLive;
#endif
        }
    }

    public bool IsPlaying
    {
        get
        {
            return (getPlaybackState() == MediaPlaybackState.Playing);
        }
    }

    public long Length
    {
        get
        {
            return getDuration();
        }
    }

    public long Position
    {
        set
        {
            setCurrentPosition(value);
        }
        get
        {
            return getCurrentPosition();
        }
    }

	public int TextureWidth
	{
		set
		{
			_textureWidth = value;
		}
		get
		{
			return _textureWidth;
		}
	}

	public int TextureHeight
	{
		set
		{
			_textureHeight = value;
		}
		get
		{
			return _textureHeight;
		}
	}

	public Texture2D GetNativeTexure()
	{
		return _nativeTexture;
	}

    public void Play()
    {
        LogDebugMessage("Start playing a media.");
        if (_startedVideo)
        {
            if (!IsPlaying)
            {
                playVideo();
            }
        }
        else
        {
            SetupVideoPlayer();
            StartCoroutine(DelayedStartVideo());
        }
    }

    public void Pause()
    {
        LogDebugMessage("Pause a media.");
        if (IsPlaying)
        {
            pauseVideo();
        }
    }

    public void Stop()
    {
        LogDebugMessage("Stop playing a media.");
        stopVideo();
        _startedVideo = false;
        _nativeTexture = null;

        #if !UNITY_EDITOR && !UNITY_STANDALONE
        if (_leftMediaRenderer != null)
        _leftMediaRenderer.material.mainTexture = _nativeTexture;

        if (_rightMediaRenderer != null)
        _rightMediaRenderer.material.mainTexture = _nativeTexture;
        #endif

    }

    public void ResetPosition()
    {
        _savedPosition = -1;
        Position = -1;
    }

    /// <summary>
    /// The start of the numeric range used by event IDs.
    /// </summary>
    /// <description>
    /// If multiple native rundering plugins are in use, the Oculus Media Surface plugin's event IDs
    /// can be re-mapped to avoid conflicts.
    ///
    /// Set this value so that it is higher than the highest event ID number used by your plugin.
    /// Oculus Media Surface plugin event IDs start at eventBase and end at eventBase plus the highest
    /// value in MediaSurfaceEventType.
    /// </description>
    public static int eventBase
    {
        get { return _eventBase; }
        set
        {
            _eventBase = value;
            Native_SetEventBase(_eventBase);
        }
    }
    private static int _eventBase = 0;

    private static void IssuePluginEvent(MediaRendererEventType eventType)
    {

#if UNITY_EDITOR
#elif UNITY_ANDROID

		GL.IssuePluginEvent((int)eventType + eventBase);

#elif UNITY_IOS

		GL.IssuePluginEvent(GetRenderEventFunc(), (int)eventType + eventBase);

#elif UNITY_STANDALONE_WIN
#elif UNITY_STANDALONE_OSX
#else
#endif


    }

    /// <summary>
    /// Initialize the local variables
    /// </summary>
    void Awake()
    {
        _savedPosition = -1;
		#if (!UNITY_ANDROID) || (UNITY_EDITOR)
		Is2DUsingTimeWarp = false;
		#endif
    }

    /// <summary>
    /// Initialize the media player and start playing if the AutoPlay is enabled
    /// </summary>
    void Start()
    {
        LogDebugMessage("Start()");

        if (_autoPlay)
            Play();
    }

    /// <summary>
    /// Update the media surface with the new texture
    /// </summary>
    void Update()
    {
        try
        {
#if UNITY_EDITOR
#elif UNITY_ANDROID

		IntPtr currTexId = Native_GetMediaTextureId();

		if (currTexId != _nativeTexId && _nativeTexture != null)
		{
			_nativeTexId = currTexId;
			_nativeTexture.UpdateExternalTexture(currTexId);
		}

#elif UNITY_IOS
#elif UNITY_STANDALONE_WIN
#elif UNITY_STANDALONE_OSX
#else
#endif

            IssuePluginEvent(MediaRendererEventType.Update);

            OnUpdate();
        }
        catch { }
    }

    partial void OnUpdate();

    void LateUpdate()
    {
        OnLateUpdate();
    }

    partial void OnLateUpdate();

    
    /// <summary>
    /// Pauses video playback when the app loses or gains focus
    /// </summary>
    void OnApplicationPause(bool wasPaused)
    {
        //LogDebugMessage("OnApplicationPause: " + wasPaused);
        //_videoPaused = wasPaused;

        if (wasPaused)
        {
            pauseVideo();
			if (OnPauseVideo != null)
			{
				OnPauseVideo ();
			}
        }
        else
        {
            playVideo();
			if(OnPlayVideo != null)
			{
				OnPlayVideo();
			}
        }
    }
    

	public void CanReInit()
	{
		ReInit();
	}

	partial void ReInit();

    private void OnDestroy()
    {
        if (_startedVideo)
            Stop();
    }

    private void LogDebugMessage(string message)
    {
        if (_isDebugMode)
        {
           // Debug.Log("JackalopeMediaPlayer::" + message, gameObject);
        }
    }

    private void LogErrorMessage(string message)
    {
        Debug.LogError("JackalopeMediaPlayer::" + message, gameObject);
    }

    /// <summary>
    /// Initialize the media surface and texture
    /// </summary>
    private void SetupVideoPlayer()
    {
        Native_InitializeRenderer();

        // Get the left rendering texture
//#if (!UNITY_ANDROID) || (UNITY_EDITOR)
		if(!_is2DUsingTimeWarp)
		{
			if(_leftRenderingObject != null)
			{
				_leftMediaRenderer = _leftRenderingObject.GetComponentInChildren<Renderer>();

				if(_leftMediaRenderer.material == null || _leftMediaRenderer.material.mainTexture == null)
				{
					LogErrorMessage("No material for left movie surface");
				}
			}

			// Get the right rendering texture
			if(_rightRenderingObject != null)
			{
				_rightMediaRenderer = _rightRenderingObject.GetComponentInChildren<Renderer>();
				if(_rightMediaRenderer.material == null || _rightMediaRenderer.material.mainTexture == null)
				{
					LogErrorMessage("No material for right movie surface");
				}
			}
		}
//#endif

        if (_moviePath != string.Empty)
        {
            StartCoroutine(RetrieveStreamingAsset(_moviePath));
        }
        else
        {
            LogErrorMessage("No media file name provided");
        }
//
//		Texture2D tex = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGBA32, false, false);        
//		IntPtr texPtr = tex.GetNativeTexturePtr();
//		_nativeTexture = Texture2D.CreateExternalTexture(_textureWidth, _textureHeight, TextureFormat.RGBA32, false, false, texPtr);

#if !UNITY_EDITOR && !UNITY_STANDALONE

		_nativeTexture = Texture2D.CreateExternalTexture(_textureWidth, _textureHeight, TextureFormat.RGBA32, false, false, new IntPtr(-1));
		_nativeTexture = Resources.Load<Texture2D>("screen_background");
		_nativeTexture.filterMode = FilterMode.Point;
		_nativeTexture.anisoLevel = 9;
		_nativeTexture.wrapMode = TextureWrapMode.Clamp;
#endif

#if (UNITY_IOS) && (!UNITY_EDITOR)
		JRP_InitializeTexture(_nativeTexture.GetNativeTextureID(),_textureWidth,_textureHeight);
		GL.InvalidateState();
#endif

        IssuePluginEvent(MediaRendererEventType.Initialize);
    }

    /// <summary>
    /// Construct the streaming asset path.
    /// Note: For Android, we need to retrieve the data from the apk.
    /// </summary>
    IEnumerator RetrieveStreamingAsset(string mediaFileName)
    {
        if (!mediaFileName.Contains("/"))
        {
            string streamingMediaPath = Application.streamingAssetsPath + "/" + mediaFileName;
            string persistentPath = Application.persistentDataPath + "/" + mediaFileName;
            if (!File.Exists(persistentPath))
            {
                WWW wwwReader = new WWW(streamingMediaPath);
                yield return wwwReader;

                if (wwwReader.error != null)
                {
                    LogErrorMessage("wwwReader error: " + wwwReader.error);
                }

                System.IO.File.WriteAllBytes(persistentPath, wwwReader.bytes);
            }
            _mediaFullPath = persistentPath;
        }
        else if (mediaFileName.ToLower().StartsWith("http:"))
        {
            _mediaFullPath = mediaFileName;
        }
        else
        {
            _mediaFullPath = mediaFileName;
        }
		
        LogDebugMessage("Movie FullPath: " + _mediaFullPath);
    }

    /// <summary>
    /// Auto-starts video playback
    /// </summary>
    IEnumerator DelayedStartVideo()
    {
        yield return null;  // delay 1 frame to allow MediaSurfaceInit from the render thread.

        if (!_startedVideo)
        {
            LogDebugMessage("DelayedStartVideo()");

            _startedVideo = true;

            LogDebugMessage(_mediaFullPath);
            startVideo();
#if !UNITY_EDITOR && !UNITY_STANDALONE
            if (_leftMediaRenderer != null)
			    _leftMediaRenderer.material.mainTexture = _nativeTexture;

            if (_rightMediaRenderer != null)
			    _rightMediaRenderer.material.mainTexture = _nativeTexture;
#endif

            if (_savedPosition != -1)
            {
                Position = _savedPosition;
            }
        }
    }

    #region PLUGIN_INTERFACES
    private static void Native_InitializeRenderer()
    {
#if UNITY_EDITOR
#elif UNITY_ANDROID
            JRP_InitializeRenderer();
#elif UNITY_IOS
#elif UNITY_STANDALONE_WIN
#elif UNITY_STANDALONE_OSX
#else
#endif
    }

    private static void Native_SetEventBase(int eventBase)
    {
#if UNITY_EDITOR
#elif UNITY_ANDROID
            JRP_SetEventBase(eventBase);
#elif UNITY_IOS
#elif UNITY_STANDALONE_WIN
#elif UNITY_STANDALONE_OSX
#else
#endif
    }

    private static IntPtr Native_GetMediaSurfaceObject()
    {
#if UNITY_EDITOR
        return (IntPtr)null;
#elif UNITY_ANDROID
            return JRP_GetMediaSurfaceObject();
#elif UNITY_IOS
            return (IntPtr)null;
#elif UNITY_STANDALONE_WIN
            return (IntPtr)null;
#elif UNITY_STANDALONE_OSX
            return (IntPtr)null;
#else
            return (IntPtr)null;
#endif
    }

    private static IntPtr Native_GetMediaTextureId()
    {
#if UNITY_EDITOR
        return (IntPtr)null;
#elif UNITY_ANDROID
            return JRP_GetMediaSurfaceTextureId();
#elif UNITY_IOS
            return (IntPtr)null;
#elif UNITY_STANDALONE_WIN
            return (IntPtr)null;
#elif UNITY_STANDALONE_OSX
            return (IntPtr)null;
#else
            return (IntPtr)null;
#endif
    }

    private static void Native_SetMediaTextureParam(int texWidth, int texHeight)
    {
#if UNITY_EDITOR
#elif UNITY_ANDROID
           JRP_SetMediaSurfaceTextureParms(texWidth, texHeight);
#elif UNITY_IOS
#elif UNITY_STANDALONE_WIN
#elif UNITY_STANDALONE_OSX
#else
#endif
    }
    #endregion PLUGIN_INTERFACES
}