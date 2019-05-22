using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

// change name to ButtonController after all the old ButtonControllers are gone
public class ButtonControllerReplacement : MonoBehaviour
{
    public enum Colors { None, Primary, Secondary, Highlight };

    public delegate void OnButtonClickedEvent(object sender);
    public event OnButtonClickedEvent OnButtonClicked;
    public delegate void OnButtonOverEvent(object sender);
    public event OnButtonOverEvent OnButtonOver;
    public delegate void OnButtonOutEvent(object sender);
    public event OnButtonOutEvent OnButtonOut;

	public bool pause = false;

    public GameObject Button;
    public GameObject BackStop;
    public Image GazeTimerImage;
    public Image RingImage;
    public Image CircleImage;
    public Image IconImage;
	public TextMeshProUGUI IconText;
    public float OverScale = 1.2f;

    public Colors CircleOverColor;
    public Colors RingOverColor;

    public bool InitOnStart = false;
    public bool MakeSquareButton = false;
    public bool DoOutActionsOnClick = false;
	public float DisabledAlpha = 0.3f;

	public GameObject ActiveHighlight;
	public bool HideSquareImage;

    [HideInInspector]
    public bool IsActive { get; set; }

    private TextMeshProUGUI _iconText;
    private BoxCollider _squareCollider;
    private MeshCollider _circleCollider;
    private Image _squareImage;
    private Color _gazeTimerColor;
    private Color _ringColor;
    private Color _circleColor;
    private Color _iconColor;
    private Color _ringOverColor;
    private Color _circleOverColor;
    private Color _iconOverColor;
    private Canvas _buttonCanvas;
    private CanvasGroup _buttonCanvasGroup;
    private Interactible _interactible;
    private float _gazeTimeInSeconds = 0.0f;
    private const float _defaultFadeTime = 0.3f;
    private Vector3 _restingScaleSize;
    private Vector3 _overScaleSize;
    private Vector3 _pressScaleSize;
    private bool _isAnimating = false;
    private bool _scaleOnGaze = true;
    private AudioController _audioController;

    void Awake()
    {
        _squareCollider = Button.gameObject.GetComponent<BoxCollider>();
        _circleCollider = Button.gameObject.GetComponent<MeshCollider>();
        _restingScaleSize = Button.gameObject.transform.localScale;
        _squareImage = Button.gameObject.GetComponent<Image>();
        _buttonCanvas = Button.gameObject.GetComponent<Canvas>();
        _buttonCanvasGroup = Button.gameObject.GetComponent<CanvasGroup>();
        _interactible = Button.gameObject.GetComponent<Interactible>();
        _iconText = Button.gameObject.GetComponentsInChildren<TextMeshProUGUI>(true).ToList()[0];
        if (GazeTimerImage != null) _gazeTimerColor = GazeTimerImage.color;
        if (CircleImage != null) _circleColor = CircleImage.color;
        if (RingImage != null) _ringColor = RingImage.color;
        if (IconImage != null) _iconColor = IconImage.color;
		if (ActiveHighlight != null)
			ActiveHighlight.SetActive (false);
        //_audioController = Extensions.GetRequired<AudioController>();
        SetGazeTime(_gazeTimeInSeconds);
        //SetDropdownColors();
    }

    void Start()
    {
        GazeTimerImage.fillAmount = 0.0f;
        //_restingScaleSize = Button.gameObject.transform.localScale;
        _overScaleSize = new Vector3(_restingScaleSize.x * OverScale, _restingScaleSize.y * OverScale, _restingScaleSize.z * OverScale);
        if (InitOnStart)
            Enable();
    }

    public void Init()
    {
        Enable();
    }

    /// <summary>
    /// Init the specified gazeTimerColor, circleCircle, ringColor and iconColor.
    /// Use Color.clear for no change
    /// </summary>
    public void Init(Color gazeTimerColor, Color circleColor, Color ringColor, Color iconColor)
    {
        if (GazeTimerImage != null && !gazeTimerColor.Equals(Color.clear)) _gazeTimerColor = gazeTimerColor;
        if (CircleImage != null && !circleColor.Equals(Color.clear)) _circleColor = circleColor;
        if (RingImage != null && !ringColor.Equals(Color.clear)) _ringColor = ringColor;
        if (IconImage != null && !iconColor.Equals(Color.clear)) _iconColor = iconColor;
        Enable();
    }

