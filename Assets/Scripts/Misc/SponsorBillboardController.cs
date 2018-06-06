using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Analytics;
using DG.Tweening;

public class SponsorBillboardController : MonoBehaviour
{
	public delegate void SponsorBillboardInUseAction();
	public event SponsorBillboardInUseAction OnSponsorBillboardInUseEvent;
	public delegate void SponsorBillboardBlackoutInUseAction();
	public event SponsorBillboardBlackoutInUseAction OnSponsorBillboardBlackoutInUseEvent;
	public delegate void SponsorBillboardFinishedAction();
	public event SponsorBillboardFinishedAction OnSponsorBillboardFinishedEvent;

	public JackalopeMediaPlayer SponsorMovie;
	public GameObject BackSignScreen;
	public Interactible CloseButton;
	public Interactible PlayButton;
	public Interactible ScreenPlayButton;
	public Interactible PauseButton;
	public GameObject TwoDPlayer;
	public GameObject TwoDPlayerContainer;
	public GameObject LoadingIndicator;
	[HideInInspector]
	public bool ScreenPositioned = false;
	[HideInInspector]
	public bool ScreenMoved = false;
	[HideInInspector]
	public bool IsPlaying { get{return _playing;} set{} }

	private JackalopeMediaPlayer _mainMovie;
	private TwoDPlayerController _mainMovie2D;
	private Tweener _loadingIndicatorTweener;
	private Vector3 _rotateVector = new Vector3(0.0f, 0.0f, -360.0f);
	private float _percent;
	private bool _hasSponsorMovie {get {return !string.IsNullOrEmpty (SponsorMovie.MoviePath);}}
	private bool _playing = false;
	private bool _controlsEnabled = false;
	private DataCursor _cursor;

