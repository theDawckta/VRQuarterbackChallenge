using System;
using System.Collections;
using UnityEngine;

public class BtnIsLiveHandler : MonoBehaviour
{
    private Interactible _Interactible;
    private VideoController _VideoController;
	private ButtonController _ButtonController;
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
			_Interactible.OnClick += OnIsLiveClick;
			_Interactible.Out();
		} else if (_ButtonController != null)
		{
			_ButtonController.OnButtonClicked += OnButtonControllerClick;
		}

        
    }

    void OnDisable()
    {
		if (_Interactible != null)
		{
			_Interactible.OnClick -= OnIsLiveClick;
			_Interactible.Out();
		} else if (_ButtonController != null)
		{
			_ButtonController.OnButtonClicked += OnButtonControllerClick;
		}
    }

	void OnButtonControllerClick(object sender)
	{
		_VideoController.SendToLive();
		_AudioController.PlayAudio(AudioController.AudioClips.SmallClick);
	}

    void OnIsLiveClick()
    {
        _VideoController.SendToLive();
		_AudioController.PlayAudio(AudioController.AudioClips.SmallClick);
		gameObject.SetActive(false);
    }
}