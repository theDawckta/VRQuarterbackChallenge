using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;


public class BtnRadialCameraSwitch : MonoBehaviour {

	public string MediaURL;
	public Text TextElement;
	public ButtonController VideoButtonController;
	public GameObject CameraNormal;
	public GameObject CameraActive;
	public GameObject CameraSelected;
	public GameObject IconNormal;
	public GameObject IconSelected;

	private VideoController _VideoController;

	void Awake()
	{
		_VideoController = Extensions.GetRequired<VideoController>();
		if(TextElement != null) 
			TextElement.gameObject.SetActive (false);
	}

	public void SetLabel(string label)
	{
		TextElement.text = label;
	}

	void OnCameraBtnClick(object sender)
	{
		//need to track Intel produced camera differently?
		if (TextElement != null)
		{
			#region Analytics Call
			Analytics.CustomEvent ("CameraSwitch", new Dictionary<string, object> {
				{ "MediaUrl", MediaURL },
				{ "CameraName", TextElement.text }
			});
			#endregion
		}
		if (_VideoController.GetMoviePath() != MediaURL)
		{
			//dispatch event here to change to this camera
			EventManager.Instance.CameraSwitchEvent(MediaURL);
		}
	}

	void OnCameraBtnOver(object sender)
	{
		TextElement.gameObject.SetActive (true);
		IconNormal.SetActive (false);
	}

	void OnCameraBtnOut(object sender)
	{
		TextElement.gameObject.SetActive (false);
		IconNormal.SetActive (true);
	}

	void OnEnable()
	{
		VideoButtonController.OnButtonClicked += OnCameraBtnClick;
		if (TextElement != null)
		{
			VideoButtonController.OnButtonOver += OnCameraBtnOver;
			VideoButtonController.OnButtonOut += OnCameraBtnOut;
		}
		if (CameraSelected != null)
		{
			EventManager.OnSwitchCameraEvent += OnCameraSwitchHandler;
		}
	}

	void OnDisable()
	{
		VideoButtonController.OnButtonClicked -= OnCameraBtnClick;
		if (TextElement != null)
		{
			VideoButtonController.OnButtonOver -= OnCameraBtnOver;
			VideoButtonController.OnButtonOut -= OnCameraBtnOut;
		}
		if (CameraSelected != null)
		{
			EventManager.OnSwitchCameraEvent -= OnCameraSwitchHandler;
		}
	}

	void OnCameraSwitchHandler(string cameraUrl)
	{
		if (cameraUrl == MediaURL)
		{
			HighlightOn();
		}
		else
		{
			HighlightOff();
		}
	}

	public void HighlightOn()
	{
		VideoButtonController.Disable ();
		CameraSelected.SetActive(true);
		CameraActive.SetActive(false);
		CameraNormal.SetActive(false);
		IconNormal.SetActive (false);
		IconSelected.SetActive (true);
		TextElement.gameObject.SetActive (false);
	}

	public void HighlightOff()
	{
		CameraSelected.SetActive(false);
		CameraActive.SetActive(false);
		CameraNormal.SetActive(true);
		IconNormal.SetActive (true);
		IconSelected.SetActive (false);
		VideoButtonController.Enable ();
		TextElement.gameObject.SetActive (false);
	}

}
