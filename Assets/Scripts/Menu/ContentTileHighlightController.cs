using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRStandardAssets.Utils;
using System.Text.RegularExpressions;
using TMPro;
using DG.Tweening;

public class ContentTileHighlightController : MonoBehaviour
{
	public delegate void OnTileSelectedEvent(ContentTileHighlightController tileClicked);
	public event OnTileSelectedEvent OnTileSelected;

    public TextMeshProUGUI Caption1;
    public TextMeshProUGUI Caption2;
    public Interactible ContentContainer;
    public CanvasGroup CTAButtonContainer;
    public ButtonControllerReplacement CTAButton;
    public Sprite CTAPlayImage;
    public GameObject Border;
	public RawImage BackgroundImage;
	public Texture HighlightBackgroundImage;
	public Texture TrueVRBackgroundImage;
	public Texture FreeDBackgroundImage;
    [HideInInspector]
    public CanvasGroup ContentTileControllerCanvasGroup;
    public CanvasGroup LoadingIndicatorContainer;
    public Image LoadingIndicatorContainerImage;
    public Image LoadingIndicatorImage;
    public bool IsInitColorsOnStartup = false;
    public RawImage ContentImage { get{return _contentImage;} private set{} }
	public String TileHighlightType;
    [HideInInspector]
    public ReticleController Reticle;
    public Vector2 Size;
    public string ContentImageUrl = "http://config.vokevr.com/vokegearvrapp/2017-OBS/TileImages/AlpineSkiing/Tile_Alpine_skiing_mens_alpine_combined.png";
    [HideInInspector]
    public ContentViewModel TileContentViewModel;
    [HideInInspector]
    public bool AllowClicks = true;

    private ButtonControllerReplacement _ctaButton;
    private Transform _loadingIndicator;
    private Image _loadingIndicatorImage;
    private Tweener _loadingIndicatorTweener;
    private RawImage _contentImage;
    private RawImage _borderRawImage;
    private RectTransform _contentTileRectTransform;
    private Texture2D marqueeImage;
    private DataCursor _cursor;
    private AudioController _AudioController;
    private AuthenticationController _Authentication;
    private float _scaleStartSize = 0.75f;
    private float _endScaleSize;
    //how long to wait before loading next scene
    private float sceneLoadDelay = 0.6f;
    private bool _isReplay = false;
    private bool _allowTileFade = true;

    private string authenticationUrl;
    private string registrationCode;
    private int uniqueTileID;

    void Awake()
    {
        _loadingIndicator = LoadingIndicatorContainer.GetComponentInChildren<Transform>();
        ContentTileControllerCanvasGroup = gameObject.GetComponent<CanvasGroup>();
        ContentTileControllerCanvasGroup.alpha = 0;
        _endScaleSize = ContentTileControllerCanvasGroup.transform.localScale.x;
        _contentImage = ContentContainer.GetComponentInChildren<RawImage>();
        _contentImage.DOFade(0.0f, 0.0f);
        _borderRawImage = Border.GetComponent<RawImage>();
        _borderRawImage.DOFade(0.0f, 0.0f);
        _ctaButton = Instantiate(CTAButton, CTAButtonContainer.transform, false);
        _ctaButton.Enable();
        _ctaButton.gameObject.SetActive(true);
        _ctaButton.BackStop.SetActive(false);
        _contentTileRectTransform = gameObject.GetComponent<RectTransform>();
        _contentTileRectTransform.sizeDelta = new Vector2(Size.x, Size.y);
        Caption1.gameObject.SetActive(false);


        _AudioController = Extensions.GetRequired<AudioController>();
        _Authentication = Extensions.GetRequired<AuthenticationController>();

        ContentContainer.ResizeCollider(Size);
        InitColorsOnStartup();
    }

