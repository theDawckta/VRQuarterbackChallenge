using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class BtnPauseHandler : MonoBehaviour
{
    private Interactible _Interactible;
    private VideoController _VideoController;
	private AudioController _AudioController;

    void Awake()
    {
        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
            throw new Exception("An _Interactible must exist on this button.");

        _VideoController = Extensions.GetRequired<VideoController>();
		_AudioController = Extensions.GetRequired<AudioController>();
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnPauseBtnClick;
        _Interactible.Out();
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnPauseBtnClick;
        _Interactible.Out();
    }

    void OnPauseBtnClick()
    {
		_AudioController.PlayAudio(AudioController.AudioClips.SmallClick);
		_VideoController.Pause();

        #region Analytics Call
        Analytics.CustomEvent("PauseButtonClicked", new Dictionary<string, object>
        {
            { "Operation", "Pause" }
        });
        #endregion
    }
}