	void Awake()
	{
		DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);
		_mainMovie = ResourceManager.Instance.MediaPlayer;
		_mainMovie2D = ResourceManager.Instance.TwoDMediaPlayer;
		PauseButton.gameObject.SetActive(false);
		PlayButton.gameObject.SetActive(false);
		CloseButton.Disable();
		LoadingIndicator.SetActive(false);
	}

	public void Disable()
	{
		PlayButton.gameObject.SetActive(false);
		ScreenPlayButton.gameObject.SetActive(false);
	}

	public void Enable()
	{
		if(_hasSponsorMovie)
		{
			ScreenPlayButton.gameObject.SetActive(true);
		}
	}

	void OnPlayButtonClick()
	{
		if(!_playing && !_controlsEnabled)
		{
			PlayButton.gameObject.transform.position = PauseButton.gameObject.transform.position;
			CloseButton.gameObject.SetActive(false);
			ScreenPlayButton.gameObject.SetActive(false);
			PlayButton.gameObject.SetActive(false);
			ScreenPlayButton.gameObject.SetActive(false);
			PauseButton.gameObject.SetActive(false);
			if(_mainMovie != null && _cursor.CurrentVideo.Type == ContentType.Clip)
			{
				_percent = (float)_mainMovie.Position / (float)_mainMovie.Length;
				_mainMovie.Stop();
			}
			else if(_mainMovie2D != null && _cursor.CurrentVideo.Type == ContentType.TWO_D_CONSUMPTION)
			{
				_percent = (float)_mainMovie2D.MediaPlayer.Position / (float)_mainMovie2D.MediaPlayer.Length;
				_mainMovie2D.MediaPlayer.Stop();
			}
			TwoDPlayer.SetActive(true);

			if(ScreenPositioned)
			{
				if(ResourceManager.Instance.VRCameraFadeObj != null)
				{
					ResourceManager.Instance.VRCameraFadeObj.FadeCameraOut();
					if(OnSponsorBillboardBlackoutInUseEvent != null)
						OnSponsorBillboardBlackoutInUseEvent();
				}
			}

			if(SceneManager.GetActiveScene().name == "HomeScene")
			{
				if(ScreenPositioned || ScreenMoved)
				{
					TwoDPlayer.layer = LayerMask.NameToLayer("TwoDPlayer");
					Transform[] children = TwoDPlayer.GetComponentsInChildren<Transform>(true);
					for (int i = 0; i < children.Length; i++)
					{
						children[i].gameObject.layer = LayerMask.NameToLayer("TwoDPlayer");
					}
				}
				else
				{
					TwoDPlayer.layer = LayerMask.NameToLayer("BrandPanel");
					Transform[] children = TwoDPlayer.GetComponentsInChildren<Transform>(true);
					for (int i = 0; i < children.Length; i++)
					{
						children[i].gameObject.layer = LayerMask.NameToLayer("BrandPanel");
					}
					CloseButton.transform.localPosition = new Vector3(CloseButton.transform.localPosition.x, ScreenPlayButton.transform.localPosition.y, CloseButton.transform.localPosition.z);
					PlayButton.transform.localPosition = new Vector3(PlayButton.transform.localPosition.x, ScreenPlayButton.transform.localPosition.y, PlayButton.transform.localPosition.z);
					PauseButton.transform.localPosition = new Vector3(PauseButton.transform.localPosition.x, ScreenPlayButton.transform.localPosition.y, PauseButton.transform.localPosition.z);
				}
			}
			SponsorMovie.Play();
			if(OnSponsorBillboardInUseEvent != null)
				OnSponsorBillboardInUseEvent();
			EventManager.Instance.BannerAdStartEvent();
			StartCoroutine("ShowCloseButton");
		}
		else if(_controlsEnabled)
		{
			PauseButton.gameObject.SetActive(true);
			PlayButton.gameObject.SetActive(false);
			SponsorMovie.Play();
		}
		else
		{
			_playing = true;
		}
	}

	void OnPauseButtonClick()
	{
		PlayButton.gameObject.SetActive(true);
		PauseButton.gameObject.SetActive(false);
		SponsorMovie.Pause();
	}

	public void MovieFinished()
	{
		StopCoroutine("ShowCloseButton");

		_playing = false;
		_controlsEnabled = false;
		if(!string.IsNullOrEmpty(SponsorMovie._moviePath))
			ScreenPlayButton.gameObject.SetActive(true);
		CloseButton.gameObject.SetActive(false);
		PauseButton.gameObject.SetActive(false);
		PlayButton.gameObject.SetActive(false);
		PlayButton.gameObject.transform.position = CloseButton.gameObject.transform.position;
		SponsorMovie.ResetPosition();
		SponsorMovie.Stop();
		if(OnSponsorBillboardFinishedEvent != null)
			OnSponsorBillboardFinishedEvent();
		if(_mainMovie != null)
		{
			if(!string.IsNullOrEmpty(_mainMovie._moviePath) && _cursor.CurrentVideo.Type == ContentType.Clip)
			{
				_mainMovie.Play();
				_mainMovie.Position = (long)((_mainMovie.Length - 1) * _percent);
			}
		}

		if(_mainMovie2D != null)
		{
			if(_mainMovie2D.MediaPlayer != null)
			{
				{
					_mainMovie2D.Play();
					_mainMovie2D.MediaPlayer.Position = (long)((_mainMovie2D.MediaPlayer.Length - 1) * _percent);
				}
			}
		}
		if(_mainMovie2D != null)
 		{
			if(_mainMovie2D.MediaPlayer != null)
 			{
				{
					_mainMovie2D.Play();
					_mainMovie2D.MediaPlayer.Position = (long)((_mainMovie2D.MediaPlayer.Length - 1) * _percent);
				}
 			}
 		}
		TwoDPlayer.SetActive(false);
		LoadingIndicator.SetActive(false);
		if(ResourceManager.Instance.VRCameraFadeObj != null) 
			ResourceManager.Instance.VRCameraFadeObj.FadeCameraIn();
	}

	void SponsorMovieReadyToPlay()
	{
		CloseButton.gameObject.SetActive(false);
		PauseButton.gameObject.SetActive(false);
		ScreenPlayButton.gameObject.SetActive(true);
		TwoDPlayerContainer.SetActive(true);
	}

	void SponsorMovieBuffering()
	{
		LoadingIndicator.SetActive(true);
		_loadingIndicatorTweener = LoadingIndicator.transform.DOLocalRotate(_rotateVector, 1.5f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
	}

	void SponsorMoviePlaying()
	{
		_loadingIndicatorTweener.Kill();
		LoadingIndicator.SetActive(false);
	}

	IEnumerator ShowCloseButton()
	{
		yield return new WaitForSeconds(6);
		_controlsEnabled = true;
		CloseButton.gameObject.SetActive(true);
		PauseButton.gameObject.SetActive(true);
	}

	void OnEnable()
	{
		SponsorMovie.OnMoviePathAssigned += SponsorMovieReadyToPlay;
		ScreenPlayButton.OnClick += OnPlayButtonClick;
		PlayButton.OnClick += OnPlayButtonClick;
		PauseButton.OnClick += OnPauseButtonClick;
		CloseButton.OnClick += MovieFinished;
		SponsorMovie.OnEndOfStream += MovieFinished;
		EventManager.OnVideoBufferingEvent += SponsorMovieBuffering;
		EventManager.OnVideoReadyToPlayEvent += SponsorMoviePlaying;
		EventManager.OnTileClickAction += MovieFinished;
		EventManager.OnHomeBtnClickEvent += MovieFinished;
		EventManager.OnBackBtnClickEvent += MovieFinished;
		EventManager.OnFeedbackBtnClickEvent += MovieFinished;
		EventManager.OnContentTileMenuPagingBtnClickEvent += MovieFinished;
	}

	void OnDisable()
	{
		SponsorMovie.OnMoviePathAssigned -= SponsorMovieReadyToPlay;
		ScreenPlayButton.OnClick -= OnPlayButtonClick;
		PlayButton.OnClick -= OnPlayButtonClick;
		PauseButton.OnClick -= OnPauseButtonClick;
		CloseButton.OnClick -= MovieFinished;
		SponsorMovie.OnEndOfStream -= MovieFinished;
		EventManager.OnVideoBufferingEvent -= SponsorMovieBuffering;
		EventManager.OnVideoReadyToPlayEvent -= SponsorMoviePlaying;
		EventManager.OnTileClickAction -= MovieFinished;
		EventManager.OnHomeBtnClickEvent -= MovieFinished;
		EventManager.OnBackBtnClickEvent -= MovieFinished;
		EventManager.OnFeedbackBtnClickEvent -= MovieFinished;
		EventManager.OnContentTileMenuPagingBtnClickEvent -= MovieFinished;
	}
}