    void Start()
    {
        Vector3 RotateVector = new Vector3(0.0f, 0.0f, -360.0f);
        _loadingIndicatorTweener = _loadingIndicator.transform.DOLocalRotate(RotateVector, 1.5f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
        marqueeImage = new Texture2D(640, 460, TextureFormat.RGB24, true);
        marqueeImage.filterMode = FilterMode.Trilinear;
    }

    IEnumerator DelayTileLoad(float delay)
    {
        float newDelay = 0.2f + (delay * 1.5f);
        yield return new WaitForSeconds(newDelay);
        FadeTileIn();
    }

    public void Init(ContentViewModel tileData, DataCursor cursor)
    {
        _cursor = cursor;
        SetContent(tileData);
        StartCoroutine(DelayTileLoad(0.0f));
    }

    public void Init(ContentViewModel tileData, DataCursor cursor, bool isReplay)
    {
        _cursor = cursor;
        _isReplay = isReplay;
        SetContent(tileData);
        StartCoroutine(DelayTileLoad(0.0f));
    }

    public void Init(ContentViewModel tileData, DataCursor cursor, float delay)
    {
        _cursor = cursor;
        SetContent(tileData);
        StartCoroutine(DelayTileLoad(delay));
    }

    public void Init(ContentViewModel tileData, DataCursor cursor, bool transitionRight, float delay)
    {
        _cursor = cursor;
        SetContent(tileData);
        StartCoroutine(DelayTileLoad(delay));

        if (transitionRight)
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x + 20.0f, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
        else
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x - 20.0f, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
        if (transitionRight)
            transform.DOLocalMoveX(transform.localPosition.x - 20.0f, 0.3f).SetDelay(delay);
        else
            transform.DOLocalMoveX(transform.localPosition.x + 20.0f, 0.3f).SetDelay(delay);
        StartCoroutine(GetPosterTexture(ContentImageUrl));
    }

    void InitColorsOnStartup()
    {
        if (IsInitColorsOnStartup)
        {
            InitColors(GlobalVars.Instance.PrimaryColor, GlobalVars.Instance.SecondaryColor, GlobalVars.Instance.GrayColor, GlobalVars.Instance.HighlightColor);
        }
    }

    public void InitColors(Color _primary, Color _secondary, Color _gray, Color _highlight)
    {
		_ctaButton.Init(Color.white, _primary, _gray, Color.white, _highlight, _gray, _secondary);
        LoadingIndicatorContainerImage.color = _highlight;
        LoadingIndicatorImage.color = _primary;
        _borderRawImage.color = _primary;
        _borderRawImage.DOFade(0.0f, 0.0f);
    }

    private void SetContent(ContentViewModel tileData)
    {
        TileContentViewModel = tileData;

        if (!String.IsNullOrEmpty(tileData.CaptionLine1))
        {
            Caption1.text = tileData.CaptionLine1;
			Caption1.gameObject.SetActive (true);
		}
		else
			Caption1.gameObject.SetActive (false);

		if (!String.IsNullOrEmpty(tileData.CaptionLine2))
		{
			Caption2.text = tileData.CaptionLine2;
			Caption2.color = new Color32(0xe5,0xb4,0x45,0xFF);
		}
		else
			Caption2.gameObject.SetActive (false);

		if (tileData.IsHighlight || (tileData.Highlight.Type == "FREED") || (tileData.Highlight.Type == "TRUEVR"))
		{
			if(tileData.IsHighlight)
				BackgroundImage.texture = HighlightBackgroundImage;
			else if(tileData.Highlight.Type == "FREED")
				BackgroundImage.texture = FreeDBackgroundImage;
			else if(tileData.Highlight.Type == "TRUEVR")
				BackgroundImage.texture = TrueVRBackgroundImage;
		}
		_ctaButton.transform.localScale = new Vector3 (0.7f, 0.7f, 0.7f);
    }

    IEnumerator GetPosterTexture(string imageURL)
    {
		if (!string.IsNullOrEmpty (imageURL))
		{
			using (WWW www = new WWW (imageURL))
			{
				yield return www;
				if (string.IsNullOrEmpty (www.error))
				{
                    var unwrapper = TextureUnwrapperQueue.Instance.CreateTextureUnwrapper(www);
                    yield return unwrapper.WaitForUnwrapCompletion();

                    _contentImage.texture = unwrapper.Texture;
					LoadingIndicatorContainer.DOFade (0.0f, 0.5f);
					_contentImage.DOFade (1.0f, 0.5f);
					yield return new WaitForSeconds (0.5f);
					_loadingIndicatorTweener.Kill ();
				} else
				{
					_loadingIndicatorTweener.Kill ();
					LoadingIndicatorContainer.DOFade (0.0f, 0.5f);
				}
			}
		}
    }

    /// <summary>
    /// Resizes the box collider, if there is one on the object.
    /// </summary>
    public void ResizeTile(Vector2 newSize)
    {
        ContentContainer.ResizeCollider(newSize);
    }

    public void FadeTileIn()
    {
        //scale size should be a percentage of our end scale size
        float scaleSize = _endScaleSize * _scaleStartSize;
        ContentTileControllerCanvasGroup.transform.localScale = new Vector3(scaleSize, scaleSize, scaleSize);
        ContentTileControllerCanvasGroup.alpha = 0;
        ContentTileControllerCanvasGroup.DOFade(1.0f, 0.3f);
        ContentTileControllerCanvasGroup.transform.DOScale(new Vector3(_endScaleSize, _endScaleSize, _endScaleSize), 0.3f);
    }

    public void FadeTileOut(float delay = 0)
    {
        float scaleSize = _endScaleSize * _scaleStartSize;
        ContentTileControllerCanvasGroup.DOFade(0.0f, 0.3f);
        ContentTileControllerCanvasGroup.transform.DOScale(new Vector3(scaleSize, scaleSize, scaleSize), 0.3f);
    }

    private void TileClickedUnique()
    {
        string tileID = TileContentViewModel.ID;
        string tileTitle = TileContentViewModel.CaptionLine1 + ": " + TileContentViewModel.CaptionLine2;

        #region Analytics Call
        Analytics.CustomEvent("TileClickedUnique", new Dictionary<string, object> {
            { "TileTitle", tileTitle },
            { "ContentID", tileID },
        });
        #endregion
    }

    IEnumerator DelayedBrowserTrigger(string urlToOpen)
    {
        yield return new WaitForSeconds(1f);
        Application.OpenURL(urlToOpen);
    }

    void OnGeoCancelButtonClicked()
    {
        _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);

        PopupController.Instance.HidePopup();
    }

