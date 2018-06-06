using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Analytics;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;

public class ShoulderContentController : MonoBehaviour
{
	public float DelayBeforeLoad = 0.1f;
	public string SceneToLoad = string.Empty;
	public ReticleController Reticle;
	public ReticleController GVRReticleController;
	public JackalopeMediaPlayer MediaPlayer;
	public GameObject WaitCursorUI;
	public VRCameraFade VRCameraFade;
    public VideoController _VideoController;
    public AudioController _AudioController;
    public GameObject ControlsUI;

    public ButtonController ReplayButton;
    public ContentTileController ReplayContentTile;
    public GameObject BtnReplayContainer;
    public ButtonController BtnReplayClose;

    private float _ElapsedTimeToCheckNetwork = 0.0f;
	private float _IntervalTimeToCheckNetwork = 10.0f; // 10 seconds
	private DataCursor _cursor;
	private ContentViewModel _Intent;
	private bool _WasMounted;
	private MessageBoardController _MessageBoardController;

	void Awake()
	{
		_WasMounted = UserPresenceController.Instance.IsMounted;
		_MessageBoardController = Extensions.GetRequired<MessageBoardController>();

		if (GlobalVars.Instance.IsDaydream && GVRReticleController != null)
			Reticle = GVRReticleController;

        HideReplayContainer();
	}

	// Use this for initialization
	IEnumerator Start ()
	{
		yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);

		_Intent = _cursor.CurrentVideo;

		if (_Intent == null)
			yield break;

		var stream = _Intent.Streams.FirstOrDefault();
		MediaPlayer.MoviePath = stream != null ? stream.Url : null;

		ShowWaitCursor();
		CheckNetworkStatus();

