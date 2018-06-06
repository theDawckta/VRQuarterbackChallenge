using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using VOKE.VokeApp.DataModel;

public class LinearVideoController : MonoBehaviour {

	/*#region TEXT
	public Text TextLiveBroadcast;
	public Text TextTimecode;
	#endregion*/
	#region TMPro
	public TextMeshProUGUI TextLiveBroadcast;
	public TextMeshProUGUI TextTimecode;
	#endregion

	public Canvas CanvasUIControls;
	public GameObject BtnJumpBack;
	public GameObject BtnJumpForward;
	public GameObject BtnPlay;
	public GameObject BtnPause;
	public GameObject BtnIsLive;
	public GameObject BtnRelatedContent;
	public GameObject VideoSlider;
	public GameObject LiveImage;

	[Header("Camera Layouts")]
	public GameObject CameraPrefab;
	public GameObject CameraPrefabContainer;
	public GameObject UniqueCameraPrefabContainer;
	public CameraUniqueLayout CameraUniqueLayout;

    [Header("Produced Feed Objects")]
    public BtnCameraSelectionHandler VRCastButton;
    public Text feedStateTxt;

    [Header("Highlights")]
    public ButtonController HighlightsButtonController;
    public HighlightGlow HighlightButtonGlow;
    public HighlightsProvider HighlightsProviderObj;

    //public enum FeedState { Manual, Auto };
    //[HideInInspector]
    //public FeedState currentFeedState;
    //public string previouslySelectedFeed;

    private VideoController _VideoController;
    private AudioController _AudioController;
	private float _LinearCameraSpacing = 85.0f;
	private Interactible _BtnPauseInteractible;
	private CanvasGroup _CanvasGroupUIControls;
	private Vector3 _CurScale = new Vector3(1,1,1);
	//we need multiple positions for where our related content button should be if we are live or not
	private Vector3 _btnRelatedContentNoLive;
	private Vector3 _btnRelatedContentIsLive;
	private float _btnRelatedPosShift = 0.26f;
    private bool _IsHighlightUpdated = false;       //do we need to reflect this highlight in our lower button

	private ContentViewModel Intent;
	private DataCursor _cursor;
	private bool _InitialLoad = true;				//track if this is the initial instance load

    private GameObject cameraIconsContainer;

    void Awake()
	{
		CheckRequired(BtnJumpBack, "BtnJumpBack");
		CheckRequired(BtnJumpForward, "BtnJumpForward");
		CheckRequired(BtnPlay, "BtnPlay");
		CheckRequired(BtnPause, "BtnPause");
		CheckRequired(BtnIsLive, "BtnIsLive");
		CheckRequired(BtnRelatedContent, "BtnRelatedContent");
		CheckRequired(UniqueCameraPrefabContainer, "UniqueCameraPrefabContainer");
		CheckRequired(CameraUniqueLayout, "CameraUniqueLayout");
		CheckRequired(CameraPrefabContainer, "CameraPrefabContainer");
		CheckRequired(CameraPrefab, "CameraPrefab");
		CheckRequired(VideoSlider, "VideoSlider");
		CheckRequired(TextLiveBroadcast, "TextLiveBroadcast");
		CheckRequired(LiveImage, "LiveImage");
		CheckRequired(TextTimecode, "TextTimecode");

		//Hide ();

		_CanvasGroupUIControls = CanvasUIControls.GetComponent<CanvasGroup>();
		CheckRequired(_CanvasGroupUIControls, "CanvasUIControls");
		_BtnPauseInteractible = BtnPause.GetComponent<Interactible>();
		_VideoController = Extensions.GetRequired<VideoController>();
        _AudioController = Extensions.GetRequired<AudioController>();
		CameraUniqueLayout.gameObject.SetActive(false);
		CanvasUIControls.gameObject.SetActive(false);

		_btnRelatedContentIsLive = BtnRelatedContent.transform.localPosition;
		_btnRelatedContentNoLive = new Vector3 (_btnRelatedContentIsLive.x + _btnRelatedPosShift,_btnRelatedContentIsLive.y,_btnRelatedContentIsLive.z);

        
	}

	IEnumerator Start()
    {
        if (!HighlightsProviderObj.HasHighlights)
        {
            HighlightsButtonController.Button.gameObject.SetActive(false);
            //HighlightsButtonController.Disable();
            //HighlightsButtonController.Button.GetComponent<Collider>().enabled = false; //Added since button was not being deactivated at the beginning
        }

		yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);
		// Get the session data transferred from the previous scene
		Intent = _cursor.CurrentVideo;