    /// <summary>
    /// Init the specified gazeTimerColor, circleColor, ringColor, iconColor, circleOverColor, ringOverColor and iconOverColor.
    /// Use Color.clear for no change
    /// </summary>
    public void Init(Color gazeTimerColor, Color circleColor, Color ringColor, Color iconColor, Color circleOverColor, Color ringOverColor, Color iconOverColor)
    {
        if (!gazeTimerColor.Equals(Color.clear)) _gazeTimerColor = gazeTimerColor;
        if (!circleColor.Equals(Color.clear)) _circleColor = circleColor;
        if (!ringColor.Equals(Color.clear)) _ringColor = ringColor;
        if (!iconColor.Equals(Color.clear)) _iconColor = iconColor;
        if (!circleOverColor.Equals(Color.clear)) _circleOverColor = circleOverColor;
        if (!ringOverColor.Equals(Color.clear)) _ringOverColor = ringOverColor;
        if (!iconOverColor.Equals(Color.clear)) _iconOverColor = iconOverColor;
        Enable();
    }

    /// <summary>
    /// Sets enabled visual treatment and disables actions
    /// </summary>
    public void Enable()
    {
        IsActive = true;
        if (_interactible != null) _interactible.Enable();
        BackStop.GetComponent<Collider>().enabled = true;
        if (_buttonCanvasGroup != null) _buttonCanvasGroup.alpha = 1.0f;
        GazeTimerImage.color = _gazeTimerColor;
        if (RingImage != null && _ringColor != Color.clear)
            RingImage.color = _ringColor;
        if (_circleColor != Color.clear)
            CircleImage.color = _circleColor;
        if (_iconColor != Color.clear)
            IconImage.color = _iconColor;
        if (_iconText != null && _iconColor != Color.clear)
            _iconText.color = _iconColor;
        SetGazeTime(_gazeTimeInSeconds);

        if (MakeSquareButton)
        {
            RingImage.gameObject.SetActive(false);
            CircleImage.gameObject.SetActive(false);
            _squareImage.color = _circleColor;
            _squareImage.enabled = true;
            _squareCollider.enabled = true;
            _circleCollider.enabled = false;
            //GazeTimerImage.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
        }

		if (HideSquareImage)
		{
			_squareImage.enabled = false;
		}
    }

    /// <summary>
    /// Set the color of our button based on dropdowns (this can be overwritten by Init)
    /// </summary>
    void SetDropdownColors()
    {
        if (CircleOverColor != Colors.None && CircleImage != null)
        {
            switch (CircleOverColor)
            {
                case Colors.Primary:
                    if (GlobalVars.Instance.PrimaryColor != Color.clear)
                        _circleOverColor = GlobalVars.Instance.PrimaryColor;
                    break;
                case Colors.Secondary:
                    if (GlobalVars.Instance.SecondaryColor != Color.clear)
                        _circleOverColor = GlobalVars.Instance.SecondaryColor;
                    break;
                case Colors.Highlight:
                    if (GlobalVars.Instance.HighlightColor != Color.clear)
                        _circleOverColor = GlobalVars.Instance.HighlightColor;
                    break;
            }
        }

        if (RingOverColor != Colors.None && RingImage != null)
        {
            switch (RingOverColor)
            {
                case Colors.Primary:
                    if (GlobalVars.Instance.PrimaryColor != Color.clear)
                        _gazeTimerColor = GlobalVars.Instance.PrimaryColor;
                    break;
                case Colors.Secondary:
                    if (GlobalVars.Instance.SecondaryColor != Color.clear)
                        _gazeTimerColor = GlobalVars.Instance.SecondaryColor;
                    break;
                case Colors.Highlight:
                    if (GlobalVars.Instance.HighlightColor != Color.clear)
                        _gazeTimerColor = GlobalVars.Instance.HighlightColor;
                    break;
            }
        }
    }

    /// <summary>
    /// Sets the gaze time. Default is 1 second, 0 disables.
    /// </summary>
    public void SetGazeTime(float gazeTimeInSeconds)
    {
        _gazeTimeInSeconds = gazeTimeInSeconds;
        if (gazeTimeInSeconds > 0.0f)
        {
            GazeTimerImage.gameObject.SetActive(true);
            if (_interactible != null)
            {
                _interactible.GazeTime = gazeTimeInSeconds;
                _interactible.UseGazeTime = true;
            }
        }
        else
        {
            GazeTimerImage.gameObject.SetActive(false);
            if (_interactible != null)
            {
                _interactible.GazeTime = 0.0f;
                _interactible.UseGazeTime = false;
            }
        }
    }

