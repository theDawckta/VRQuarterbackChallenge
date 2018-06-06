using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using TMPro;

public class BtnCameraSelectionHandler : MonoBehaviour
{
    public string MediaURL;
	public TextMeshProUGUI TextElement;
    public GameObject CameraNormal;
    public GameObject CameraActive;
    public GameObject CameraSelected;

    private BtnAnimations _BtnAnimation;
    private Interactible _Interactible;
    private VideoController _VideoController;
	private AudioController _AudioController;
	private CameraBtn _CameraBtn;

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
		_AudioController = Extensions.GetRequired<AudioController>();
		_CameraBtn = this.gameObject.GetComponent<CameraBtn> ();
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnCameraBtnClick;
        EventManager.OnSwitchCameraEvent += OnCameraSwitchHandler;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnCameraBtnClick;
        EventManager.OnSwitchCameraEvent -= OnCameraSwitchHandler;
    }

    public void SetLabel(string label)
    {
        TextElement.text = label;
    }

    void OnCameraBtnClick()
    {
		_AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
		if (_VideoController.GetMoviePath() != MediaURL)
		{
			//dispatch event here to change to this camera
			EventManager.Instance.CameraSwitchEvent(MediaURL);
		}

        #region Analytics Call
        Analytics.CustomEvent("CameraSwitch", new Dictionary<string, object>
        {
            { "MediaUrl", MediaURL },
            { "CameraName", TextElement.text }
        });
        #endregion

    }

	void OnCameraSwitchHandler(string cameraUrl)
    {
        //_VideoController.LinearVideoController.previouslySelectedFeed = cameraUrl;

        if (cameraUrl == MediaURL)
        {
            if (_BtnAnimation != null)
            	_BtnAnimation.IsActive = false;
			if (_CameraBtn != null)
				_CameraBtn.IsActive = true;
			
            CameraSelected.SetActive(true);
            CameraActive.SetActive(false);
            HighlightOn();
        }
        else
        {
            if (_BtnAnimation != null)
            	_BtnAnimation.IsActive = true;
			if (_CameraBtn != null)
				_CameraBtn.IsActive = false;
			
            CameraSelected.SetActive(false);
            CameraActive.SetActive(true);
            HighlightOff();
        }
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