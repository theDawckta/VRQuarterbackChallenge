using System;
using UnityEngine;
using System.Collections;

public class BtnFeedbackHandler : MonoBehaviour {

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
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnFeedbackBtnClick;
        EventManager.OnEnableUserClick += OnEnableUserClick;
        EventManager.OnDisableUserClick += OnDisableUserClick;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnFeedbackBtnClick;
        EventManager.OnEnableUserClick -= OnEnableUserClick;
        EventManager.OnDisableUserClick -= OnDisableUserClick;
    }

    public void Enable()
    {
        if (_Interactible != null)
        {
            _Interactible.Enable();
        }
        _CanvasGroup.alpha = 1.0f;
    }

    public void Disable()
    {
        if (_Interactible != null)
        {
            _Interactible.Disable();
        }
        if (_CanvasGroup != null)
        {
            _CanvasGroup.alpha = _DisabledAlpha;
        }
    }

    void OnFeedbackBtnClick()
    {
        Debug.Log("Feedback button clicked");
        _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
        EventManager.Instance.FeedbackBtnClickEvent();
        FeedbackSystemController.Instance.TriggerGlobalFeedback();
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