    /// <summary>
    /// Sets whether the button scales when moused over or not.
    /// </summary>
    /// <param name="scaleOnGaze">If set to true scale on gaze.</param>
    public void SetScaleOnGaze(bool scaleOnGaze)
    {
        _scaleOnGaze = scaleOnGaze;
    }

    /// <summary>
    /// Sets the icon to text, disables IconImage.
    /// </summary>
    public void SetIcon(string newIconText)
    {
        if (_iconText != null)
        {
            string textStr = newIconText;
            if (string.IsNullOrEmpty(textStr))
                textStr = newIconText;
            textStr = textStr.ToUpper();
            _iconText.text = textStr;
            _iconText.gameObject.SetActive(true);
        }
        IconImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the icon to IconImage, disables text.
    /// </summary>
    /// <param name="iconText">Icon text.</param>
    public void SetIcon(Sprite newIconSprite)
    {
        if (IconImage != null)
            IconImage.sprite = newIconSprite;
    }

    /// <summary>
    /// Sets disabled visual treatment and disables any actions
    /// </summary>
    public void Disable(bool isAlphaDisabled = true)
    {
        IsActive = false;
        if (_interactible != null) _interactible.Disable();
        BackStop.GetComponent<Collider>().enabled = false;
        if (isAlphaDisabled)
        {
            if (_buttonCanvasGroup != null)
				_buttonCanvasGroup.alpha = DisabledAlpha;
        }
    }

	public void Hide()
	{
		if (_buttonCanvasGroup != null)
			_buttonCanvasGroup.alpha = 0.0f;
	}

    public void Highlight(bool isDisable = false)
    {
        HandleOutActions();
		if (ActiveHighlight != null)
		{
			ActiveHighlight.gameObject.SetActive (true);
		} else
		{
			if (!_circleOverColor.Equals (Color.clear))
			{
				CircleImage.color = _circleOverColor;
				if (MakeSquareButton)
				{
					_squareImage.color = _circleOverColor;
				}
			}
		}
        if (!_iconOverColor.Equals(Color.clear))
        {
            IconImage.color = _iconOverColor;
            if (_iconText != null)
                _iconText.color = _iconOverColor;
        }
        if (isDisable)
            Disable(false);
    }

    public void UnHighlight()
    {
        Enable();
        HandleOut();
		if(ActiveHighlight != null)
			ActiveHighlight.gameObject.SetActive (false);
    }

    //need a passthrough action for eventmanager calls
    void DisableAction() { Disable(); }
    public void SetDisabledState() { StartCoroutine(DisabledDelay()); }

    /// <summary>
    /// Call the disable actions - need to wait for end of frame to process initial load
    /// </summary>
    IEnumerator DisabledDelay()
    {
        yield return new WaitForEndOfFrame();
        if (GazeTimerImage != null)
            GazeTimerImage.fillAmount = 0.0f;

        if (!_circleOverColor.Equals(Color.clear))
        {
            CircleImage.color = _circleOverColor;
            if (MakeSquareButton)
                _squareImage.color = _circleOverColor;
        }

        Button.transform.DOScale(_restingScaleSize, 0.3f).SetEase(Ease.OutBack);

        Disable(false);
    }

    public void SetEnabledState()
    {
        if (!_circleColor.Equals(Color.clear)) CircleImage.color = _circleColor;
        Enable();
        HandleOutActions();
    }

	public void SetIconColors(Color iconColor, Color iconOverColor)
	{
		_iconColor = iconColor;
		_iconOverColor = iconOverColor;

		IconImage.color = _iconColor;
        if(_iconText != null)
		    _iconText.color = _iconColor;
	}

    /// <summary>
    /// Fades the button out.
    /// </summary>
    public void FadeButtonOut(float optionalTime = _defaultFadeTime)
    {
        if (_interactible != null)
            _interactible.Disable();
        _isAnimating = true;
        if (_buttonCanvasGroup != null)
            _buttonCanvasGroup.DOFade(0.0f, optionalTime).OnComplete(DoneAnimating);
    }

    /// <summary>
    /// Fades the button in.
    /// </summary>
    public void FadeButtonIn(float optionalTime = _defaultFadeTime)
    {
        _isAnimating = true;
        _buttonCanvasGroup.DOFade(1.0f, optionalTime).OnComplete(DoneAnimating);
        _interactible.Enable();
    }

    public void SetSquareSize(Vector2 size)
    {
        Button.gameObject.GetComponent<RectTransform>().sizeDelta = size;
        _interactible.ResizeCollider(size);
    }

    void HandleOver()
    {
        if (IsActive && !_isAnimating)
        {
            _isAnimating = true;
            _interactible.OnOut += HandleOut;
            _interactible.OnOver -= HandleOver;
            //_buttonCanvas.sortingOrder++;
            if (_interactible.UseGazeTime)
            {
                StartCoroutine("Gazed");
                GazeTimerImage.fillAmount = 0.0f;
                GazeTimerImage.gameObject.SetActive(true);
            }
            if (!_ringOverColor.Equals(Color.clear))
            {
                if (RingImage != null)
                    RingImage.color = _ringOverColor;
            }
            if (!_circleOverColor.Equals(Color.clear))
            {
                CircleImage.color = _circleOverColor;
                if (MakeSquareButton)
                    _squareImage.color = _circleOverColor;
            }
            if (!_iconOverColor.Equals(Color.clear))
            {
                IconImage.color = _iconOverColor;
                if (_iconText != null)
                    _iconText.color = _iconOverColor;
            }
            if (_scaleOnGaze)
            {
                Button.transform.DOScale(_overScaleSize, 0.3f).SetEase(Ease.OutBack).OnComplete(DoneAnimating);
            }
            else
            {
                DoneAnimating();
            }
            if (OnButtonOver != null)
                OnButtonOver(this);
        }
    }

    void ButtonClicked()
    {
		if (IsActive && pause == false)
        {
			StartCoroutine ("PauseTimer");

            if (DoOutActionsOnClick)
                HandleOutActions();
            if (_interactible.UseGazeTime)
            {
                StopCoroutine("Gazed");
                if (GazeTimerImage != null)
                {
                    GazeTimerImage.fillAmount = 0.0f;
                    GazeTimerImage.gameObject.SetActive(false);
                }

            }
            //_audioController.PlayAudio(AudioController.AudioClips.SmallClick);
            PlayPressTween();
            if (OnButtonClicked != null)
                OnButtonClicked(this);
        }
    }


	IEnumerator PauseTimer()
	{
		pause = true;
		yield return new WaitForSeconds (0.2f);
		pause = false;
	}


    void HandleOut()
    {
        if (IsActive)
        {
            HandleOutActions();
        }
    }

    void HandleOutActions()
    {
        _isAnimating = true;
        _interactible.OnOut -= HandleOut;
        _interactible.OnOver += HandleOver;
        //_buttonCanvas.sortingOrder--;
        if (_interactible.UseGazeTime)
        {
            StopCoroutine("Gazed");
            GazeTimerImage.fillAmount = 0.0f;
            GazeTimerImage.gameObject.SetActive(false);
        }
        if (!_circleColor.Equals(Color.clear))
        {
            CircleImage.color = _circleColor;
            if (MakeSquareButton)
                _squareImage.color = _circleColor;
        }
        if (!_ringColor.Equals(Color.clear))
        {
            if (RingImage != null)
                RingImage.color = _ringColor;
        }
        if (!_iconColor.Equals(Color.clear))
        {
            IconImage.color = _iconColor;
            if (_iconText != null)
                _iconText.color = _iconColor;
        }
        if (_scaleOnGaze)
        {
            Button.transform.DOScale(_restingScaleSize, 0.3f).SetEase(Ease.OutBack).OnComplete(DoneAnimating);
        }
        else
        {
            DoneAnimating();
        }
        if (OnButtonOut != null)
            OnButtonOut(this);
    }

    IEnumerator Gazed()
    {
        float timer = 0.0f;
        while (timer <= _gazeTimeInSeconds)
        {
            GazeTimerImage.fillAmount = Mathf.Lerp(0.0f, 1.0f, timer / _gazeTimeInSeconds);
            timer += Time.deltaTime;
            yield return null;
        }
        GazeTimerImage.fillAmount = 1.0f;
        ButtonClicked();
    }

    void DoneAnimating()
    {
        _isAnimating = false;
    }

    void PlayPressTween()
    {
        if (_scaleOnGaze)
        {
            Sequence pressSequence = DOTween.Sequence();
            pressSequence.Append(Button.transform.DOScale(_restingScaleSize, 0.07f));
            pressSequence.Append(Button.transform.DOScale(_overScaleSize, 0.1f));
        }
    }

    void OnEnable()
    {
        _interactible.OnClick += ButtonClicked;
        _interactible.OnOver += HandleOver;
    }

    void OnDisable()
    {
        pause = false;
        if (_interactible.UseGazeTime)
            GazeTimerImage.fillAmount = 0.0f;

        _interactible.OnClick -= ButtonClicked;
        _interactible.OnOver -= HandleOver;
        _interactible.OnOut -= HandleOut;
    }
}