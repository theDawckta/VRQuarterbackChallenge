using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class TwoDPlayerController : MonoBehaviour 
{	
	public delegate void TwoDPlayerInUseAction();
	public event TwoDPlayerInUseAction OnTwoDPlayerInUseEvent;
	public delegate void TwoDPlayerFinishedAction();
	public event TwoDPlayerFinishedAction OnTwoDPlayerFinishedEvent;

	public Camera UICamera;
	public TextMeshProUGUI Timecode;
	public ButtonController ButtonControllerPlay;
	public ButtonController ButtonControllerPause;
	public ButtonController ButtonControllerClose;
	public ButtonController ButtonControllerRelatedContent;
	public ButtonController ButtonControllerStepForward;
	public ButtonController ButtonControllerStepBack;
	public GameObject LoadingIndicator;
	[Header("Scrubber Pieces")]
	public Interactible Scrubber;
	public GameObject SliderLeft;
	public GameObject SliderRight;
	[HideInInspector]
	public JackalopeMediaPlayer MediaPlayer;

	private Tweener _loadingIndicatorTweener;
	private int _videoSkipAmount = 15000;
	private Slider _slider;
	private float _timeStarted;
	private bool _buffering = false;
	private float _timeStartedBuffering;
	private float _timeBuffering = 0.0f;
	private DataCursor _cursor;

	void Awake()
	{
		//DisableInput();
		ButtonControllerPause.gameObject.SetActive(false);
		ButtonControllerClose.gameObject.SetActive(false);
		ButtonControllerRelatedContent.gameObject.SetActive(false);
		_slider = Scrubber.GetComponentInChildren<Slider>();
		MediaPlayer = gameObject.GetComponent<JackalopeMediaPlayer>();
        //We need to play consecutive freeD highligts from the beginning, hence we don't save the last position
        MediaPlayer.SaveLastPosition = false;
		Hide();
		if (UICamera == null)
			throw new Exception("TwoDPlayerController - A UICamera must be defined.");
		DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);
		if(SceneManager.GetActiveScene().name == "ConsumptionScene")
		{
			ButtonControllerClose.gameObject.SetActive(false);
			ButtonControllerRelatedContent.gameObject.SetActive(true);
		}	
		else
		{
			ButtonControllerClose.gameObject.SetActive(true);
			ButtonControllerRelatedContent.gameObject.SetActive(false);
		}	
	}

	void Start()
	{
		LoadingIndicator.gameObject.SetActive(false);
	}

	void OnEnable()
	{
		ButtonControllerPlay.OnButtonClicked += OnPlayButtonClicked;
		ButtonControllerPause.OnButtonClicked += OnPauseButtonClicked;
		ButtonControllerClose.OnButtonClicked += OnCloseButtonClicked;
		ButtonControllerRelatedContent.OnButtonClicked += OnRelatedContentButtonClicked;
		ButtonControllerStepForward.OnButtonClicked += OnStepForwardButtonClicked;
		ButtonControllerStepBack.OnButtonClicked += OnStepBackButtonClicked;
		MediaPlayer.OnPlayVideo += OnMediaPlayed;
		MediaPlayer.OnPauseVideo += OnMediaPaused;
		MediaPlayer.OnEndOfStream += OnMediaEndOfStream;
		MediaPlayer.OnMoviePathAssigned += OnMediaPathAssigned;
		MediaPlayer.OnBuffering += OnMediaBuffering;
		MediaPlayer.OnReadyToPlay += OnMediaReadyToPlay;
		Scrubber.OnClick += OnSliderClick;
	}

	void Update () 
	{
		if (MediaPlayer.IsPlaying)
		{
			string timecodeStr = GetVideoTimecode();
			Timecode.text = timecodeStr;
			_slider.value = GetSliderPosition();
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		if(MediaPlayer != null)
			MediaPlayer.Stop();
	}

	public void Show()
	{
		gameObject.SetActive(true);
	}

	public void SetMediaUrl(string mediaUrl)
	{
		MediaPlayer.MoviePath = mediaUrl;
	}

	public string GetMediaUrl()
	{
		return MediaPlayer.MoviePath;
	}

	public void Play()
	{
		// Have to check if the gameObject is active here or else will throw errors
		if(gameObject.activeSelf)
		{
			MediaPlayer.Play();
			ButtonControllerPlay.Disable();
			ButtonControllerPlay.gameObject.SetActive(false);
			ButtonControllerPause.gameObject.SetActive(true);
			if(OnTwoDPlayerInUseEvent != null)
				OnTwoDPlayerInUseEvent();
		}
	}

	void SeekToOnClick()
    {
		RaycastHit rch;
		GameObject reticle = ResourceManager.Instance.Reticle;
		if (GlobalVars.Instance.IsDaydream)
		{
			reticle = ResourceManager.Instance.ReticleDaydream;
		}

		if (Physics.Raycast(reticle.transform.position, reticle.transform.forward, out rch))
        {
            float sliderLen = -(SliderRight.transform.position.x - SliderLeft.transform.position.x);
            float clickProj = -(rch.point.x  - SliderLeft.transform.position.x);
            float fract = clickProj / sliderLen;

            fract = Mathf.Max(0, fract);
            fract = Mathf.Min(fract, 1.0f);

			MediaPlayer.Position = (long)((MediaPlayer.Length - 1) * fract);
        }
    }

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

	void OnPlayButtonClicked (object sender)
	{
		//EnableInput();
		MediaPlayer.Play();
		ButtonControllerPlay.gameObject.SetActive(false);
		ButtonControllerPause.gameObject.SetActive(true);
	}

	void OnPauseButtonClicked (object sender)
	{
		MediaPlayer.Pause();
		ButtonControllerPlay.gameObject.SetActive(true);
		ButtonControllerPause.gameObject.SetActive(false);
	}

	void OnCloseButtonClicked (object sender)
	{
		MediaPlayer.Stop();
		ButtonControllerPlay.gameObject.SetActive(true);
		ButtonControllerPause.gameObject.SetActive(false);
		if(OnTwoDPlayerFinishedEvent != null)
			OnTwoDPlayerFinishedEvent();
		_buffering = false;
		_timeBuffering = 0.0f;
	}

	void OnRelatedContentButtonClicked (object sender)
	{
		
	}

    void OnStepForwardButtonClicked(object sender)
    {
        if (MediaPlayer.Length > MediaPlayer.Position + _videoSkipAmount)
        {
            long pos = MediaPlayer.Position + (_videoSkipAmount);
            MediaPlayer.Position = pos;
        }
    }

    void OnStepBackButtonClicked(object sender)
    {
        long pos = MediaPlayer.Position - (_videoSkipAmount);
        if (pos < 0)
            pos = 0;

        MediaPlayer.Position = pos;
    }

    void OnMediaPlayed()
	{
		if(_buffering)
		{
			_timeBuffering = _timeBuffering + (Time.time - _timeStartedBuffering);
			_timeStartedBuffering = 0.0f;
			_buffering = false;
		}
		ButtonControllerPlay.gameObject.SetActive(false);
		ButtonControllerPause.gameObject.SetActive(true);
	}

	void OnMediaPaused()
	{
		ButtonControllerPlay.gameObject.SetActive(true);
		ButtonControllerPause.gameObject.SetActive(false);
	}

    void OnMediaEndOfStream()
    {
        ButtonControllerPlay.gameObject.SetActive(true);
        ButtonControllerPause.gameObject.SetActive(false);
        MediaPlayer.Stop();
        if (OnTwoDPlayerFinishedEvent != null)
        {
            OnTwoDPlayerFinishedEvent();
        }
    }

    void OnMediaPathAssigned()
	{
		ButtonControllerPlay.Enable();
		LoadingIndicator.gameObject.SetActive(true);
		OnMediaBuffering();
	}

	void OnMediaBuffering()
	{
		_buffering = true;
		//DisableInput();
		LoadingIndicator.SetActive(true);
		_loadingIndicatorTweener = LoadingIndicator.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, -360.0f), 1.5f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
		_timeStartedBuffering = Time.time;
	}

	void OnMediaReadyToPlay()
	{
		//EnableInput();
		_loadingIndicatorTweener.Kill();
		LoadingIndicator.SetActive(false);
	}

	void OnSliderClick()
    {
        SeekToOnClick();
    }

	void SendAnalytics(float timeStopped)
	{
		float timeWatched = (Time.time - _timeStarted) - _timeBuffering;
		Debug.Log("Analytics: Track Optional Media: " + MediaPlayer._moviePath.ToString() + " watched for " + timeWatched + " seconds.");

		#region Analytics Call
		Analytics.CustomEvent("TimeInOptionalMedia", new Dictionary<string, object>
		{
				{MediaPlayer._moviePath.ToString(),  timeWatched}
		});
		#endregion

		EventManager.Instance.BannerAdStopEvent();
	}

    void DisableInput()
    {
		ButtonControllerPlay.Disable();
		ButtonControllerPause.Disable();
		if(SceneManager.GetActiveScene().name == "ConsumptionScene")
			ButtonControllerRelatedContent.Disable();
		else
			ButtonControllerClose.Disable();
		ButtonControllerStepForward.Disable();
		ButtonControllerStepBack.Disable();
		Scrubber.Disable();
    }

    void EnableInput()
    {
		ButtonControllerPlay.Enable();
		ButtonControllerPause.Enable();
		if(SceneManager.GetActiveScene().name == "ConsumptionScene")
			ButtonControllerRelatedContent.Enable();
		else
			ButtonControllerClose.Enable();
		ButtonControllerStepForward.Enable();
		ButtonControllerStepBack.Enable();
		Scrubber.Enable();
    }

	void OnDisable()
	{
		ButtonControllerPlay.OnButtonClicked -= OnPlayButtonClicked;
		ButtonControllerPause.OnButtonClicked -= OnPauseButtonClicked;
		ButtonControllerClose.OnButtonClicked -= OnCloseButtonClicked;
		ButtonControllerStepForward.OnButtonClicked -= OnStepForwardButtonClicked;
		ButtonControllerStepBack.OnButtonClicked -= OnStepBackButtonClicked;
		MediaPlayer.OnPlayVideo -= OnMediaPlayed;
		MediaPlayer.OnPauseVideo -= OnMediaPaused;
		MediaPlayer.OnEndOfStream -= OnMediaEndOfStream;
		MediaPlayer.OnMoviePathAssigned -= OnMediaPathAssigned;
		MediaPlayer.OnBuffering -= OnMediaBuffering;
		MediaPlayer.OnReadyToPlay -= OnMediaReadyToPlay;
		Scrubber.OnClick -= OnSliderClick;
	}
}
