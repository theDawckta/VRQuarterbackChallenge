using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VideoController : MonoBehaviour
{

    [HideInInspector]
    public JackalopeMediaPlayer MediaPlayer;

    [Header("Video Players")]
    public LinearVideoController LinearVideoController;
    public GameObject LinearVideoContainer;
    public GameObject LinearHideShow;

    [HideInInspector]
    public ContentViewModel Intent;
    private DataCursor _cursor;
    private int _VideoSkipAmount = 15000;   //in milliseconds

    private MessageBoardController _MessageBoardController;
    private AuthenticationController _Authentication;
    private VolumeController _VolumeController;
    private bool _initReadyToPlay = true;   //our first ready to play event?
    private Vector3 _linearScale;


    void Awake()
    {
        MediaPlayer = ResourceManager.Instance.MediaPlayer;

        CheckRequired(MediaPlayer, "MediaPlayer");
        CheckRequired(LinearVideoController, "LinearVideoController");
        CheckRequired(LinearVideoContainer, "LinearVideoContainer");
        CheckRequired(LinearHideShow, "LinearHideShow");


        _linearScale = LinearVideoContainer.gameObject.transform.localScale;

        //LinearVideoContainer.gameObject.transform.localScale = new Vector3 (0, 0, 0);

        _VolumeController = Extensions.GetRequired<VolumeController>();
        _MessageBoardController = Extensions.GetRequired<MessageBoardController>();
        _Authentication = Extensions.GetRequired<AuthenticationController>();

        LinearHideShow.SetActive(false);

    }

    private void CheckRequired(object thing, string name)
    {
        if (thing == null)
            throw new Exception(String.Format("A {0} is required to run this scene.", name));
    }

    void OnEnable()
    {
        MediaPlayer.OnPlayVideo += OnPlayVideo;
        MediaPlayer.OnPauseVideo += OnPauseVideo;
        MediaPlayer.OnBuffering += OnBuffering;
        MediaPlayer.OnReadyToPlay += OnReadyToPlay;
        MediaPlayer.OnError += OnError;
        MediaPlayer.OnEndOfStream += OnEndOfStream;
    }

    void OnDisable()
    {
        MediaPlayer.OnPlayVideo -= OnPlayVideo;
        MediaPlayer.OnPauseVideo -= OnPauseVideo;
        MediaPlayer.OnBuffering -= OnBuffering;
        MediaPlayer.OnReadyToPlay -= OnReadyToPlay;
        MediaPlayer.OnError -= OnError;
        MediaPlayer.OnEndOfStream -= OnEndOfStream;
    }

    private IEnumerator Start()
    {
        yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);
        // Get the session data transferred from the previous scene
        Intent = _cursor.CurrentVideo;

        if (Intent == null)
        {
            //string errorMessage = "Media Player Scene fails to get the session data.";
            _MessageBoardController.SetErrorMessage(GlobalVars.Instance.VideoError, true);
            yield break;
        }

		LinearHideShow.SetActive(true);
        yield return _Authentication.SignAnyStreamsAsync(Intent);

        _initReadyToPlay = true;

        MediaPlayer.SetMute(!_VolumeController.IsAudioOn);

        InitLinearUI();
    }

    IEnumerator AllowCameraPivot()
    {
        yield return new WaitForEndOfFrame();
        EventManager.Instance.AllowCameraPivotEvent();
    }

    void InitLinearUI()
    {
        if (MediaPlayer.MoviePath != null)
        {
            EventManager.Instance.CameraSwitchEvent(MediaPlayer.MoviePath);
        }
    }

    /// <summary>
    /// Enable the appropriate video UI
    /// </summary>
    //    void InitVideoUI()
    //    {
    //
    //        LinearVideoController.gameObject.SetActive(Intent.IsLinearCamera);
    //
    //
    //        //TODO: should set player controls not be called until video triggers ready event?
    //        //currently allowing linear camera in specific scenarios
    //        if (Intent.IsLinearCamera)
    //        {
    //            //instantiate camera prefabs if needed
    //            if (Intent.Streams.Count >= 2)
    //            {
    //                LinearVideoController.InstantiateCameraPrefabs();
    //            }
    //            LinearVideoController.Show();
    //            LinearVideoController.SetPlayerControls();
    //            LinearHideShow.SetActive(Intent.IsLinearCamera);
    //        }
    //
    //        if (MediaPlayer.MoviePath != null)
    //        {
    //            EventManager.Instance.CameraSwitchEvent(MediaPlayer.MoviePath);
    //        }
    //    }

    void SetVisibleLinearUI()
    {
        LinearVideoContainer.gameObject.transform.localScale = _linearScale;
    }

    void SetVisibleUI()
    {
        if (Intent.IsLinearCamera)
        {
            LinearVideoContainer.gameObject.transform.localScale = _linearScale;
        }
    }

    //**************
    // METHODS
    //**************

    /// <summary>
    /// Return if media player is playing
    /// </summary>
    /// <returns><c>true</c>, if MediaPlayer returns IsPlaying, <c>false</c> otherwise.</returns>
    public bool IsPlaying
    {
        get { return MediaPlayer.IsPlaying; }
    }

    /// <summary>
    /// Set the path for the video
    /// </summary>
    /// <param name="mediaUrl">Media URL</param>
    public string GetMoviePath()
    {
        return MediaPlayer.MoviePath;
    }

    /// <summary>
    /// Return the slider position as a percentage from 0.0 to 1.0
    /// </summary>
    /// <returns>The slider position.</returns>
    public float GetSliderPosition()
    {
        float p = 0;
        if (MediaPlayer.Length > 0)
        {
            p = MediaPlayer.Position / (1.0f * MediaPlayer.Length);
            p = Mathf.Max(0, p);
            p = Mathf.Min(p, 1.0f);
        }
        return p;
    }

    /// <summary>
    /// Return the time to be displayed within the video timer based on media position
    /// </summary>
    /// <returns>The video timecode.</returns>
    public string GetVideoTimecode()
    {
        long s = MediaPlayer.Position / 1000;
        long h = s / 3600;
        s %= 3600;
        long m = s / 60;
        s %= 60;

        string timecode = string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);
        return timecode;
    }

    /// <summary>
    /// Jump backwards in the video stream X amount
    /// </summary>
	public void JumpBack(bool triggerPlay = false)
    {
        long pos = MediaPlayer.Position - (_VideoSkipAmount / 2);    // AMP units appear to be multiplied by a factor of 2

        if (pos < 0)
            pos = 0;

        MediaPlayer.Position = pos;

        if (triggerPlay)
            StartCoroutine(DelayPlay());
    }

    IEnumerator DelayPlay()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        EventManager.Instance.VideoPositionChangedEvent();
        Play();
    }

    /// <summary>
    /// Jump forward in the video stream X amount
    /// </summary>
    public void JumpForward()
    {
        //only allow us to jump forward if there is enough room
        if (MediaPlayer.IsLive || MediaPlayer.Length > MediaPlayer.Position)
        {
            long pos = MediaPlayer.Position + (_VideoSkipAmount / 2);    // AMP units appear to be multiplied by a factor of 2
            if (pos > MediaPlayer.Length)
                pos = MediaPlayer.Length - 1;
            MediaPlayer.Position = pos;
            //StartCoroutine (DelayPlay ());
        }
    }

    /// <summary>
    /// Pause the media player stream.
    /// </summary>
    public void Pause()
    {
        LinearVideoController.OnPauseVideo();
        MediaPlayer.Pause();
    }

    void OnPauseVideo()
    {
        LinearVideoController.OnPauseVideo();
    }

    /// <summary>
    /// Pause the media player stream from current position.
    /// </summary>
    public void Play()
    {
        OnPlayVideo();
        MediaPlayer.Play();
        EventManager.Instance.VideoPlayEvent();
    }

    void OnPlayVideo()
    {
        LinearVideoController.OnPlayVideo();
    }

    public void ResetPosition()
    {
        MediaPlayer.ResetPosition();
    }

    /// <summary>
    /// Seeks to percentage time.
    /// </summary>
    /// <param name="percent">Percent - 0.0 to 1.0 - the percent of the player to seek to</param>
    public void SeekToPercentageTime(float percent)
    {
        bool IsEndOfVideo = false;
        //are we coming from the end of the video
        if (MediaPlayer.Position >= MediaPlayer.Length - 1000)
            IsEndOfVideo = true;

        MediaPlayer.Position = (long)((MediaPlayer.Length - 1) * percent);
        if (IsEndOfVideo)
            StartCoroutine(DelayPlay());
    }

    //TODO: Need code from voke for how to handle send to live
    /// <summary>
    /// send the video to the currently live feed
    /// </summary>
    public void SendToLive()
    {
        if (Intent.IsLive)
        {
            LinearVideoController.LiveImage.SetActive(true);
        }
        EventManager.Instance.CameraSwitchEvent(MediaPlayer.MoviePath);
    }

    /// <summary>
    /// Set the path for the video
    /// </summary>
    /// <param name="mediaUrl">Media URL</param>
    public void SetMoviePath(string mediaUrl)
    {
        MediaPlayer.MoviePath = mediaUrl;
    }

    /// <summary>
    /// Stop the media player stream
    /// </summary>
    public void Stop()
    {
        LinearVideoController.OnStopVideo();
        MediaPlayer.Stop();
    }

    /// <summary>
    /// Handle the buffering event.
    /// </summary>
    void OnBuffering()
    {
        Debug.Log("VideoController::OnBuffering():Buffering is started.");
        EventManager.Instance.VideoBufferingEvent();
    }

    /// <summary>
    /// Handle the ready to play event.
    /// </summary>
    void OnReadyToPlay()
    {
        Debug.Log("VideoController::OnBuffering():Ready to play.");
        if (_initReadyToPlay)
        {
            _initReadyToPlay = false;
            //SetVisibleUI ();
            SetVisibleLinearUI();
        }
        EventManager.Instance.VideoReadyToPlayEvent();
    }

    void OnEndOfStream()
    {
        EventManager.Instance.VideoEndEvent();
    }

    void OnError(int what, int extra, string message)
    {
        if (message.IndexOf("BehindLiveWindowException") != -1)
        {
            if (MediaPlayer.MoviePath != null)
            {
                EventManager.Instance.CameraSwitchEvent(MediaPlayer.MoviePath);
                LinearVideoController.BtnIsLive.SetActive(false);
            }
        }
        else
        {
            //video error event
            EventManager.Instance.VideoErrorEvent(message);
        }
    }
}