		if (Intent == null)
		{
			yield break;
		}

		Show();
		SetPlayerControls();
		//instantiate camera prefabs if needed
		if (Intent.Streams.Count >= 2)
		{
			InstantiateCameraPrefabs ();
		} else
		{
			//there is only one camera and we are not a camera layout list, allow camera pivot
			if(Intent.CameraLayoutList.Count == 0)
				StartCoroutine(AllowCameraPivot());
		}
    }

    private void CheckRequired(object thing, string name)
	{
		if (thing == null)
			throw new Exception(String.Format("A {0} is required to run this scene.", name));
	}


	//**************
	// METHODS
	//**************
	/// <summary>
	/// if we do not have camera navigation, we should allow the camera to rotate
	/// </summary>
	IEnumerator AllowCameraPivot()
	{
		yield return new WaitForEndOfFrame();
		EventManager.Instance.AllowCameraPivotEvent();
	}

	void DestroyCameraPrefabs()
	{
		foreach (Transform child in CameraPrefabContainer.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		foreach (Transform child in UniqueCameraPrefabContainer.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
	}

	/// <summary>
	/// check to see if our previous camera ID exists in our new list
	/// </summary>
	/// <returns><c>true</c>, if identifier exist was doesed, <c>false</c> otherwise.</returns>
	/// <param name="currentActiveCameraLayout">Boolean - ID to check for</param>
	bool DoesIDExist(string currentActiveCameraLayout)
	{
		//if we are on a hole that no longer exists, we need to push us to either the first hole, or a VR Cast hole
		foreach (var cameraLayout in _cursor.CurrentVideo.CameraLayoutList)
		{
			if (currentActiveCameraLayout == cameraLayout.ID)
				return true;
		}
		return false;
	}

	IEnumerator FadeUpControls(CanvasGroup canvasGroup, float fadeSpeed = 2.0f)
	{
		while (canvasGroup.alpha < 1f)
		{
			canvasGroup.alpha += fadeSpeed * Time.deltaTime;
			yield return null;
		}
	}

	void Hide()
	{
		_CurScale = this.gameObject.transform.localScale;
		this.gameObject.transform.localScale = new Vector3 (0, 0, 0);
	}

	/// <summary>
	/// Check to see if our cameraLayoutList has been updated
	/// </summary>
	/// <returns><c>true</c>, if lists are same, <c>false</c> otherwise.</returns>
	bool ListIsSame(IList<CameraLayoutUrl> a, IList<CameraLayoutUrl> b)
	{
		if (a == b)
			return true; // exact same instance

		if (a == null || b == null)
			return false;

		// could check more than just ids if necessary
		return a.Select(_ => _.ID).SequenceEqual(b.Select(_ => _.ID));
	}

	public void Show()
	{
		this.gameObject.transform.localScale = _CurScale;
	}

	/// <summary>
	/// Capture the number of cameras in the scene and load them into the top camera canvas
	/// </summary>
	void InstantiateCameraPrefabs()
	{
		//Destroy children if they exist
		DestroyCameraPrefabs();

		//check to see if a list of camera layouts is defined
		//if so, we need to allow transition between cameras
		if (Intent.CameraLayoutList.Count > 0)
		{
			SetCameraLayoutList ();
		}
		//check to see if there is a defined camera layout url (backwards compatibility cameralayout)
		else if (!String.IsNullOrEmpty(Intent.CameraLayout) || !String.IsNullOrEmpty(Intent.CameraLayoutUrl))
		{
			float cameraZPos = 0;
			CameraUniqueLayout.gameObject.SetActive(true);
  	        cameraIconsContainer = CameraUniqueLayout.gameObject;
			string textureUrl = (!String.IsNullOrEmpty(Intent.CameraLayout)) ? Intent.CameraLayout : Intent.CameraLayoutUrl;
			CameraUniqueLayout.SetTexture(textureUrl);
			var initCount = 0;
			foreach (var stream in Intent.Streams)
			{
				float curZVal = Convert.ToSingle (stream.OffsetZ);
				if (initCount == 0)
				{
					cameraZPos = curZVal;
				} else
				{
					if (curZVal < cameraZPos)
						cameraZPos = curZVal;
				}
				CameraUniqueLayout.SetCamera(stream);
				initCount++;
			}
			StartCoroutine (AllowCameraPivot ());
			CameraUniqueLayout.SetHitPosition (cameraZPos);
		}
		else
		{
			//set the width of our container to allow for adequate camera spacing
			float camSpacing = (_LinearCameraSpacing * Intent.Streams.Count);
			CameraPrefabContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(camSpacing, 100);
			cameraIconsContainer = CameraPrefabContainer;
			//use standard linear list
			foreach (var stream in Intent.Streams)
			{
				Vector3 newPos = new Vector3(0, 0, 0);
				GameObject curCamera = Instantiate(CameraPrefab, newPos, Quaternion.identity) as GameObject;
				curCamera.transform.parent = CameraPrefabContainer.transform;
				curCamera.transform.localRotation = Quaternion.identity;
				curCamera.transform.localPosition = newPos;
				curCamera.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

				BtnCameraSelectionHandler camSelection = curCamera.GetComponent<BtnCameraSelectionHandler>();
				if (stream != null)
				{
					camSelection.MediaURL = stream.Url;
					camSelection.SetLabel(stream.Label);
				}
			}
			StartCoroutine (AllowCameraPivot ());
		}
	}

	/// <summary>
	/// There are multiple camera layouts available, we need to provide navigation
	/// </summary>
	void SetCameraLayoutList()
	{
		CameraUniqueLayout.gameObject.SetActive(true);
		CameraUniqueLayout.ClearCameras ();
		foreach (var stream in Intent.Streams)
		{
			CameraUniqueLayout.SetCamera(stream);
		}
		CameraUniqueLayout.EnableCameraLayoutNavigation (Intent);
	}

	public void SetPlayerControls()
	{
        //do we need to remove our produced feed button?
        //SetProducedFeedUI();

        //is live only activated if behind stream
        BtnIsLive.SetActive(false);
		//BtnRelatedContent.SetActive (false);
		//turn off items in live mode
		if (Intent.IsLive)
		{
			//BtnRelatedContent.SetActive (false);
			BtnJumpBack.SetActive(false);
			BtnJumpForward.SetActive(false);
			VideoSlider.SetActive(false);
			LiveImage.SetActive(true);
			TextTimecode.gameObject.SetActive(false);
			BtnRelatedContent.transform.localPosition = _btnRelatedContentIsLive;
		}
		else
		{
			//BtnRelatedContent.SetActive (true);
			BtnJumpBack.SetActive(true);
			BtnJumpForward.SetActive(true);
			VideoSlider.SetActive(true);
			LiveImage.SetActive(false);
			TextTimecode.gameObject.SetActive(true);
			BtnRelatedContent.transform.localPosition = _btnRelatedContentNoLive;
		}

		_CanvasGroupUIControls.alpha = 0.2f;
		StartCoroutine(FadeUpControls(_CanvasGroupUIControls));

		CanvasUIControls.gameObject.SetActive(true);
	}

    /// <summary>
    /// Sets up the produced feed UI at the beginning
    /// </summary>
    void SetProducedFeedUI()
    {
        bool removeProducedFeed = false;
        if (Intent.ProducedFeed == null)
        {
            removeProducedFeed = true;
        }
        else
        {
            if (String.IsNullOrEmpty(Intent.ProducedFeed.Url))
            {
                removeProducedFeed = true;
            }
        }

        //If we need to remove
        if (removeProducedFeed)
        {
            //IntelProducedFeedButton.gameObject.SetActive(false);
            //ManualProducedFeedButton.gameObject.SetActive(false);
            VRCastButton.gameObject.SetActive(false);
            feedStateTxt.enabled = false;
        }
        else
        {
            //if (IntelProducedFeedButton!=null)            
            //    IntelProducedFeedButton.GetComponent<BtnProducedFeedToggleHandler>().MediaURL = _VideoController.Intent.ProducedFeed.Url;
            //ToggleFeedState(FeedState.Manual);
            VRCastButton.MediaURL = Intent.ProducedFeed.Url;
        }
    }

    /// <summary>
    /// Toggles the current Feed State and also hides/shows the selection cameras based on the state
    /// </summary>
    /// <param name="targetState"></param>
    //public void ToggleFeedState(FeedState targetState)
    //{
    //    switch (targetState)
    //    {
    //        case FeedState.Auto:
    //            IntelProducedFeedButton.CameraSelected.SetActive(true);
    //            IntelProducedFeedButton.CameraNormal.SetActive(false);
    //            IntelProducedFeedButton.CameraActive.SetActive(false);

    //            ManualProducedFeedButton.CameraSelected.SetActive(false);
    //            ManualProducedFeedButton.CameraNormal.SetActive(true);
    //            ManualProducedFeedButton.CameraActive.SetActive(false);

    //            _VideoController.LinearVideoController.CameraPrefabContainer.SetActive(false);
    //            _VideoController.LinearVideoController.UniqueCameraPrefabContainer.SetActive(false);
    //            currentFeedState = FeedState.Auto;
    //            feedStateTxt.text = "AUTO";
    //            break;

    //        case FeedState.Manual:
    //            ManualProducedFeedButton.CameraSelected.SetActive(true);
    //            ManualProducedFeedButton.CameraNormal.SetActive(false);
    //            ManualProducedFeedButton.CameraActive.SetActive(false);

    //            IntelProducedFeedButton.CameraSelected.SetActive(false);
    //            IntelProducedFeedButton.CameraNormal.SetActive(true);
    //            IntelProducedFeedButton.CameraActive.SetActive(false);

    //            _VideoController.LinearVideoController.CameraPrefabContainer.SetActive(true);
    //            _VideoController.LinearVideoController.UniqueCameraPrefabContainer.SetActive(true);
    //            currentFeedState = FeedState.Manual;
    //            feedStateTxt.text = "MANUAL";
    //            break;

    //        default:
    //            break;
    //    }
    //}

    //***************
    // EVENTS
    //***************
    void OnEnable()
	{
		_BtnPauseInteractible.OnClick += OnPauseBtnClick;
		EventManager.OnVideoEndEvent += OnVideoEndHandler;
		DataCursorComponent.Instance.Updated += OnInstanceUpdated;
	}

	void OnDisable()
	{
		_BtnPauseInteractible.OnClick -= OnPauseBtnClick;
		EventManager.OnVideoEndEvent -= OnVideoEndHandler;
		if (!DataCursorComponent.ApplicationIsQuitting)
			DataCursorComponent.Instance.Updated -= OnInstanceUpdated;
	}

	/// <summary>
	/// check for update of the XML file. Ignore on initial load
	/// </summary>

	void OnInstanceUpdated(object sender, ContentUpdatedEventArgs<DataCursor> e)
	{
		if (_InitialLoad)
		{
			_InitialLoad = false;
			return;
		}
		if (_cursor != null && ListIsSame (e.Content.CurrentVideo.CameraLayoutList, _cursor.CurrentVideo.CameraLayoutList))
		{
			return;
		}

		string currentActiveCameraLayout = CameraUniqueLayout.GetActiveLayoutID ();

		_cursor = e.Content;
		Intent = _cursor.CurrentVideo;

		//our list has changed, and we have a camera layout list
		//two scenarios:
		//1. User is on a camera that has disappeared - in this instance, refresh camera nav and cameras
		//2. New hole has been added but current hole still exists - add to nav list and add to cameras but do not move user along
		if (_cursor.CurrentVideo.CameraLayoutList.Count > 0)
		{
			bool doesIDExist = DoesIDExist (currentActiveCameraLayout);
			DestroyCameraPrefabs ();
			SetCameraLayoutList ();

			if (!doesIDExist)
			{
				CameraUniqueLayout.TriggerFirstOrVRCast ();
			}
		}
	}
		
	void OnVideoEndHandler()
	{
		BtnPlay.SetActive(false);
		BtnPause.SetActive(false);
		BtnJumpForward.SetActive (false);
		if(cameraIconsContainer != null)
        	cameraIconsContainer.SetActive(false);
	}

	void OnPauseBtnClick()
	{
		//if we're live and paused, we need to show our islive btn to allow user to refresh
		if (Intent.IsLive)
		{
			BtnIsLive.SetActive(true);
			LiveImage.SetActive(false);
			//BtnRelatedContent.transform.localPosition = _btnRelatedContentNoLive;
		}
	}

	public void OnPlayVideo()
	{
		BtnAnimations btnAnimations = BtnPlay.GetComponent<BtnAnimations> ();
		if (btnAnimations != null)
		{
			if (btnAnimations.BtnRollover != null)
				btnAnimations.BtnRollover.SetActive (false);

			if (btnAnimations.BtnNormal != null)
				btnAnimations.BtnNormal.SetActive (true);
		}

		BtnPlay.SetActive(false);
		BtnPause.SetActive(true);

		if (Intent != null)
		{
            if (!Intent.IsLive)
            {
                BtnJumpForward.SetActive(true);               
            }
		}

		if (cameraIconsContainer != null)
		{
			if (!cameraIconsContainer.activeSelf)
				cameraIconsContainer.SetActive (true);
		}
	}
		
	public void OnPauseVideo()
	{
		BtnPlay.SetActive(true);
		BtnPause.SetActive(false);
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
            HighlightsButtonController.Button.gameObject.SetActive(true);

            //HighlightsButtonController.Enable();
            //_IsHighlightUpdated = true;        
            HighlightButtonGlow.AnimScaleUp();   
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
