using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonController : MonoBehaviour
{
	public delegate void OnButtonClickedEvent(object sender);
    public event OnButtonClickedEvent OnButtonClicked;
	public delegate void OnButtonOverEvent(object sender);
	public event OnButtonOverEvent OnButtonOver;
	public delegate void OnButtonOutEvent(object sender);
	public event OnButtonOutEvent OnButtonOut;

    public GameObject Button;
	public GameObject ButtonNormal;
    public GameObject ButtonRollover;
	public Image GazeTimerImg;
	public GameObject ButtonIcon;
    public GameObject ButtonIconActive;
    public float ScaleSize = 1.2f;
    public bool IsActive = true;
	public bool PlayClickAudio = true;
	public bool ResizeCollider = true;

    private Interactible _interactible;
    private BoxCollider _buttonCollider;
    private CanvasGroup _canvasGroup;
    private Image _buttonIconImage;
    private float _disabledAlpha = 0.3f;
	private Vector3 _startScaleSize;
    private Vector3 _endScaleSize;
    private Canvas _controllerCanvas;
	private AudioController _audioController;

    void Awake()
    {
        _canvasGroup = Button.gameObject.GetComponent<CanvasGroup>();
        _interactible = Button.gameObject.GetComponent<Interactible>();
		_buttonCollider = Button.gameObject.GetComponent<BoxCollider>();
        _controllerCanvas = GetComponent<Canvas>();
		_audioController = Extensions.GetRequired<AudioController>();
        SetSortingOrder();
    }

    void Start()
    {
		if(ResizeCollider)
			StartCoroutine(SizeBoxCollider());
		if(ButtonNormal != null) ButtonNormal.SetActive(true);
		if(ButtonRollover != null) ButtonRollover.SetActive(false);
		GazeTimerImg.fillAmount = 0.0f;
		_startScaleSize = gameObject.transform.localScale;
		_endScaleSize = new Vector3(_startScaleSize.x * ScaleSize, _startScaleSize.y * ScaleSize, _startScaleSize.z * ScaleSize);
    }

    void OnEnable()
    {
        _interactible.OnClick += ButtonClicked;
		_interactible.OnOver += HandleOver;
        _interactible.OnOut += HandleOut;
        EventManager.OnEnableUserClick += OnEnableUserClick;
        EventManager.OnDisableUserClick += OnDisableUserClick;
    }

    void OnDisable()
    {
		_interactible.OnClick -= ButtonClicked;
		_interactible.OnOver -= HandleOver;
        _interactible.OnOut -= HandleOut;
        EventManager.OnEnableUserClick -= OnEnableUserClick;
        EventManager.OnDisableUserClick -= OnDisableUserClick;
    }

    public void Enable()
    {
		if(_interactible != null) _interactible.Enable();
		if(_canvasGroup != null) _canvasGroup.alpha = 1.0f;
    }

    public void Disable()
    {
		if(_interactible != null) _interactible.Disable();
		if(_canvasGroup != null) _canvasGroup.alpha = _disabledAlpha;
    }

	void ToggleOver(bool isOver)
    {
        if (ButtonNormal != null)
        {
            ButtonNormal.SetActive(!isOver);
        }
        if (ButtonRollover != null)
        {
            ButtonRollover.SetActive(isOver);
        }
    }

	public void SetGazeValue(float gazeValue)
    {
       	GazeTimerImg.fillAmount = gazeValue;
    }

	void OnDisableUserClick()
    {
    	IsActive = false;
		Disable();
    }

    void OnEnableUserClick()
    {
    	IsActive = true;
        Enable();
	}

	private void ButtonClicked()
    {
		if (IsActive)
		{
			if(OnButtonClicked != null)
			{
				OnButtonClicked (this);
				if(PlayClickAudio) _audioController.PlayAudio(AudioController.AudioClips.GenericClick);
			}
		}
    }

	public void HandleOver()
    {
        transform.DOScale(_endScaleSize, 0.3f).SetEase(Ease.OutBack);
        if (IsActive)
        {
            ToggleOver(true);
			if(OnButtonOver != null)
				OnButtonOver (this);
        }
    }

    public void HandleOut()
    {
        if (IsActive)
        {
            ToggleOver(false);
			if(OnButtonOut != null)
				OnButtonOut (this);
        }
		transform.DOScale(_startScaleSize, 0.3f).SetEase(Ease.OutBack);
    }

    public void FadeButtonOut(float time)
    {
		_buttonCollider.enabled = false;
		_canvasGroup.DOFade(0.0f, time);
    }

	public void FadeButtonIn(float time)
    {
		_canvasGroup.DOFade(1.0f, time).OnComplete(FadeButtonInComplete);
	}

	public void EnableBox()
	{
		ToggleOver(false);
		this.gameObject.transform.localScale = _startScaleSize;
		_buttonCollider.enabled = true;
	}

	void FadeButtonInComplete()
	{
		_buttonCollider.enabled = true;
	}

	IEnumerator SizeBoxCollider()
	{
		yield return new WaitForEndOfFrame();
		Vector3 newSize = Button.gameObject.GetComponent<RectTransform> ().sizeDelta;
		_buttonCollider.size = newSize;

	}

    /// <summary>
    /// Toggles the button State from Normal to Active or Vice versa
    /// </summary>
    /// <returns></returns>
    public void ToggleButtonState()
    {
		if(ButtonIconActive != null)
		{
	        if (ButtonIcon.activeSelf)
	        {
	            ButtonIcon.SetActive(false);
	            ButtonIconActive.SetActive(true);
	        }
	        else
	        {
	            ButtonIcon.SetActive(true);
	            ButtonIconActive.SetActive(false);
	        }

	        SetSortingOrder();
        }
    }

    public void SetSortingOrder()
    {
		if(ButtonIconActive != null)
		{
	        if (_controllerCanvas != null)
	        {
	            if (ButtonIcon.activeSelf)
	                _controllerCanvas.sortingOrder = 0; // Button is inactive so bring it in front
	            else
	                _controllerCanvas.sortingOrder = -1; // Button is in active state so push it behind
	        }
        }
    }
}