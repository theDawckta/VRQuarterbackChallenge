using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class BtnVideoJumpHandler : MonoBehaviour
{
    //either send a jump back or jump forward message to controller
    public bool IsJumpBack = true;

    private Interactible _Interactible;
	private ButtonController _ButtonController;
    private VideoController _VideoController;
	private AudioController _AudioController;

    void Awake()
    {
        _Interactible = GetComponent<Interactible>();
		_ButtonController = GetComponent<ButtonController> ();

		if (_Interactible == null && _ButtonController == null)
            throw new Exception("An _Interactible or ButtonController must exist on this button.");

        _VideoController = Extensions.GetRequired<VideoController>();
		_AudioController = Extensions.GetRequired<AudioController>();
    }

    void OnEnable()
    {
		if (_Interactible != null)
		{
			_Interactible.OnClick += OnJumpBtnClick;
		} else if (_ButtonController != null)
		{
			_ButtonController.OnButtonClicked += OnButtonControllerClick;
		}
        
    }

    void OnDisable()
    {
		if (_Interactible != null)
		{
			_Interactible.OnClick -= OnJumpBtnClick;
		} else if (_ButtonController != null)
		{
			_ButtonController.OnButtonClicked -= OnButtonControllerClick;
		}
    }

	void OnButtonControllerClick(object sender)
	{
		OnJumpBtnClick ();
	}

    void OnJumpBtnClick()
    {
        string operation;
		_AudioController.PlayAudio(AudioController.AudioClips.SmallClick);

        if (IsJumpBack)
        {
			bool isPlaying = _VideoController.IsPlaying;
			_VideoController.JumpBack (isPlaying);
            operation = "JumpBack";
        }
        else
        {
            _VideoController.JumpForward();
            operation = "JumpForward";
        }
		
        Analytics.CustomEvent("VideoJumpButtonClicked", new Dictionary<string, object>
        {
            { "Operation", operation }
        });
    }
}
