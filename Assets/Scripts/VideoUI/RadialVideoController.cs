using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Analytics;
using DG.Tweening;

public class RadialVideoController : MonoBehaviour
{

    public JackalopeMediaPlayer MediaPlayer;
    public GameObject VideoControls;        //which game object to animate
    public GameObject PlayDialCover;
    public GameObject PositionBox;
    public Camera UICamera;             //which cameras raycast to look at
    public LayerMask ShowHideLayerMask; //which layer mask should we use to determine hits
    public Interactible ScrubberInteractible;
    public RadialController RadialVideoControls;
    public RadialController RadialCameraIcons;
    public GameObject BtnPlay;
    public GameObject BtnPause;
    public RadialSlider RadialSlider;
    public Text TextTimecode;
    public Text TextLive;
    public LayerMask ScrubberLayerMask;
    public Vector3 ButtonScale;
    [Header("Intel Produced Icons")]
    public RadialItem IntelProducedBtnContainer;
    public BtnRadialCameraSwitch IntelProducedBtn;
    public BtnRadialCameraSwitch UserProducedBtn;
    [Header("Live Icons")]
    public List<RadialItem> LiveIcons;
    [Header("Non-Live Icons")]
    public List<RadialItem> NonLiveIcons;
    [Header("VOD Icons")]
    public List<RadialItem> VODIcons;
    [Header("Highlights")]
    public ButtonController HighlightsButtonController;
    public HighlightGlow HighlightMainGlow;
    public HighlightGlow HighlightButtonGlow;
    [Header("Camera Layouts")]
    public GameObject CameraPrefab;
    public GameObject CameraPrefabContainer;
    public HighlightsProvider HighlightsProviderObj;

    private Transform _cameraTransform;
    private bool _isMenuOpen = false;
    private bool _isAnimating = false;
    private Vector3 _startingPosition;
    private Vector3 _startingRotation;
    private Vector3 _endingPosition;
    private Vector3 _endingRotation;
    private float _animSpeed = 0.3f;
    private VideoController _VideoController;
    private RectTransform _scrubberInteractibleRect;
    private ButtonController _BtnPauseButtonController;
    private AudioController _AudioController;
    private bool _IsHighlightUpdated = false;       //do we need to reflect this highlight in our lower button
    private Vector3 _CurScale = new Vector3(1, 1, 1);
    private string _previousCameraURL = "";
    private ButtonController _playBtnController;


    void Awake()
    {
        CheckRequired(PlayDialCover, "PlayDialCover");
        CheckRequired(UICamera, "UICamera");
        CheckRequired(VideoControls, "VideoControls");
        CheckRequired(PositionBox, "PositionBox");
        CheckRequired(ScrubberInteractible, "ScrubberInteractible");
        CheckRequired(TextTimecode, "TextTimecode");
        CheckRequired(TextLive, "TextLive");
        CheckRequired(RadialSlider, "RadialSlider");
        CheckRequired(HighlightsButtonController, "HighlightsButtonController");
        CheckRequired(HighlightMainGlow, "HighlightMainGlow");
        CheckRequired(HighlightButtonGlow, "HighlightButtonGlow");
        CheckRequired(CameraPrefab, "CameraPrefab");
        CheckRequired(CameraPrefabContainer, "CameraPrefabContainer");
        CheckRequired(RadialCameraIcons, "RadialCameraIcons");
        CheckRequired(HighlightsProviderObj, "HighlightsProviderObj");

        Hide();
        //only for positioning so make sure is hidden
        PositionBox.SetActive(false);
        _cameraTransform = UICamera.transform;
        _startingPosition = VideoControls.transform.localPosition;
        _startingRotation = VideoControls.transform.localEulerAngles;
        _endingPosition = PositionBox.transform.localPosition;
        _endingRotation = PositionBox.transform.localEulerAngles;
        _VideoController = Extensions.GetRequired<VideoController>();
        _scrubberInteractibleRect = ScrubberInteractible.GetComponent<RectTransform>();
        _BtnPauseButtonController = BtnPause.GetComponent<ButtonController>();
        _AudioController = Extensions.GetRequired<AudioController>();
        _playBtnController = BtnPlay.GetComponent<ButtonController>();
        RadialVideoControls.SetBtnScale(ButtonScale);
    }

    void Start()
    {
        if (!HighlightsProviderObj.HasHighlights)
        {
            HighlightsButtonController.Disable();
        }
    }

    private void CheckRequired(object thing, string name)
    {
        if (thing == null)
            throw new Exception(String.Format("A {0} is required to run a RadialController.", name));
    }