		StartCoroutine (DelayIntroFade ());
	}

	IEnumerator DelayIntroFade()
	{
		//yield return new WaitForSeconds(1.0f);
		yield return StartCoroutine(VRCameraFade.BeginFadeIn(false));
	}

	void OnEnable()
	{
		EventManager.OnVideoBufferingEvent += ShowWaitCursor;
		EventManager.OnVideoReadyToPlayEvent += HideWaitCursor;
		EventManager.OnBackBtnClickEvent += OnBackBtnClickHandler;
		EventManager.OnHomeBtnClickEvent += OnHomeBtnClickHandler;
		EventManager.OnVideoErrorEvent += OnVideoErrorHandler;
		EventManager.OnVideoEndEvent += OnVideoEndHandler;
        EventManager.OnPopupShown += OnPopupShown;

		MediaPlayer.OnBuffering += ShowWaitCursor;
		MediaPlayer.OnReadyToPlay += HideWaitCursor;
		MediaPlayer.OnEndOfStream += OnEndOfStream;

        BtnReplayClose.OnButtonClicked += OnReplayButtonCloseClicked;
        ReplayButton.OnButtonClicked += OnReplayButtonClicked;
    }

	void OnDisable()
	{
		EventManager.OnVideoBufferingEvent -= ShowWaitCursor;
		EventManager.OnVideoReadyToPlayEvent -= HideWaitCursor;
		EventManager.OnBackBtnClickEvent -= OnBackBtnClickHandler;
		EventManager.OnHomeBtnClickEvent -= OnHomeBtnClickHandler;
		EventManager.OnVideoErrorEvent -= OnVideoErrorHandler;
		EventManager.OnVideoEndEvent -= OnVideoEndHandler;
        EventManager.OnPopupShown -= OnPopupShown;

        MediaPlayer.OnBuffering -= ShowWaitCursor;
		MediaPlayer.OnReadyToPlay -= HideWaitCursor;
		MediaPlayer.OnEndOfStream -= OnEndOfStream;

        BtnReplayClose.OnButtonClicked -= OnReplayButtonCloseClicked;
        ReplayButton.OnButtonClicked -= OnReplayButtonClicked;
    }

	void Update()
	{
//		if(!JackalopeMediaPLayer.IsPlaying)
//		{
//			ShowWaitCursor();
//		}
//		else
//		{
//			HideWaitCursor();
//		}

		// Check the internet connectivity every 10 seconds
		_ElapsedTimeToCheckNetwork += Time.deltaTime;
		if (_ElapsedTimeToCheckNetwork > _IntervalTimeToCheckNetwork)
		{
			CheckNetworkStatus();
			_ElapsedTimeToCheckNetwork = 0.0f;
		}

		CheckUserPresence();
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
			if (!isConnected)
			{
				_MessageBoardController.SetErrorMessage(GlobalVars.Instance.NoInternet, true);
				MediaPlayer.Pause();
			}
			else
			{
				_VideoController.Play();
			}
		}));
	}

	void HideWaitCursor()
	{
		if (WaitCursorUI != null)
		{
			WaitCursorUI.SetActive(false);
		}
//		JackalopeMediaPLayer.Play();
	}

	void ShowWaitCursor()
	{
		if (WaitCursorUI != null)
		{
			WaitCursorUI.SetActive(true);
		}
	}

	void OnHomeBtnClickHandler()
	{
		_cursor.MoveToHome();
		ReturnToMainMenu();
	}

	void OnVideoErrorHandler(string message)
	{
		_MessageBoardController.SetErrorMessage(GlobalVars.Instance.VideoError, true);
		//ReturnToMainMenu();
	}

	void OnBackBtnClickHandler()
	{
		ReturnToMainMenu();
	}

	void OnVideoEndHandler()
	{
        //ControlsUI.SetActive(false);
        //ShowReplayContainer();
        //TODO Change this to show Replay Container, currently seeing an error where ContenTileController Canvasgroup is null and hence PrepInit throws an error
        ReturnToMainMenu();
	}

	void OnEndOfStream()
	{
		EventManager.Instance.VideoEndEvent();
	}

	void ReturnToMainMenu()
	{
		StartCoroutine(DelayedSceneLoad());
	}

	/// <summary>
	/// Asynchronously start the main scene load.
	/// </summary>
	IEnumerator DelayedSceneLoad()
	{
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

		SceneChanger.Instance.FadeToSceneAsync(sceneToLoad);
	}

	void SetUserPresence(bool isUserPresent)
	{
//		bool isPlaying = _VideoController.IsPlaying;
//		if (isPlaying && !isUserPresent)
//		{
//			if (_Intent.IsLive)
//			{
//				_VideoController.Stop();
//			}
//			else
//			{
//				_VideoController.Pause();
//			}
//		}
//
//		if (!isPlaying && isUserPresent)
//		{
//			_VideoController.Play();
//		}
	}

    void HideReplayContainer()
    {
        EventManager.Instance.PanelCloseEvent();
        //SetVideoOverlay(false);
        BtnReplayContainer.SetActive(false);
        ReplayContentTile.gameObject.SetActive(false);
    }

    void ShowReplayContainer()
    {
        //don't open replay if related or highlights is open
        if (!GlobalVars.Instance.IsAllowControlsOpen)
        {
            //should we have logic to re-open replay once these items are closed?
        }
        else
        {
            BtnReplayContainer.SetActive(true);
            ContentViewModel getNextVideo = _cursor.CurrentVideo.GetWatchNextVideo();

            if (getNextVideo != null)
            {
                ReplayContentTile.gameObject.SetActive(true);
                ReplayContentTile.Init(getNextVideo, _cursor, true);
                //HideRelatedVideo(null);
                //SetVideoOverlay(true);
                EventManager.Instance.PanelOpenEvent();
            }
            else
            {
                ReplayContentTile.gameObject.SetActive(false);
                //SetVideoOverlay(false);
            }
        }

        PopupController.Instance.HidePopup();
    }

    public void OnReplayButtonClicked(object sender)
    {
        _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
        StartCoroutine(ReplayVideo());
    }

    IEnumerator ReplayVideo()
    {
        ControlsUI.SetActive(true);
        _VideoController.ResetPosition();
        yield return new WaitForEndOfFrame();
        HideReplayContainer();
        #region Analytics Call
        Analytics.CustomEvent("ReplayButtonClicked", new Dictionary<string, object>
        {
            { "Operation", "Replay" }
        });
        #endregion
        yield return new WaitForEndOfFrame();
        _VideoController.Play();
    }

    void OnReplayButtonCloseClicked(object sender)
    {
        HideReplayContainer();
    }

    void OnPopupShown()
    {
        HideReplayContainer();
    }
}
