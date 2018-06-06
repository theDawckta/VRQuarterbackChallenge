using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class BtnBackHandler : MonoBehaviour
{
    private Interactible _Interactible;
    private CanvasGroup _CanvasGroup;
    private float _DisabledAlpha = 0.3f;
    private AudioController _AudioController;
    private bool _IsHardwareClickAllowed = true;
    private float _ButtonDownStartTime = -1.0f;
	[HideInInspector]
	public bool IsBackBtnClose = false;

    void Awake()
    {
        _CanvasGroup = GetComponent<CanvasGroup>();
        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
        {
            throw new Exception("An _Interactible must exist on this button.");
        }
        _AudioController = Extensions.GetRequired<AudioController>();

		if (GlobalVars.Instance.IsDaydream) 
		{
			GvrDaydreamApi.Create ();
		}
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnBackBtnClick;
        EventManager.OnEnableUserClick += OnEnableUserClick;
        EventManager.OnDisableUserClick += OnDisableUserClick;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnBackBtnClick;
        EventManager.OnEnableUserClick -= OnEnableUserClick;
        EventManager.OnDisableUserClick -= OnDisableUserClick;
    }
		
    void Update()
    {
		if (GlobalVars.Instance.IsDaydream) //DAYDREAM EXIT BUTTON	
		{
			if (Input.GetKeyDown (KeyCode.Escape))
			{
				Application.Quit ();
				//GvrDaydreamApi.LaunchVrHome ();
			}
		}
		else //GEAR VR HARDWARE BACK BUTTON
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				_ButtonDownStartTime = Time.realtimeSinceStartup;
			}
			if (Input.GetKey(KeyCode.Escape))
			{
				float backPressTime = Time.realtimeSinceStartup - _ButtonDownStartTime;
				if (backPressTime > 0.75)
				{
					QuitToGlobalMenu();
				}
			}
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				if (_IsHardwareClickAllowed)
				{
					EventManager.Instance.BackBtnClickEvent();
				}
			}
		}
    }

	void OnApplicationPause( bool paused ) 
	{
		if (GlobalVars.Instance.IsDaydream && paused)
		{
			GvrDaydreamApi.LaunchVrHome ();
		}
	}

    public void Enable()
    {
		if (_Interactible != null)
		{
			_Interactible.Enable ();
		}
		_CanvasGroup.alpha = 1.0f;
    }

    public void Disable()
    {
		if (_Interactible != null)
		{
			_Interactible.Disable ();
		}
		if (_CanvasGroup != null)
        {
            _CanvasGroup.alpha = _DisabledAlpha;
        }
    }

    void OnBackBtnClick()
    {
        #region Analytics Call
        Analytics.CustomEvent("BackButtonClicked", new Dictionary<string, object>
        {
            { "Operation", "GoBack" }
        });
        #endregion

        _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
		if (IsBackBtnClose)
		{
			CloseApp ();
		} else
		{
			EventManager.Instance.BackBtnClickEvent ();
		}
    }

	private void CloseApp()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_STANDALONE
		Application.Quit();
		#elif UNITY_ANDROID
		if (GlobalVars.Instance.IsDaydream) GvrDaydreamApi.LaunchVrHome ();
		else OVRManager.instance.ReturnToLauncher();
		#endif
	}

    /// <summary>
    /// On long press calls Global Menu(Universal menu)
    /// </summary>
    private void QuitToGlobalMenu()
    {
        OVRManager.PlatformUIGlobalMenu();
    }

    /// <summary>
    /// when traversing between scenes, turn on/off allowing hardware clicks
    /// </summary>
    void OnDisableUserClick()
    {
        _IsHardwareClickAllowed = false;
    }

    void OnEnableUserClick()
    {
        _IsHardwareClickAllowed = true;
    }
}