    //************
    // METHODS
    //************

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out hit, 200.0f, ShowHideLayerMask) && GlobalVars.Instance.IsAllowControlsOpen)
        {
            //if used to open menu
            if (!_isMenuOpen && !_isAnimating)
            {
                _isMenuOpen = true;
                AnimMenuOpen();

                if (!RadialVideoControls.AllowHit)
                {
                    RadialVideoControls.AllowHit = true;
                }
                if (!RadialCameraIcons.AllowHit)
                {
                    RadialCameraIcons.AllowHit = true;
                }
            }
        }
        else
        {
            //close out the menu if open
            if (_isMenuOpen && !_isAnimating)
            {
                _isMenuOpen = false;
                AnimMenuClosed();
            }

            if (RadialVideoControls.AllowHit)
            {
                RadialVideoControls.AllowHit = false;
            }
            if (RadialCameraIcons.AllowHit)
            {
                RadialCameraIcons.AllowHit = false;
            }
        }
    }

    private void AnimMenuOpen()
    {
		VideoControls.transform.DOLocalMove(_endingPosition, _animSpeed).SetEase(Ease.OutCubic);
		VideoControls.transform.DOLocalRotate(_endingRotation, _animSpeed).SetEase(Ease.OutCubic);

        if (_IsHighlightUpdated)
        {
            _IsHighlightUpdated = false;
            HighlightButtonGlow.AnimScaleUp();
        }
    }

    private void AnimMenuClosed()
    {
		VideoControls.transform.DOLocalMove(_startingPosition, _animSpeed).SetEase(Ease.InCubic);
		VideoControls.transform.DOLocalRotate(_startingRotation, _animSpeed).SetEase(Ease.InCubic);
    }

    void Hide()
    {
        PlayDialCover.SetActive(false);
        _CurScale = this.gameObject.transform.localScale;
        this.gameObject.transform.localScale = new Vector3(0, 0, 0);
    }

    public void Show()
    {
        this.gameObject.transform.localScale = _CurScale;
    }

    /// <summary>
    /// Capture the number of cameras in the scene and load them into the top camera canvas
    /// </summary>
    public void InstantiateCameraPrefabs()
    {
        DestroyCameraPrefabs();
        List<RadialItem> curRadialList = new List<RadialItem>();
        foreach (var stream in _VideoController.Intent.Streams)
        {
            Vector3 newPos = new Vector3(0, 0, 0);
            GameObject curCamera = Instantiate(CameraPrefab, newPos, Quaternion.identity) as GameObject;
            curCamera.transform.parent = CameraPrefabContainer.transform;
            curCamera.transform.localRotation = Quaternion.identity;
            curCamera.transform.localPosition = newPos;
            curCamera.transform.localScale = ButtonScale;

            BtnRadialCameraSwitch camSelection = curCamera.GetComponent<BtnRadialCameraSwitch>();
            if (stream != null)
            {
                camSelection.MediaURL = stream.Url;
                camSelection.SetLabel(stream.Label);
            }

            RadialItem curRadialItem = curCamera.GetComponent<RadialItem>();
            curRadialList.Add(curRadialItem);
        }
        RadialCameraIcons.SetBtnScale(ButtonScale);
        RadialCameraIcons.ResetRadialItemPositions(curRadialList);
    }

    void DestroyCameraPrefabs()
    {
        foreach (Transform child in CameraPrefabContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Disables button then re-enables after a set amount of time
    /// </summary>
    /// <param name="btnController">Button controller.</param>
    IEnumerator DisableThenEnableBtn(ButtonController btnController)
    {
        btnController.Disable();
        yield return new WaitForSeconds(2.0f);
        btnController.Enable();
    }

    /// <summary>
    /// if we are live video, what should we do when a user tries to re-enter the live stream
    /// </summary>
    /// <param name="isLive">If set to <c>true</c> is live.</param>
    void SetControlsToLive(bool isLive)
    {
        TextLive.gameObject.SetActive(isLive);
        if (isLive)
        {
            RadialVideoControls.ResetRadialItemPositions(LiveIcons, true);
        }
        else
        {
            RadialVideoControls.ResetRadialItemPositions(NonLiveIcons, true);
        }
    }

    /// <summary>
    /// check to see if we have a produced feed - if not we need to remove that button from list
    /// </summary>
    void SetProducedFeedUI()
    {
        bool removeProducedFeed = false;
        if (_VideoController.Intent.ProducedFeed == null)
        {
            removeProducedFeed = true;
        }
        else
        {
            if (String.IsNullOrEmpty(_VideoController.Intent.ProducedFeed.Url))
            {
                removeProducedFeed = true;
            }
        }
        //we need to remove
        if (removeProducedFeed)
        {
            if (IntelProducedBtnContainer != null)
            {
                //TODO: try/catch to prevent errors here
                LiveIcons.Remove(IntelProducedBtnContainer);
                VODIcons.Remove(IntelProducedBtnContainer);
                NonLiveIcons.Remove(IntelProducedBtnContainer);
            }
        }
        else
        {
            if (IntelProducedBtn)
            {
                IntelProducedBtn.MediaURL = _VideoController.Intent.ProducedFeed.Url;
                UserProducedBtn.gameObject.SetActive(false);
            }
        }
    }


    /// <summary>
    /// Hide controls based on live
    /// </summary>
    public void SetPlayerControls()
    {
        //do we need to remove our produced feed button?
        SetProducedFeedUI();

        //need to know what icons the radial menu should display
        if (_VideoController.Intent.IsLive)
        {
            RadialVideoControls.ResetRadialItemPositions(LiveIcons);
            TextTimecode.gameObject.SetActive(false);
            TextLive.gameObject.SetActive(true);
            RadialSlider.gameObject.SetActive(false);
        }
        else
        {
            RadialVideoControls.ResetRadialItemPositions(VODIcons);
            TextTimecode.gameObject.SetActive(true);
            TextLive.gameObject.SetActive(false);
            RadialSlider.gameObject.SetActive(true);
        }
    }

    //************
    // EVENTS
    //************
    void OnEnable()
    {
        ScrubberInteractible.OnClick += OnScrubberClick;
        _BtnPauseButtonController.OnButtonClicked += OnPauseBtnClick;

        EventManager.OnSwitchCameraEvent += OnCameraSwitchHandler;
        EventManager.OnPanelOpenComplete += OnPanelOpenComplete;
        EventManager.OnPanelCloseComplete += OnPanelCloseComplete;
    }

    void OnDisable()
    {
        ScrubberInteractible.OnClick -= OnScrubberClick;
        _BtnPauseButtonController.OnButtonClicked -= OnPauseBtnClick;
        EventManager.OnSwitchCameraEvent -= OnCameraSwitchHandler;
        EventManager.OnPanelOpenComplete -= OnPanelOpenComplete;
        EventManager.OnPanelCloseComplete -= OnPanelCloseComplete;
    }

    void OnPanelOpenComplete()
    {
        PlayDialCover.SetActive(true);
    }

    void OnPanelCloseComplete()
    {
        PlayDialCover.SetActive(false);
    }

    void OnCameraSwitchHandler(string cameraURL)
    {
        if (String.IsNullOrEmpty(_previousCameraURL))
        {
            _previousCameraURL = cameraURL;
        }
        //hold our url
        UserProducedBtn.MediaURL = _previousCameraURL;
        if (cameraURL != IntelProducedBtn.MediaURL)
        {
            //we are not currently the intel produced feed
            _previousCameraURL = cameraURL;
            UserProducedBtn.gameObject.SetActive(false);
            IntelProducedBtn.gameObject.SetActive(true);
        }
        else
        {
            //we are the intel produced feed
            UserProducedBtn.gameObject.SetActive(true);
            IntelProducedBtn.gameObject.SetActive(false);
        }
    }

    public void SendToLive()
    {
        if (_VideoController.Intent.IsLive)
        {
            SetControlsToLive(true);
        }
    }

    void OnPauseBtnClick(object sender)
    {
        //if we're live and paused, we need to show our islive btn to allow user to refresh
        if (_VideoController.Intent.IsLive && TextLive.IsActive())
        {
            //no longer live - but in live stream
            SetControlsToLive(false);
        }
    }

    void OnScrubberClick()
    {
        if (!_VideoController.Intent.IsLive)
        {
            RaycastHit hit;
            //Vector2 localPosition;
            Ray ray = new Ray(UICamera.transform.position, UICamera.transform.forward);
            if (Physics.Raycast(ray, out hit, 100.0f, ScrubberLayerMask))
            {

                Vector3 newPoint = _scrubberInteractibleRect.transform.InverseTransformPoint(hit.point);
                float angle = Mathf.Atan2(newPoint.x, newPoint.y) * Mathf.Rad2Deg;
                if (angle < 0)
                {
                    angle = 360 + angle;
                }
                angle %= 360;

                float percentComplete = angle / 360;

                percentComplete = Mathf.Max(0, percentComplete);
                percentComplete = Mathf.Min(percentComplete, 1.0f);
                _VideoController.SeekToPercentageTime(percentComplete);

                #region Analytics Call
                Analytics.CustomEvent("VideoSliderClicked", new Dictionary<string, object> {
                    { "SliderPercentageClicked",  percentComplete }
                });
                #endregion
            }
        }
    }

    public void OnPlayVideo()
    {
        BtnPlay.SetActive(false);
        BtnPause.SetActive(true);
		if(this.gameObject.activeSelf)
        	StartCoroutine(DisableThenEnableBtn(_BtnPauseButtonController));
    }

    public void OnPauseVideo()
    {
        BtnPlay.SetActive(true);
        BtnPause.SetActive(false);
		if(this.gameObject.activeSelf)
        	StartCoroutine(DisableThenEnableBtn(_playBtnController));
    }

    public void OnStopVideo()
    {
        BtnPlay.SetActive(true);
        BtnPause.SetActive(false);
    }

    public void OnHighlightsChanged()
    {
        // if we only want to allow live events - note, however, this will get called immediately (before _videoController is defined)
        //		if (_VideoController.Intent != null)
        //		{
        //			if (_VideoController.Intent.IsLive)
        //			{
        DataCursor cursor = HighlightsProviderObj.GetHighlights();
        //only if we actually have some highlights
        if (cursor.CurrentItems.Count > 0)
        {
            HighlightsButtonController.Enable();
            _IsHighlightUpdated = true;
            HighlightMainGlow.AnimScaleUp();
            _AudioController.PlayAudio(AudioController.AudioClips.Notification);
        }
        else
        {
            HighlightsButtonController.Disable();
        }
        //			}
        //		}
    }
}
