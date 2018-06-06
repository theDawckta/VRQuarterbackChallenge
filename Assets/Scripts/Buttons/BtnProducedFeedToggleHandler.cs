using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class BtnProducedFeedToggleHandler : MonoBehaviour
{
    public string MediaURL;
    public Text TextElement;
    public GameObject CameraNormal;
    public GameObject CameraActive;
    public GameObject CameraSelected;

    //public LinearVideoController.FeedState feedType;

    private BtnAnimations _BtnAnimation;
    private Interactible _Interactible;
    private VideoController _VideoController;    

    void Awake()
    {
        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
        {
            throw new Exception("An _Interactible must exist on this button.");
        }
        if (TextElement == null)
        {
            throw new Exception("A TextElement must exist on this button.");
        }

        _BtnAnimation = gameObject.GetComponent<BtnAnimations>();
        _VideoController = Extensions.GetRequired<VideoController>();     
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnCameraBtnClick;
        EventManager.OnSwitchCameraEvent += OnFeedSwitchHandler;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnCameraBtnClick;
        EventManager.OnSwitchCameraEvent -= OnFeedSwitchHandler;
    }

    public void SetLabel(string label)
    {
        TextElement.text = label;
    }

    void OnCameraBtnClick()
    {
        #region Analytics Call
        Analytics.CustomEvent("CameraSwitch", new Dictionary<string, object>
        {
            { "MediaUrl", MediaURL },
            { "CameraName", TextElement.text }
        });
        #endregion

        //if (this.feedType != _VideoController.LinearVideoController.currentFeedState)
        //{
        //    //Switching Feed state
        //    //_VideoController.LinearVideoController.ToggleFeedState(this.feedType);

        //    if (this.feedType == LinearVideoController.FeedState.Auto)
        //    {
        //        //dispatch event to switch to produced feed
        //        EventManager.Instance.CameraSwitchEvent(MediaURL);
        //    }
        //    else
        //    {
        //        //dispacth event ot switch to previously selected manual feed
        //        EventManager.Instance.CameraSwitchEvent(_VideoController.LinearVideoController.previouslySelectedFeed);
        //    }
        //}
    }

	void OnFeedSwitchHandler(string cameraUrl)
    {
        //This is called on any valid Camera switch 
        //But this script only handles the Toggle scenario

        

        //if (cameraUrl == MediaURL)
        //{
        //    if (_BtnAnimation != null)
        //    {
        //        _BtnAnimation.IsActive = false;
        //    }
        //    CameraSelected.SetActive(true);
        //    CameraActive.SetActive(false);
        //    HighlightOn();
        //}
        //else
        //{
        //    if (_BtnAnimation != null)
        //    {
        //        _BtnAnimation.IsActive = true;
        //    }
        //    CameraSelected.SetActive(false);
        //    CameraActive.SetActive(true);
        //    HighlightOff();
        //}
    }

    public void HighlightOn()
    {
        _Interactible.Disable();
        CameraNormal.SetActive(false);
    }

    public void HighlightOff()
    {
        CameraNormal.SetActive(true);
        _Interactible.Enable();
    }
}