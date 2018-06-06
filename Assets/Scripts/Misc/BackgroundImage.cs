using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BackgroundImage : MonoBehaviour {

	private DynamicTexture DynamicTexture;
	private string _CurBackgroundImg;

	void Awake()
	{
		DynamicTexture = this.gameObject.GetComponent<DynamicTexture> ();
		if (DynamicTexture == null)
			throw new Exception ("You must define a DynamicTexture for the Background image.");
	}

	void EventManager_OnBackgroundImageTrigger (string backgroundImgStr)
	{
		if (!string.IsNullOrEmpty (backgroundImgStr))
		{
			if (_CurBackgroundImg != backgroundImgStr)
			{
				_CurBackgroundImg = backgroundImgStr;
				DynamicTexture.AnimOut (_CurBackgroundImg);
			} else
			{
				//Debug.Log ("do not change background image, none set or same");
			}
		}
	}

	void OnEnable()
	{
		EventManager.OnBackgroundImageTrigger += EventManager_OnBackgroundImageTrigger;
	}
		
	void OnDisable()
	{
		EventManager.OnBackgroundImageTrigger -= EventManager_OnBackgroundImageTrigger;
	}
}