    void OnGeoOkButtonClicked()
    {
        _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);

        PopupController.Instance.HidePopup();
    }

    public void DoMove()
    {
        _cursor.MoveTo(TileContentViewModel);
        string tileTitle = TileContentViewModel.CaptionLine1 + ": " + TileContentViewModel.CaptionLine2;

        #region Analytics Call
        Analytics.CustomEvent("TileClicked", new Dictionary<string, object> {
            { "TileTitle", tileTitle },
        });
        #endregion

        ContentViewModel channelToTrack = null;

        if (TileContentViewModel.Type == ContentType.Clip || TileContentViewModel.Type == ContentType.TWO_D_CONSUMPTION)
        {
            #region Analytics Call
            Analytics.CustomEvent("VideoPlayed", new Dictionary<string, object> {
                { "VideoTitle", tileTitle },
                { "VideoId", TileContentViewModel.ID }
            });
            #endregion

            PlayerPrefs.SetString("LastVideoSeen", tileTitle);

            _AudioController.PlayAudio(AudioController.AudioClips.SmallClick);
			channelToTrack = CheckParentageToTrack(TileContentViewModel);
        }
        else if (TileContentViewModel.Type == ContentType.Channel)
        {
            _AudioController.PlayAudio(AudioController.AudioClips.ArrowClick);
			channelToTrack = TileContentViewModel;
        }

        if (channelToTrack != null)
        {
            StartChannelTracking(channelToTrack);
        }
        else
        {
            EventManager.Instance.ChannelTriggerEvent();
        }
    }

    /// <summary>
    /// In order to track channels, we need to be in a channel. This discards tiles that exist at the top level
    /// </summary>
    /// <param name="channelToTrack">Channel to track.</param>
    ContentViewModel CheckParentageToTrack(ContentViewModel channelToTrack)
    {
        if (channelToTrack.Parent != null)
        {
            if (channelToTrack.Parent.Parent != null)
            {
                return FindTileChannelToTrack(TileContentViewModel);
            }
        }

        return null;
    }

    //TODO:Change to check for n levels
    /// <summary>
    /// loop through parents to find the channel we belong to
    /// </summary>
    /// <returns>The tile channel.</returns>
    /// <param name="item">Item.</param>
    ContentViewModel FindTileChannelToTrack(ContentViewModel item)
    {
        if (item.Parent.Parent == null)
        {
            return item;
        }
        return FindTileChannelToTrack(item.Parent);
    }

    /// <summary>
    /// Track the channel we are actually in
    /// </summary>
    /// <param name="channelToTrack">Channel to track.</param>
    void StartChannelTracking(ContentViewModel channelToTrack)
    {
        string channelTitle = "";
        if (!String.IsNullOrEmpty(channelToTrack.CaptionLine1))
            channelTitle = Regex.Replace(channelToTrack.CaptionLine1, @"\s+", "");

        if (!String.IsNullOrEmpty(channelToTrack.CaptionLine2))
            channelTitle = channelTitle + "-" + Regex.Replace(channelToTrack.CaptionLine2, @"\s+", "");

        string channelID = channelToTrack.ID;
        string playerPrefsChannelID = PlayerPrefs.GetString("channelID");
        bool initChannelTracking = true;
        //if there is an existing channel value, we need to log that data. If not, just set
        if (!String.IsNullOrEmpty(playerPrefsChannelID))
        {
            //if we're equal, we don't need to do any tracking
            if (playerPrefsChannelID == channelID)
            {
                initChannelTracking = false;
            }
            else
            {
                EventManager.Instance.ChannelTriggerEvent();
            }
        }

        if (initChannelTracking)
        {
            PlayerPrefs.SetString("channelTitle", channelTitle.ToLower());
            PlayerPrefs.SetString("channelID", channelID);
            PlayerPrefs.SetFloat("channelStartTime", Time.time);
            PlayerPrefs.SetInt("isChannelToTrack", channelToTrack.IsTracked ? 1 : 0);
        }
    }

    /// <summary>
    /// need to disable the users ability to click back via hardware for X amount of time
    /// </summary>
    /// <param name="delayTime">The amount of delay before hardware click is renabled.</param>
    private IEnumerator DisableHardwareBack(float delayTime = 1.0f)
    {
        EventManager.Instance.DisableUserClickEvent();
        yield return new WaitForSeconds(delayTime);
        EventManager.Instance.EnableUserClickEvent();
    }

    private IEnumerator FadeWithDelay()
    {
        EventManager.Instance.DisableUserClickEvent();
        if (Reticle != null)
            Reticle.AnimReticleOut();
        yield return new WaitForSeconds(sceneLoadDelay);
//		EventManager.Instance.LightsOffEvent ();
//		yield return new WaitForSeconds(0.8f);
        yield return SceneChanger.Instance.FadeToConsumptionAsync();
    }

	public void ChangeBackgroundImage(Texture backgroundTexture)
	{
		BackgroundImage.texture = backgroundTexture;
	}

    public void Enable()
    {
        ContentContainer.gameObject.SetActive(true);
        CTAButtonContainer.gameObject.SetActive(true);
    }

    public void Disable()
    {
        ContentContainer.gameObject.SetActive(false);
        CTAButtonContainer.gameObject.SetActive(false);
    }

    public void SetTileFade(bool newValue)
    {
        _allowTileFade = newValue;
    }

    private void HandleImageOver()
    {
        CTAButtonContainer.DOFade(1.0f, 0.3f);
        _borderRawImage.DOFade(1.0f, 0.3f);
    }

    private void HandleImageOut()
    {
        CTAButtonContainer.DOFade(0.0f, 0.3f);
        _borderRawImage.DOFade(0.0f, 0.3f);
    }

    private void HandleImageClick()
    {
        HandleClick();
    }

    private void HandleCTAOver(object sender)
    {
        CTAButtonContainer.DOFade(1.0f, 0.3f);
        _ctaButton.OnButtonOut += HandleCTAOut;
        ContentContainer.OnOut -= HandleImageOut;
    }

    private void HandleCTAOut(object sender)
    {
        CTAButtonContainer.DOFade(1.0f, 0.3f);
        _ctaButton.OnButtonOut -= HandleCTAOut;
        ContentContainer.OnOut += HandleImageOut;
    }

    private void HandleCTAClick(object sender)
    {
        HandleClick();
    }

    private void HandleClick()
    {
        //track unique tile clicks for this user
        string tileID = TileContentViewModel.ID;
        string playerPrefTileID = PlayerPrefs.GetString(tileID);

        //if we have an existing tileID in this user, see if enough time has elapsed to track
        if (!String.IsNullOrEmpty(playerPrefTileID))
        {
            DateTime oldDate = Convert.ToDateTime(playerPrefTileID);

            if (oldDate.AddHours(12) < DateTime.Now)
            {
                PlayerPrefs.SetString(tileID, DateTime.Now.ToString());
                TileClickedUnique();
            }
        }
        else
        {
            PlayerPrefs.SetString(tileID, DateTime.Now.ToString());
            TileClickedUnique();
        }

		OnTileSelected (this);
    }
    private void OnEnable()
    {
		if (ContentContainer != null)
		{
			ContentContainer.OnOver += HandleImageOver;
			ContentContainer.OnOut += HandleImageOut;
			ContentContainer.OnClick += HandleImageClick;
		}
        _ctaButton.OnButtonOver += HandleCTAOver;
        _ctaButton.OnButtonClicked += HandleCTAClick;
    }

    private void OnDisable()
    {
		if (ContentContainer != null)
		{
			ContentContainer.OnOver -= HandleImageOver;
			ContentContainer.OnOut -= HandleImageOut;
			ContentContainer.OnClick -= HandleImageClick;
		}
        _ctaButton.OnButtonOver -= HandleCTAOver;
        _ctaButton.OnButtonClicked -= HandleCTAClick;
    }
}
