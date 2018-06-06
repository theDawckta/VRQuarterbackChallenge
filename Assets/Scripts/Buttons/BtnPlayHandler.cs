using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class BtnPlayHandler : MonoBehaviour
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
        _Interactible.OnClick += OnPlayBtnClick;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnPlayBtnClick;
        _Interactible.Out();
    }

    void OnPlayBtnClick()
    {
		_AudioController.PlayAudio(AudioController.AudioClips.SmallClick);
		_VideoController.Play();

        #region Analytics Call
        Analytics.CustomEvent("PlayButtonClicked", new Dictionary<string, object>
        {
            { "Operation", "Play" }
        });
        #endregion
    }
}