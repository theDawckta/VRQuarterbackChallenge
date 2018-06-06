using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class BtnHomeHandler : MonoBehaviour
{
    private Interactible _Interactible;
    private CanvasGroup _CanvasGroup;
    private float _DisabledAlpha = 0.3f;
    private AudioController _AudioController;

    void Awake()
    {
        _CanvasGroup = GetComponent<CanvasGroup>();
        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
            throw new Exception("An _Interactible must exist on this button.");

        _AudioController = Extensions.GetRequired<AudioController>();
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnHomeBtnClick;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnHomeBtnClick;
    }

    void OnHomeBtnClick()
    {
        #region Analytics Call
        Analytics.CustomEvent("HomeButtonClicked", new Dictionary<string, object>
        {
            { "Operation", "GoHome" }
        });
        #endregion

        _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
        EventManager.Instance.HomeBtnClickEvent();
		EventManager.Instance.ChannelTriggerEvent();
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
}